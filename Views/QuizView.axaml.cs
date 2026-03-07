using Avalonia.Controls;
using Avalonia.Input;
using VocabularyTrainer.ViewModels;

namespace VocabularyTrainer.Views;

/// <summary>
/// Quiz window displaying vocabulary question with multiple choice answers.
/// </summary>
public partial class QuizView : Window
{
    public QuizView()
    {
        InitializeComponent();
    }

    private void Window_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Allow closing by clicking anywhere when quiz is completed
        if (DataContext is QuizViewModel viewModel)
        {
            viewModel.OnWindowClicked();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Cleanup resources
        if (DataContext is QuizViewModel viewModel)
        {
            viewModel.Dispose();
        }

        base.OnClosed(e);
    }
}
