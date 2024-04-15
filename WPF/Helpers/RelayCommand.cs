using System;
using System.Windows.Input;

namespace KGuiX.Helpers
{
    internal class RelayCommand : ICommand
    {
        /// <summary>
        /// The predicate that gets executed when <see cref="CanExecute(object)"/> is called.
        /// </summary>
        readonly Predicate<object>? _canExecute;

        /// <summary>
        /// The action that gets executed when <see cref="Execute(object)"/> is called.
        /// </summary>
        readonly Action<object> _execute;

        /// <inheritdoc/>
        public bool CanExecute(object? parameter)
            => _canExecute is null || _canExecute(parameter);   // Returns null for Execute only command

        /// <inheritdoc/>
        public void Execute(object? parameter)
            => _execute(parameter);

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged
        {
            add    => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Creates a new instance of <see cref="RelayCommand"/> without a predicate.
        /// </summary>
        /// <param name="execute">The action used for <see cref="Execute(object)"/>.</param>
        public RelayCommand(Action<object> execute) => _execute = execute;

        /// <summary>
        /// Creates a new instance of <see cref="RelayCommand"/> using the supplied values.
        /// </summary>
        /// <param name="canExecute">The predicate used for <see cref="CanExecute(object)"/>.</param>
        /// <param name="execute">The action used for <see cref="Execute(object)"/>.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            => (_execute, _canExecute) = (execute, canExecute);
    }
}
