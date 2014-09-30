
using System;
using System.Windows.Interop;

namespace PebbleSharp.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {
        public static IntPtr MainWindowHandle { get; private set; }

        protected override void OnActivated( EventArgs e )
        {
            base.OnActivated( e );
            if ( MainWindowHandle == IntPtr.Zero )
            {
                MainWindowHandle = new WindowInteropHelper( MainWindow ).Handle;
            }
        }
    }
}
