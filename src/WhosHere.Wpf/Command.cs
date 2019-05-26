using System;
using System.Windows.Input;

namespace WhosHere.Wpf
{
    public class Command : ICommand
    {
        private Action<object> execute;

        private Predicate<object> canExecute;

        private event EventHandler CanExecuteChangedInternal;

        public Command(Action<object> execute)
            : this(execute, DefaultCanExecute)
        {
        }

        public Command(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException("canExecute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                CanExecuteChangedInternal -= value;
            }
        }

        public bool CanExecute(object parameter) => 
            canExecute != null && canExecute(parameter);

        public void Execute(object parameter) => execute(parameter);

        public void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChangedInternal;
            if (handler != null)
            {
                //DispatcherHelper.BeginInvokeOnUIThread(() => handler.Invoke(this, EventArgs.Empty));
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public void Destroy()
        {
            canExecute = _ => false;
            execute = _ => { return; };
        }

        private static bool DefaultCanExecute(object parameter) => true;
    }
}

