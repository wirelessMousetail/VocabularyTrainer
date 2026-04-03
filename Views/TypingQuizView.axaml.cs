using Avalonia.Input;
using VocabularyTrainer.ViewModels;

namespace VocabularyTrainer.Views;

/// <summary>
/// Quiz window for typing-mode quizzes where the user types a free-text answer.
/// </summary>
public partial class TypingQuizView : Avalonia.Controls.Window
{
    public TypingQuizView()
    {
        InitializeComponent();
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
