using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using Microsoft.Xaml.Behaviors;

namespace KGuiX.Helpers.Behaviors
{
    internal class ClearFocusOnTabChangeBehavior : Behavior<TabControl>
    {
        /// <summary>
        /// Remove automatic focus of TextBox on the selection changed event of the associated tab control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0)
            {
                var previousTabItem = e.RemovedItems[0] as TabItem;     // Gets the previously selected TabItem

                if (previousTabItem != null)
                {
                    // Allow automatic focus behavior to complete when changing TabItem
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!previousTabItem.IsSelected)
                        {
                            // Removes automatic focus of text box element
                            if (Keyboard.FocusedElement as UIElement is TextBox)
                            {
                                AssociatedObject.Focus();
                                Keyboard.ClearFocus();
                            }
                        }
                    }), DispatcherPriority.Background);
                }
            }
        }

        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += TabControl_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= TabControl_SelectionChanged;
        }
    }
}