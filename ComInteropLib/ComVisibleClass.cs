using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using mshtml;

namespace ComInteropLib
{
    [ComVisible(true)]
    public class ComVisibleClass
    {
        public ComVisibleClass(WebBrowser webBrowser)
        {
            WebBrowser = webBrowser;
        }

        public WebBrowser WebBrowser { get; set; }

        public bool CodeToExecute()
        {
            MessageBox.Show((WebBrowser.Document as HTMLDocument)?.body.parentElement.outerHTML);
            return true;
        }
    }
}