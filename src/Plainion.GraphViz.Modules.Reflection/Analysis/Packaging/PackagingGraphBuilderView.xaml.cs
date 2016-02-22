/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2016. All rights reserved
   ------------------------------------------------------------------------------------------------- */
   
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging
{
    [Export( typeof( PackagingGraphBuilderView ) )]
    public partial class PackagingGraphBuilderView : UserControl
    {
        [ImportingConstructor]
        internal PackagingGraphBuilderView( PackagingGraphBuilderViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
