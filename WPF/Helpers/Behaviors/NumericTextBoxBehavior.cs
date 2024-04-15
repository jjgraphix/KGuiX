using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

namespace KGuiX.Helpers.Behaviors
{
    internal class NumericTextBoxBehavior : Behavior<TextBox> 
    {
        public static readonly DependencyProperty MaxValueProperty = 
            DependencyProperty.Register("MaxValue",
                typeof(int), typeof(NumericTextBoxBehavior), new PropertyMetadata(999999));

        public int MaxValue     // Gets the maximum allowed property value
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty = 
            DependencyProperty.Register("MinValue",
                typeof(int), typeof(NumericTextBoxBehavior), new PropertyMetadata(0));

        public int MinValue     // Gets the minimum allowed property value
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        /// <summary>
        /// Preview text input of the associated text box to validate numerical characters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
            => e.Handled = sender is TextBox
                           && !string.IsNullOrEmpty(e.Text)
                           && !char.IsDigit(e.Text, e.Text.Length - 1);

        /// <summary>
        /// <br>Handle keyboard events of the associated text box.</br>
        /// <br>Allow key combinations for undo/redo and copy/paste commands.</br>
        /// <br>Update source property on enter key then move keyboard focus down.</br>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox))
                return;

            TextBox textBox = AssociatedObject as TextBox;
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);  // Ensures correct key type

            bool isValueUp = key is Key.Up or Key.Add;
            bool isValueDown = key is Key.Down or Key.Subtract;
            bool isCtrl = Keyboard.Modifiers == ModifierKeys.Control;
            bool isAltKey = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);

            if (isValueUp || isValueDown)
                SetValueIncrement(isValueUp, isValueDown);

            if (key is Key.Enter or Key.Return)
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();     // Updates source property with new value

            if (key == Key.Z && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                textBox.Redo();     // Alternate CTRL+SHIFT+Z redo command

            // Allow key commands for undo/redo, copy/paste and select all
            e.Handled = key is Key.Space || isAltKey
                        || (isCtrl && !(key is Key.A or Key.Z or Key.Y or Key.C or Key.V))
                        || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift | ModifierKeys.Control);
        }

        /// <summary>
        /// Handle mousewheel event of associated text box to adjust numerical value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            SetValueIncrement(e.Delta > 0, e.Delta < 0);    // Adjust value on mousewheel up or down
            e.Handled = true;
        }

        /// <summary>
        /// Handle text changed event of associated text box to limit input characters to a maximum length.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var sourceBinding = AssociatedObject.GetBindingExpression(TextBox.TextProperty);

            if (sourceBinding != null)
            {
                if (AssociatedObject.Text.Length > MaxValue.ToString().Length + 1)
                    sourceBinding.UpdateTarget();   // Resets to previously set value
            }
        }

        /// <summary>
        /// Select all text on the mouse double click event of the associated text box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBox_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (AssociatedObject.SelectionLength == 0)
                AssociatedObject.SelectAll();
        }

        /// <summary>
        /// Incremental value adjustment with shift key stepping modifier.
        /// </summary>
        /// <param name="increaseValue"></param>
        /// <param name="decreaseValue"></param>
        void SetValueIncrement(bool increaseValue = false, bool decreaseValue = false)
        {
            if (!increaseValue && !decreaseValue)
                return;

            TextBox textBox = AssociatedObject as TextBox;

            uint textBoxValue = Convert.ToUInt32(textBox.Text != "" ? textBox.Text : null);
            bool isShift = Keyboard.Modifiers == ModifierKeys.Shift;

            // Set increment stepping for shift key modifier depending on maximum allowed value
            int increment = isShift ? (MaxValue >= 10000 ? 500 : (MaxValue > 1000 ? 50 : 5)) : 1;

            if (increaseValue)
                textBox.Text = Convert.ToString(Math.Min((textBoxValue + increment), MaxValue));
            if (decreaseValue)
                textBox.Text = Convert.ToString(Math.Max((textBoxValue - increment), MinValue));
        }

        /// <summary>
        /// Handle clipboard paste event of associated text box to prevent non-numeric strings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string? clipboardText = Convert.ToString(e.DataObject.GetData(DataFormats.Text));

                if(!uint.TryParse(clipboardText, out _)) // Validates if string is unsigned integer
                    e.CancelCommand();
            }
        }

        protected override void OnAttached()
        {
            AssociatedObject.PreviewTextInput += TextBox_PreviewTextInput;
            AssociatedObject.PreviewKeyDown += TextBox_PreviewKeyDown;
            AssociatedObject.TextChanged += TextBox_TextChanged;
            AssociatedObject.MouseDoubleClick += TextBox_MouseDoubleClick;
            AssociatedObject.PreviewMouseWheel += TextBox_PreviewMouseWheel;
            DataObject.AddPastingHandler(AssociatedObject, PastingHandler);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewTextInput -= TextBox_PreviewTextInput;
            AssociatedObject.PreviewKeyDown -= TextBox_PreviewKeyDown;
            AssociatedObject.TextChanged -= TextBox_TextChanged;
            AssociatedObject.MouseDoubleClick -= TextBox_MouseDoubleClick;
            AssociatedObject.PreviewMouseWheel -= TextBox_PreviewMouseWheel;
            DataObject.RemovePastingHandler(AssociatedObject, PastingHandler);
        }
    }
}
