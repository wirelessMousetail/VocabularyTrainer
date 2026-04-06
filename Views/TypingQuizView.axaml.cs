using System.ComponentModel;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using VocabularyTrainer.ViewModels;

namespace VocabularyTrainer.Views;

/// <summary>
/// Quiz window for typing-mode quizzes where the user types a free-text answer.
/// </summary>
public partial class TypingQuizView : Avalonia.Controls.Window
{
    private bool _readyToClose;

    public TypingQuizView()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, Window_KeyDown, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is TypingQuizViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TypingQuizViewModel.IsQuizCompleted)
            && DataContext is TypingQuizViewModel { IsQuizCompleted: true })
        {
            RootPanel.Focus();
            // Defer so the Enter that submitted the answer doesn't also close the window
            Dispatcher.UIThread.Post(() => _readyToClose = true);
        }
    }

    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _readyToClose && DataContext is TypingQuizViewModel vm)
            vm.OnWindowClicked();
    }

    private void Window_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is TypingQuizViewModel vm)
            vm.OnWindowClicked();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is TypingQuizViewModel vm)
            vm.Dispose();
        base.OnClosed(e);
    }
}
