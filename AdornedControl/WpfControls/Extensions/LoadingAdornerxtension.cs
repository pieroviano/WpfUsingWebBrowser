using System.Windows;

namespace WebBrowserLib.WpfControls.Extensions
{
    public static class LoadingAdornerxtension
    {
        public static void StartStopWait(this AdornedControl.AdornedControl loadingAdorner, UIElement uiElement)
        {
            loadingAdorner.IsAdornerVisible = !loadingAdorner.IsAdornerVisible;

            //WebBrowser.IsEnabled = !WebBrowser.IsEnabled;
            uiElement.Visibility = uiElement.Visibility == Visibility.Visible // condition
                ? Visibility.Collapsed // if-case
                : Visibility.Visible; // else-case
        }
    }
}