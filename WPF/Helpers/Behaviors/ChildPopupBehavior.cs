using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

using Microsoft.Xaml.Behaviors;

namespace KGuiX.Helpers.Behaviors
{
    internal class ChildPopupBehavior : Behavior<Popup>
    {
        Window _appWindow;
        DispatcherTimer _delayTimer;

        void OnPopupOpened(object sender, EventArgs e)
        {
            UpdatePopupPosition();
        }

        /// <summary>
        /// Close popup during main window location change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnWindowLocationChanged(object? sender, EventArgs e)
        {
            if (AssociatedObject.IsOpen)
            {
                AssociatedObject.IsOpen = false;    // Closes popup while window is moving
                _delayTimer?.Stop();
                _delayTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(600), DispatcherPriority.Background, PopupOpenDelay, Dispatcher);
                _delayTimer.Start();
            }
        }

        /// <summary>
        /// Handle mouse left button down event to close popup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Popup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 1)                  // Closes popup on mouse click
                AssociatedObject.IsOpen = false;
        }

        /// <summary>
        /// Open popup on dispatcher timer tick.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PopupOpenDelay(object? sender, EventArgs e)
        {
            AssociatedObject.IsOpen = true;         // Delays opening of popup by timer interval
            _delayTimer.Stop();
        }

        /// <summary>
        /// Override popup topmost property to ensure visual relationship with the main window.
        /// </summary>
        void UpdatePopupPosition()
        {
            var hwnd = ((HwndSource)PresentationSource.FromVisual(AssociatedObject.Child)).Handle;

            if (GetWindowRect(hwnd, out RECT rect))     // Reposition popup window and modify topmost property
                SetWindowPos(hwnd, _appWindow.Topmost ? -1 : -2, rect.Left, rect.Top, 0, 0, 0x0011);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
            static extern int SetWindowPos(IntPtr hWnd, int hwndInsertAfter, int x, int y, int cx, int cy, int wFlags);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            internal int Left;
            internal int Top;
            internal int Right;
            internal int Bottom;
        }

        protected override void OnAttached()
        {
            AssociatedObject.Opened += OnPopupOpened;
            AssociatedObject.PreviewMouseLeftButtonDown += Popup_MouseLeftButtonDown;

            if ((_appWindow = Window.GetWindow(AssociatedObject)) != null)
                _appWindow.LocationChanged += OnWindowLocationChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Opened -= OnPopupOpened;
            AssociatedObject.PreviewMouseLeftButtonDown -= Popup_MouseLeftButtonDown;
            _appWindow.LocationChanged -= OnWindowLocationChanged;
            _delayTimer?.Stop();
            _delayTimer = null;
        }

    }
}
