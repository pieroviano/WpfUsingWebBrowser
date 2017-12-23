using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Threading;
using HostAppInPanelLib.Utility;
using HostAppInPanelLib.Utility.Win32;
using WpfAdornedControl.WpfControls.Extensions;

namespace HostAppInPanelLib
{
    /// <summary>
    ///     Interaction logic for WrapperWindow.xaml
    /// </summary>
    public partial class WrapperWindow : Window
    {
        private Panel _panel;

        public WrapperWindow()
        {
            InitializeComponent();
        }

        public bool KillProcessOnClose { get; set; } = false;

        public Process Process { get; set; }

        public string Arguments { get; set; } = "";
        public string ProcessPath { get; set; } = "notepad.exe";

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the interop host control.
            var host =
                new WindowsFormsHost();

            // Create the MaskedTextBox control.
            _panel = new Panel();

            // Assign the MaskedTextBox control as the host control's child.
            host.Child = _panel;

            // Add the interop host control to the Grid
            // control's collection of child controls.
            grid.Children.Add(host);

            if (Process == null)
            {
                Process = new Process();
                var info = new ProcessStartInfo
                {
                    FileName = ProcessPath,
                    Arguments = Arguments,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };
                Process.StartInfo = new ProcessStartInfo();
                Process.StartInfo = info;
                Process.Start();
            }

            var processExitWaiter = new ProcessExitWaiter(Process);
            processExitWaiter.ProcessExited += ProcessOnExited;
            processExitWaiter.WaitProcessExit();

            if (Process != null)
            {
                try
                {
                    Process.WaitForInputIdle();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
                Thread.Sleep(500);

                Win32Interop.SetParent(Process.MainWindowHandle, _panel.Handle);
                WindowHelper.MakeExternalWindowBorderless(Process.MainWindowHandle, 0, 0, _panel.Width, _panel.Height);
            }
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            ProcessExited();
        }

        public void ProcessExited()
        {
            try
            {
                Dispatcher.Invoke(() => { Close(); });
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!Process.HasExited)
            {
                Process.Kill();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (KillProcessOnClose)
            {
                Process.Kill();
            }
            if (Process.HasExited)
            {
                return;
            }
            e.Cancel = true;
            var hWnd = Process.MainWindowHandle;
            if (hWnd.ToInt32() != 0)
            {
                Win32Interop.PostMessage(hWnd, Win32Interop.WmClose, 0, 0);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingAdorner.StartStopWait(grid);

            var timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(2)};
            timer.Start();
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                LoadingAdorner.StartStopWait(grid);
                Width += 1;
                Width -= 1;
            };
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Process != null && _panel != null)
            {
                WindowHelper.MakeExternalWindowBorderless(Process.MainWindowHandle, 0, 0, _panel.Width, _panel.Height);
            }
        }
    }
}