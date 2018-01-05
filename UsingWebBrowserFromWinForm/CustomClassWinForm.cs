using System;
using System.Windows.Forms;
using mshtml;

namespace UsingWebBrowserFromWinForm
{
    public class CustomClassWinForm
    {
        public CustomClassWinForm(WebBrowser webBrowser)
        {
            WebBrowser = webBrowser;
        }

        public WebBrowser WebBrowser { get; set; }

        public event EventHandler RaisedEvent;

        public bool CodeToExecute()
        {
            MessageBox.Show((WebBrowser.Document?.DomDocument as HTMLDocument)?.body.parentElement.outerHTML);
            RaisedEvent?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}
