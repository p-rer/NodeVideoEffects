using System.Windows.Input;

namespace NodeVideoEffects.Utility;

public class RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    : ICommand
{
    private readonly Action<object?> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    // コマンドが実行可能かどうかを判定
    public bool CanExecute(object? parameter)
    {
        return canExecute == null || canExecute(parameter);
    }

    // コマンドを実行
    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    // CanExecuteの結果が変更された際に発生するイベント
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}