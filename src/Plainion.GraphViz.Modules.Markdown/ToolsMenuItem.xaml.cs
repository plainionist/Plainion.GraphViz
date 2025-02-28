﻿using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Markdown
{
    [ViewSortHint("tool-0100")]
    internal partial class ToolsMenuItem : MenuItem
    {
        public ToolsMenuItem(ToolsMenuItemModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}