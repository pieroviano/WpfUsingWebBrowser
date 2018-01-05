using System.Drawing;
using System.Windows.Forms;

namespace UsingWebBrowserFromWinForm
{
    public class LoadingAdorner : Label
    {

        public LoadingAdorner()
        {
            Text = "Please Wait";
            TextAlign = ContentAlignment.MiddleCenter;
            Dock = DockStyle.Fill;
            Visible = false;
        }

        public bool IsAdornerVisible { get; set; }

        public void StartStopWait(Control uiElement)
        {
            IsAdornerVisible = !IsAdornerVisible;
            Visible = IsAdornerVisible;

            uiElement.Visible = !IsAdornerVisible;
        }

    }
}
