using EnvDTE;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AspNetFileNicluder.Logic.Includers
{
    /// <summary>
    /// Interaction logic for FIleIncluderToolboxControl.xaml.
    /// </summary>
    //[ProvideToolboxControl("AspNetFileNicluder.Logic.Includers.FIleIncluderToolboxControl", true)]
    [ProvideToolboxControl("General", true)]
    public partial class FIleIncluderToolboxControl : UserControl
    {
        public FIleIncluderToolboxControl()
        {
            InitializeComponent();
        }
    }
}
