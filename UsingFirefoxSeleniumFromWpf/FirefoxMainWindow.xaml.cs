using HostAppInPanelLib.Controls;

namespace UsingFirefoxSeleniumFromWpf
{
    /// <summary>
    ///     Interaction logic for FirefoxMainWindow.xaml
    /// </summary>
    public partial class FirefoxMainWindow
    {
        public FirefoxMainWindow():base(typeof(FirefoxWrapperControl))
        {
            InitializeComponent();
        }
    }
}