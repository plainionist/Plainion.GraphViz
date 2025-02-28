﻿using System.Windows.Controls;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Obfuscate;

[ViewSortHint("tool-1000")]
partial class ToolsMenuItem : MenuItem
{
    public ToolsMenuItem(ToolsMenuItemModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}
