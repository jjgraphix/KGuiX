using KGuiX.Interop;

using System;
using System.Windows;
using System.Windows.Interop;
using System.Diagnostics;

namespace KGuiX
{
    public partial class MainWindow : Window
    {
        internal event EventHandler? ThemeSwitched;

        /// <summary>
        /// Creates a new <see cref="MainWindow"/> instance.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when the <see cref="MainWindow"/> is initalized.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);      // Gets the application handle
            source?.AddHook(WndProc);           // Listens for message from another instance attempting to run

            // Interops.SetWorkingSetSize(Process.GetCurrentProcess(), -1, -1);        // Minimize memory allocation
        }

        /// <summary>
        /// Taskbar minimize <see cref="MainWindow"/> routed click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// Taskbar close <see cref="MainWindow"/> routed click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// UI theme toggle routed click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SwitchTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeSwitched?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Process message sent by another instance of the application.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Interops.WM_SHOWME)      // Occurs when another instance attempts to open
            {
                Activate();
                BringIntoView();
                WindowState = WindowState.Normal;
            }

            return IntPtr.Zero;
        }
    }
}