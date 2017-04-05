using System;

namespace Plainion.GraphViz.Presentation
{
    public class CaptionModule : PropertySetModule<Caption>, ICaptionModule
    {
        public CaptionModule(Func<string, Caption> defaultProvider)
            : base(defaultProvider)
        {
        }

        public ILabelConverter LabelConverter { get; set; }

        protected override void OnAdding(Caption item)
        {
            if (LabelConverter != null)
            {
                item.DisplayText = LabelConverter.Convert(item.Label);
            }
        }
    }
}
