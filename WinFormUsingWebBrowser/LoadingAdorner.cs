using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormUsingWebBrowser
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
