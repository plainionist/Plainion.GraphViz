using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Configuration;
using Plainion.GraphViz.Viewer.Services;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Plainion.Prism.Mvvm;

namespace Plainion.GraphViz.Viewer.ViewModels
{
    [Export(typeof(SettingsEditorModel))]
    public class SettingsEditorModel : ViewModelBase, IInteractionRequestAware
    {
        private ConfigurationService myConfigService;
        private string myMatchingText;
        private string myReplacementText;
        private bool myAddConversionEnabled;
        private ILabelConversionStep mySelectedConversionStep;

        [ImportingConstructor]
        public SettingsEditorModel(ConfigurationService configService)
        {
            myConfigService = configService;
            myConfigService.ConfigChanged += myConfigService_ConfigChanged;

            // make a copy here in order to support cancel
            ConversionSteps = new ObservableCollection<ILabelConversionStep>(Config.LabelConversion);
            Labels = new ObservableCollection<LabelViewModel>();

            myAddConversionEnabled = false;

            CancelCommand = new DelegateCommand(OnCancel);
            OkCommand = new DelegateCommand(OnOk);
            AddConversionCommand = new DelegateCommand(OnAddConversion, () => myAddConversionEnabled);
            RemoveConversionStepCommand = new DelegateCommand(OnRemoveConversionStep);
            MouseDownCommand = new DelegateCommand<MouseButtonEventArgs>(OnMouseDown);
        }

        public ICommand MouseDownCommand
        {
            get;
            private set;
        }

        private void OnMouseDown(MouseButtonEventArgs args)
        {
            if (args.ClickCount == 2 && SelectedConversionStep != null)
            {
                var selectedItem = (RegexConversion)SelectedConversionStep;

                MatchingText = selectedItem.Matching;
                ReplacementText = selectedItem.Replacement;
            }
        }

        public ICommand RemoveConversionStepCommand
        {
            get;
            private set;
        }

        public DelegateCommand AddConversionCommand
        {
            get;
            private set;
        }

        private void OnAddConversion()
        {
            CommitTemporalLabels();

            MatchingText = null;
            ClearErrors("MatchingText");

            ReplacementText = null;
            ClearErrors("ReplacementText");
        }

        public string MatchingText
        {
            get { return myMatchingText; }
            set
            {
                if (myMatchingText == value)
                {
                    return;
                }

                myMatchingText = value;

                myAddConversionEnabled = false;
                AddConversionCommand.RaiseCanExecuteChanged();

                ClearErrors();

                try
                {
                    if (MatchingText.Length == 0)
                    {
                        ResetTemporalLabels();

                        return;
                    }

                    var regex = new Regex(myMatchingText, RegexOptions.IgnoreCase);

                    if (!Labels.Any(label => regex.IsMatch(label.Commited)))
                    {
                        SetError(ValidationFailure.Warning);
                        return;
                    }

                    ConvertLabelsTemporarily();
                }
                catch
                {
                    SetError(ValidationFailure.Error);
                }
                finally
                {
                    RaisePropertyChanged(nameof(MatchingText));
                }
            }
        }

        public string ReplacementText
        {
            get { return myReplacementText ?? string.Empty; }
            set
            {
                if (myReplacementText == value)
                {
                    return;
                }

                myReplacementText = value ?? string.Empty;

                myAddConversionEnabled = false;
                AddConversionCommand.RaiseCanExecuteChanged();

                ClearErrors();
                try
                {
                    ConvertLabelsTemporarily();
                }
                catch
                {
                    SetError(ValidationFailure.Error);
                }

                RaisePropertyChanged(nameof(ReplacementText));
            }
        }

        private void ConvertLabelsTemporarily()
        {
            var convertion = new RegexConversion();
            convertion.Matching = MatchingText;
            convertion.Replacement = ReplacementText;

            if (!convertion.IsInitialized)
            {
                return;
            }

            myAddConversionEnabled = true;
            AddConversionCommand.RaiseCanExecuteChanged();

            ConvertTemporarily(convertion);
        }

        public ILabelConversionStep SelectedConversionStep
        {
            get { return mySelectedConversionStep; }
            set { SetProperty(ref mySelectedConversionStep, value); }
        }

        private void OnRemoveConversionStep()
        {
            RemoveConversionStep(SelectedConversionStep);
        }

        public ICommand CancelCommand
        {
            get;
            private set;
        }

        public ICommand OkCommand
        {
            get;
            private set;
        }

        private void OnCancel()
        {
            ((IConfirmation)Notification).Confirmed = false;
            FinishInteraction();
        }

        private void OnOk()
        {
            Config.LabelConversion.Clear();
            Config.LabelConversion.AddRange(ConversionSteps);
            Config.Save();

            if (Presentation != null)
            {
                var converter = new GenericLabelConverter(Config.LabelConversion);
                var captionModule = Presentation.GetPropertySetFor<Caption>();
                foreach (var node in Presentation.Graph.Nodes)
                {
                    var caption = captionModule.Get(node.Id);
                    caption.DisplayText = converter.Convert(caption.Label);
                }
            }

            ((IConfirmation)Notification).Confirmed = true;
            FinishInteraction();
        }

        void myConfigService_ConfigChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(Config));
            ConversionSteps = new ObservableCollection<ILabelConversionStep>(Config.LabelConversion);
            RaisePropertyChanged(nameof(ConversionSteps));
        }

        public Config Config
        {
            get
            {
                return myConfigService.Config;
            }
        }

        public ObservableCollection<ILabelConversionStep> ConversionSteps
        {
            get;
            private set;
        }

        public ObservableCollection<LabelViewModel> Labels
        {
            get;
            set;
        }

        public ILabelConversionStep TemporalConversionStep
        {
            get;
            private set;
        }

        public IGraphPresentation Presentation
        {
            get;
            private set;
        }

        public void RemoveConversionStep(ILabelConversionStep step)
        {
            ConversionSteps.Remove(step);

            ConvertAllLabels();

            SelectedConversionStep = ConversionSteps.FirstOrDefault();
        }

        private void ConvertAllLabels()
        {
            var converter = new GenericLabelConverter(ConversionSteps);

            foreach (var label in Labels)
            {
                label.Commited = converter.Convert(label.Original);
            }
        }

        internal void ConvertTemporarily(ILabelConversionStep convertion)
        {
            TemporalConversionStep = convertion;

            var steps = new Queue<ILabelConversionStep>();
            steps.Enqueue(TemporalConversionStep);

            var converter = new GenericLabelConverter(steps);

            foreach (var label in Labels)
            {
                label.Temporal = converter.Convert(label.Commited);
            }
        }

        internal void ResetTemporalLabels()
        {
            foreach (var label in Labels)
            {
                label.Temporal = label.Commited;
            }
        }

        internal void CommitTemporalLabels()
        {
            ConversionSteps.Add(TemporalConversionStep);

            foreach (var label in Labels)
            {
                label.Commited = label.Temporal;
            }
        }

        protected override void OnModelPropertyChanged(string propertyName)
        {
            if (propertyName == "Presentation")
            {
                if (Presentation == Model.Presentation)
                {
                    return;
                }

                Presentation = Model.Presentation;

                Labels.Clear();

                var converter = new GenericLabelConverter(ConversionSteps);
                var captionModule = Presentation.GetPropertySetFor<Caption>();
                foreach (var node in Presentation.Graph.Nodes)
                {
                    var label = new LabelViewModel(captionModule.Get(node.Id).Label);
                    label.Commited = converter.Convert(label.Original);

                    Labels.Add(label);
                }

                RaisePropertyChanged(nameof(Labels));
            }
        }

        public Action FinishInteraction
        {
            get;
            set;
        }

        public INotification Notification
        {
            get;
            set;
        }
    }
}
