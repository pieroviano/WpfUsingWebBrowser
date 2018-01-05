using HostAppInPanelLib.Controls;

namespace UsingChromeSeleniumFromWpf
{
    /// <summary>
    ///     Interaction logic for ChromeMainWindow.xaml
    /// </summary>
    public partial class ChromeMainWindow
    {
        public ChromeMainWindow():base(typeof(ChromeWrapperControl))
        {
            InitializeComponent();
        }
    }
}