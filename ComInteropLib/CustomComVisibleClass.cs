using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using mshtml;

namespace ComInteropLib
{
    [ComVisible(true)]
    public class CustomComVisibleClass
    {
        public CustomComVisibleClass(WebBrowser webBrowser)
        {
            WebBrowser = webBrowser;
        }

        public WebBrowser WebBrowser { get; set; }

        public event EventHandler RaisedEvent;

        public bool CodeToExecute()
        {
            MessageBox.Show((WebBrowser.Document as HTMLDocument)?.body.parentElement.outerHTML);
            RaisedEvent?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}