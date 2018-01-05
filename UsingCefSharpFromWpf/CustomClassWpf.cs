using System;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;

namespace UsingCefSharpFromWpf
{
    public class CustomClassWpf
    {
        public CustomClassWpf(ChromiumWebBrowser webBrowser)
        {
            WebBrowser = webBrowser;
        }

        public ChromiumWebBrowser WebBrowser { get; set; }

        public event EventHandler RaisedEvent;

        public bool CodeToExecute()
        {
            var task = WebBrowser.EvaluateScriptAsync(@"document.getElementsByTagName ('html')[0].innerHTML");
            task.Wait();
            MessageBox.Show(task.Result.Result.ToString());
            RaisedEvent?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}