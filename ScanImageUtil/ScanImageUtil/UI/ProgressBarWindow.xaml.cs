using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ScanImageUtil.UI
{
    /// <summary>
    /// Interaction logic for ProgressBarWindow.xaml
    /// </summary>
    public partial class ProgressBarWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private readonly BackgroundWorker currentWorker;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        public ProgressBarWindow(BackgroundWorker worker)
        {
            InitializeComponent();            
            this.Loaded += Window_Loaded;
            currentWorker = worker;
        }

        public void UpdateProgress(int percentage)
        {
            // When progress is reported, update the progress bar control.
            pbLoad.Value = percentage;

            // When progress reaches 100%, close the progress bar window.
            if (percentage >= 100)
            {
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            currentWorker.CancelAsync();
            this.Close();
        }                        
    }
}
