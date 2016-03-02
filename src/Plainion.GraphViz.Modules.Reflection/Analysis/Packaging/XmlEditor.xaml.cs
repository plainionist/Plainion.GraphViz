using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging
{
    public partial class XmlEditor : UserControl
    {
        private FoldingManager myFoldingManager;
        private XmlFoldingStrategy myFoldingStrategy;
        private CompletionWindow myCompletionWindow;

        internal XmlEditor()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register("Document", typeof(TextDocument), typeof(XmlEditor));

        public TextDocument Document
        {
            get { return (TextDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        public static readonly DependencyProperty CompletionDataProperty = DependencyProperty.Register("CompletionData", typeof(IEnumerable<KeywordCompletionData>), typeof(XmlEditor));

        public IEnumerable<KeywordCompletionData> CompletionData
        {
            get { return (IEnumerable<KeywordCompletionData>)GetValue(CompletionDataProperty); }
            set { SetValue(CompletionDataProperty, value); }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            myTextEditor.Options.IndentationSize = 4;
            myTextEditor.Options.ConvertTabsToSpaces = true;

            myTextEditor.Document.TextChanged += OnTextChanged;
            OnTextChanged(null, null);

            myTextEditor.TextArea.TextEntering += OnTextEntering;
            myTextEditor.TextArea.TextEntered += OnTextEntered;
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            if (myFoldingManager == null)
            {
                myFoldingManager = FoldingManager.Install(myTextEditor.TextArea);
                myFoldingStrategy = new XmlFoldingStrategy();
            }

            myFoldingStrategy.UpdateFoldings(myFoldingManager, myTextEditor.Document);
        }

        private void OnTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && myCompletionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    myCompletionWindow.CompletionList.RequestInsertion(e);
                }
            }

            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "<")
            {
                HandleKeywordCompletion();
            }
            else if (e.Text == ">")
            {
                HandleTagClosing();
            }
            else if (e.Text == " ")
            {
                HandlePropertyCompletion();
            }
        }

        private void HandleKeywordCompletion()
        {
            ShowCompletionWindow(CompletionData);
        }

        private void ShowCompletionWindow(IEnumerable<ICompletionData> competionItems)
        {
            myCompletionWindow = new CompletionWindow(myTextEditor.TextArea);

            var data = myCompletionWindow.CompletionList.CompletionData;
            foreach (var item in competionItems)
            {
                data.Add(item);
            }
            myCompletionWindow.Show();
            myCompletionWindow.Closed += delegate
            {
                myCompletionWindow = null;
            };
        }

        private void HandleTagClosing()
        {
            if (myTextEditor.Document.GetCharAt(myTextEditor.CaretOffset - 2) == '/')
            {
                return;
            }

            var currentLine = myTextEditor.Document.GetLineByOffset(myTextEditor.CaretOffset);
            var lastOpenedTagPos = myTextEditor.Document.LastIndexOf('<', currentLine.Offset, myTextEditor.CaretOffset - currentLine.Offset);
            var spaceAfterOpenedTagPos = myTextEditor.Document.IndexOf(' ', lastOpenedTagPos, myTextEditor.CaretOffset - lastOpenedTagPos);
            var xmlTag = myTextEditor.Document.Text.Substring(lastOpenedTagPos + 1, spaceAfterOpenedTagPos - lastOpenedTagPos - 1);

            var oldCaretOffset = myTextEditor.CaretOffset;

            myTextEditor.Document.Insert(myTextEditor.CaretOffset, "</" + xmlTag + ">");

            myTextEditor.CaretOffset = oldCaretOffset;
        }

        private void HandlePropertyCompletion()
        {
            var lastClosedTagPos = myTextEditor.Document.LastIndexOf('>', 0, myTextEditor.CaretOffset);
            if (lastClosedTagPos < 0)
            {
                lastClosedTagPos = 0;
            }

            var lastOpenedTagPos = myTextEditor.Document.LastIndexOf('<', lastClosedTagPos, myTextEditor.CaretOffset - lastClosedTagPos);
            if (lastOpenedTagPos < 0)
            {
                return;
            }

            var spaceAfterOpenedTagPos = myTextEditor.Document.IndexOf(' ', lastOpenedTagPos, myTextEditor.CaretOffset - lastOpenedTagPos);
            if (spaceAfterOpenedTagPos < 0)
            {
                return;
            }

            var xmlTag = myTextEditor.Document.Text.Substring(lastOpenedTagPos + 1, spaceAfterOpenedTagPos - lastOpenedTagPos - 1);
            var completionData = CompletionData.SingleOrDefault(d => d.Type.Name == xmlTag);
            if (completionData == null)
            {
                return;
            }

            var completionItems = completionData.Type.GetProperties()
                .Select(p => new PropertyCompletionData(p));
            ShowCompletionWindow(completionItems);
        }
    }
}
