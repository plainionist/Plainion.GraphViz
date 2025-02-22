﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Search;

namespace Plainion.Windows.Editors.Xml
{
    public partial class XmlEditor : UserControl
    {
        private FoldingManager myFoldingManager;
        private XmlFoldingStrategy myFoldingStrategy;
        private CompletionWindow myCompletionWindow;

        public XmlEditor()
        {
            InitializeComponent();

            if( !DesignerProperties.GetIsInDesignMode( this ) )
            {
                Loaded += OnLoaded;
            }
        }

        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register( "Document", typeof( TextDocument ), typeof( XmlEditor ) );

        public TextDocument Document
        {
            get { return ( TextDocument )GetValue( DocumentProperty ); }
            set { SetValue( DocumentProperty, value ); }
        }

        public static readonly DependencyProperty CompletionDataProperty = DependencyProperty.Register( "CompletionData", typeof( IEnumerable<ElementCompletionData> ), typeof( XmlEditor ) );

        public IEnumerable<ElementCompletionData> CompletionData
        {
            get { return ( IEnumerable<ElementCompletionData> )GetValue( CompletionDataProperty ); }
            set { SetValue( CompletionDataProperty, value ); }
        }

        private void OnLoaded( object sender, RoutedEventArgs e )
        {
            myTextEditor.Options.IndentationSize = 4;
            myTextEditor.Options.ConvertTabsToSpaces = true;

            myTextEditor.Document.TextChanged += OnTextChanged;
            OnTextChanged( null, null );

            myTextEditor.TextArea.TextEntering += OnTextEntering;
            myTextEditor.TextArea.TextEntered += OnTextEntered;

            SearchPanel.Install(myTextEditor);
        }

        private void OnTextChanged( object sender, EventArgs e )
        {
            if( myFoldingManager == null )
            {
                myFoldingManager = FoldingManager.Install( myTextEditor.TextArea );
                myFoldingStrategy = new XmlFoldingStrategy();
            }

            if( !string.IsNullOrEmpty( myTextEditor.Document.Text ) )
            {
                myFoldingStrategy.UpdateFoldings( myFoldingManager, myTextEditor.Document );
            }
        }

        private void OnTextEntering( object sender, TextCompositionEventArgs e )
        {
            if( e.Text.Length > 0 && myCompletionWindow != null )
            {
                if( !char.IsLetterOrDigit( e.Text[ 0 ] ) )
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    myCompletionWindow.CompletionList.RequestInsertion( e );
                }
            }

            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void OnTextEntered( object sender, TextCompositionEventArgs e )
        {
            if( e.Text == "<" )
            {
                HandleTagCompletion();
            }
            else if( e.Text == ">" )
            {
                HandleTagClosing();
            }
            else if( e.Text == " " )
            {
                HandleAttributeCompletion();
            }
            else if( e.Text == "." )
            {
                HandlePropertyElementCompletion();
            }
        }

        private void HandleTagCompletion()
        {
            ShowCompletionWindow( CompletionData );
        }

        private void ShowCompletionWindow( IEnumerable<ICompletionData> competionItems )
        {
            myCompletionWindow = new CompletionWindow( myTextEditor.TextArea );

            var data = myCompletionWindow.CompletionList.CompletionData;
            foreach( var item in competionItems )
            {
                data.Add( item );
            }
            myCompletionWindow.Show();
            myCompletionWindow.Closed += delegate
            {
                myCompletionWindow = null;
            };
        }

        private void HandleTagClosing()
        {
            if( myTextEditor.Document.GetCharAt( myTextEditor.CaretOffset - 2 ) == '/' )
            {
                // empty tag
                // -> nothing to do
                return;
            }

            var currentLine = myTextEditor.Document.GetLineByOffset( myTextEditor.CaretOffset );
            var lastOpenedTagPos = myTextEditor.Document.LastIndexOf( '<', currentLine.Offset, myTextEditor.CaretOffset - currentLine.Offset );
            var spaceAfterOpenedTagPos = myTextEditor.Document.IndexOf( ' ', lastOpenedTagPos, myTextEditor.CaretOffset - lastOpenedTagPos );
            var xmlTagEndPos = spaceAfterOpenedTagPos == -1 ? myTextEditor.CaretOffset - 1 : spaceAfterOpenedTagPos;
            var xmlTag = myTextEditor.Document.Text.Substring( lastOpenedTagPos + 1, xmlTagEndPos - lastOpenedTagPos - 1 );

            var oldCaretOffset = myTextEditor.CaretOffset;

            myTextEditor.Document.Insert( myTextEditor.CaretOffset, "</" + xmlTag + ">" );

            myTextEditor.CaretOffset = oldCaretOffset;
        }

        private void HandleAttributeCompletion()
        {
            var lastClosedTagPos = myTextEditor.Document.LastIndexOf( '>', 0, myTextEditor.CaretOffset );
            if( lastClosedTagPos < 0 )
            {
                lastClosedTagPos = 0;
            }

            var lastOpenedTagPos = myTextEditor.Document.LastIndexOf( '<', lastClosedTagPos, myTextEditor.CaretOffset - lastClosedTagPos );
            if( lastOpenedTagPos < 0 )
            {
                return;
            }

            var spaceAfterOpenedTagPos = myTextEditor.Document.IndexOf( ' ', lastOpenedTagPos, myTextEditor.CaretOffset - lastOpenedTagPos );
            if( spaceAfterOpenedTagPos < 0 )
            {
                return;
            }

            var xmlTag = myTextEditor.Document.Text.Substring( lastOpenedTagPos + 1, spaceAfterOpenedTagPos - lastOpenedTagPos - 1 );
            var completionData = CompletionData.SingleOrDefault( d => d.Type.Name == xmlTag );
            if( completionData == null )
            {
                return;
            }

            var completionItems = completionData.Type.GetProperties()
                .Select( p => new AttributeCompletionData( p ) );
            ShowCompletionWindow( completionItems );
        }

        private void HandlePropertyElementCompletion()
        {
            var lastClosedTagPos = myTextEditor.Document.LastIndexOf( '>', 0, myTextEditor.CaretOffset );
            if( lastClosedTagPos < 0 )
            {
                lastClosedTagPos = 0;
            }

            var lastOpenedTagPos = myTextEditor.Document.LastIndexOf( '<', lastClosedTagPos, myTextEditor.CaretOffset - lastClosedTagPos );
            if( lastOpenedTagPos < 0 )
            {
                return;
            }

            var spaceAfterOpenedTagPos = myTextEditor.Document.IndexOf( ' ', lastOpenedTagPos, myTextEditor.CaretOffset - lastOpenedTagPos );
            if( spaceAfterOpenedTagPos !=-1 )
            {
                // no xml property completion if attributes already started
                return;
            }

            var xmlTag = myTextEditor.Document.Text.Substring( lastOpenedTagPos + 1, myTextEditor.CaretOffset - lastOpenedTagPos - 2 );
            var completionData = CompletionData.SingleOrDefault( d => d.Type.Name == xmlTag );
            if( completionData == null )
            {
                return;
            }

            var completionItems = completionData.Type.GetProperties()
                .Select( p => new PropertyElementCompletionData( p ) );
            ShowCompletionWindow( completionItems );
        }
    }
}
