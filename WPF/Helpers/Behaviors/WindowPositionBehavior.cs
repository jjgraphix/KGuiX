using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

namespace KGuiX.Helpers.Behaviors
{
    internal class WindowPositionBehavior : Behavior<Window>
    {
        /// <summary>
        /// Indicates the status of user setting <see cref="UiSaveWindowPos">.
        /// </summary>
        public static readonly DependencyProperty EnableSavingProperty =
            DependencyProperty.Register("SaveWinPosition",
                typeof(bool), typeof(WindowPositionBehavior), new PropertyMetadata(true));

        public bool SaveWinPosition
        {
            get { return (bool)GetValue(EnableSavingProperty); }
            set { SetValue(EnableSavingProperty, value); }
        }

        double _screenWidth;
        double _screenHeight;
        double _windowWidth;
        double _windowHeight;

        /// <summary>
        /// Restore window position on loaded event of the associated window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnWindowLoaded(object? sender, EventArgs e)
        {
            if (SaveWinPosition)
            {
                _screenWidth = SystemParameters.WorkArea.Width;
                _screenHeight = SystemParameters.WorkArea.Height;
                _windowWidth = AssociatedObject.Width;
                _windowHeight = AssociatedObject.Height;

                RestoreWindowPosition();            }
        }

        /// <summary>
        /// Save window position on closing event of the associated window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            if (AssociatedObject.WindowState == WindowState.Normal)
            {
                SaveWindowPosition();
            }
        }

        /// <summary>
        /// Save last window position on state changed event when associated window is minimized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnWindowStateChange(object? sender, EventArgs e)
        {
            if (AssociatedObject.WindowState == WindowState.Minimized)
            {
                SaveWindowPosition();
            }
        }

        /// <summary>
        /// Handle keyboard events of the associated Window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (key == Key.Escape)
                ClearElementFocus();
        }

        /// <summary>
        /// Handle mouse down event of the associated Window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ClearElementFocus();
        }


        /// <summary>
        /// Restore previous window position within the active display boundary.
        /// </summary>
        void RestoreWindowPosition()    // TODO: Ensure support for multiple displays
        {
            double leftPos = Properties.Settings.Default.WindowPosLeft;
            double topPos = Properties.Settings.Default.WindowPosTop;

            // AssociatedObject.Topmost = IsTopmost;

            // NOTE: WindowStartupLocation defined in XAML as fallback for invalid position
            if (IsValidPosition(leftPos, topPos))
            {
                if ((leftPos + _windowWidth) > _screenWidth)
                    leftPos = Math.Floor(_screenWidth - _windowWidth);
                if ((topPos + _windowHeight) > _screenHeight)
                    topPos = Math.Floor(_screenHeight - _windowHeight);

                AssociatedObject.Left = (leftPos > 0) ? leftPos : 0;
                AssociatedObject.Top = (topPos > 0) ? topPos : 0;
            }
        }

        /// <summary>
        /// Save valid window positions to application property settings.
        /// </summary>
        void SaveWindowPosition()
        {
            if (IsValidPosition(AssociatedObject.Left, AssociatedObject.Top))
            {
                Properties.Settings.Default.WindowPosLeft = AssociatedObject.Left;
                Properties.Settings.Default.WindowPosTop = AssociatedObject.Top;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Validate that the window position is a real coordinate.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        bool IsValidPosition(double left, double top)
        {
            return !(double.IsNaN(left) || double.IsNaN(top)
                || double.IsInfinity(left) || double.IsInfinity(top));
        }

        /// <summary>
        /// Move logical focus of from active UIElement to the parent TabItem and clear keyboard focus.
        /// </summary>
        void ClearElementFocus()
        {
            UIElement focusedElement = Keyboard.FocusedElement as UIElement;

            if (focusedElement is UIElement)
            {
                var tabItem = FindParentTabItem(focusedElement);

                if (tabItem != null)
                {
                    // NOTE: TabItem used to avoid focus bugs related to moving logical focus to the app window
                    FocusManager.SetFocusedElement(tabItem, tabItem);
                    Keyboard.ClearFocus();
                }
            }
        }

        /// <summary>
        /// Find selected TabItem of the parent TabControl for the focused UIElement.
        /// </summary>
        /// <param name="focusedElement"></param>
        /// <returns></returns>
        TabItem FindParentTabItem(UIElement focusedElement)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(focusedElement);

            while (parent != null && !(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            TabControl tabControl = parent as TabControl;

            if (tabControl != null)
            {
                foreach (TabItem tabItem in tabControl.Items)
                {
                    if (tabItem.IsSelected)
                        return tabItem;
                }
            }

            return null;    // No selected TabItem was found
        }


        protected override void OnAttached()
        {
            AssociatedObject.Loaded += OnWindowLoaded;
            AssociatedObject.PreviewKeyDown += Window_PreviewKeyDown;
            AssociatedObject.MouseDown += Window_MouseDown;

            if (SaveWinPosition)
            {
                AssociatedObject.Closing += OnWindowClosing;
                AssociatedObject.StateChanged += OnWindowStateChange;
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnWindowLoaded;
            AssociatedObject.Closing -= OnWindowClosing;
            AssociatedObject.StateChanged -= OnWindowStateChange;
            AssociatedObject.PreviewKeyDown -= Window_PreviewKeyDown;
            AssociatedObject.MouseDown -= Window_MouseDown;
        }
    }
}