using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EquipmentSignalData.Command
{
    public class RelayCommandT<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Predicate<T>? canExecute;

        public RelayCommandT(Action<T> execute, Predicate<T>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            // 如果没有指定 `canExecute`，则始终允许执行；否则检查是否可以执行
            return canExecute == null || (parameter is T t && canExecute(t));
        }

        public void Execute(object? parameter)
        {
            if (parameter is T t)
            {
                execute(t);
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

    }
}
