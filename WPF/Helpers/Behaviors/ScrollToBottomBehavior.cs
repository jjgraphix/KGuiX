using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

namespace KGuiX.Helpers.Behaviors
{
    internal class ScrollToBottomBehavior : Behavior<TextBlock>
    {
        ScrollViewer? _scrollViewer;

        /// <summary>
        /// Set position of a parent ScrollViewer on loaded event of associated text block.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTextBlockLoaded(object sender, RoutedEventArgs e)
        {
            _scrollViewer = FindVisualParent<ScrollViewer>(AssociatedObject);
            ScrollToBottom();
        }

        /// <summary>
        /// Set position of parent ScrollViewer on size changed event of associated text block.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollToBottom();
        }

        /// <summary>
        /// Set ScrollViewer vertical position to bottom of associated text block.
        /// </summary>
        void ScrollToBottom()
        {
            if (_scrollViewer != null)
                _scrollViewer.ScrollToBottom();
        }

        /// <summary>
        /// Find parent object of the associated text block.
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject is null)
                return null;

            if (parentObject is T parent)
                return parent;

            return FindVisualParent<T>(parentObject);
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += OnTextBlockLoaded;
            AssociatedObject.SizeChanged += TextBlock_SizeChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnTextBlockLoaded;
            AssociatedObject.SizeChanged -= TextBlock_SizeChanged;
        }
    }
}

