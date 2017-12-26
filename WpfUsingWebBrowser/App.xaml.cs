using System;
using System.Windows;

namespace UsingWebBrowserFromWpf
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected void OnStartup(object sender, StartupEventArgs e)
        {
            var toUri = new UriTypeConverter();
            StartupUri = (Uri) toUri.ConvertFrom("MainWindow.xaml");
        }
    }
}