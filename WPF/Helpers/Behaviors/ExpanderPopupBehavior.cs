using System;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

namespace KGuiX.Helpers.Behaviors
{
    internal class ExpanderPopupBehavior : Behavior<Expander>
    {
        protected override void OnAttached()
        {
            AssociatedObject.IsVisibleChanged += Expander_IsVisibleChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.IsVisibleChanged -= Expander_IsVisibleChanged;
        }

        /// <summary>
        /// Ensure associated expander is not expanded if it is not visible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Expander_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!AssociatedObject.IsVisible && AssociatedObject.IsExpanded)
            {
                AssociatedObject.IsExpanded = false;
            }
        }
    }
}