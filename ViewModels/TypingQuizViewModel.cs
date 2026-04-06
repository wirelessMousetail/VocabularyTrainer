using Avalonia.Threading;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;
using Timer = System.Timers.Timer;

namespace VocabularyTrainer.ViewModels;

/// <summary>
/// View model for the typing-mode quiz window.
/// </summary>
public class TypingQuizViewModel : ViewModelBase
{
    private const string ColorDefault = "Black";
    private const string ColorCorrect = "Green";
    private const string ColorWrong = "Red";
    private const string ColorArticle = "Orange";

    private readonly QuizSession _session;
    private readonly Action _onQuizCompleted;
    private Timer? _autoCloseTimer;

    private string _question = string.Empty;
    private string _textInput = string.Empty;
    private string _hintText = string.Empty;
    private string _resultMessage = string.Empty;
    private string _resultColor = ColorDefault;
    private bool _isQuizCompleted;

    /// <summary>
    /// Gets the question text to display.
    /// </summary>
    public string Question
    {
        get => _question;
        private set => SetProperty(ref _question, value);
    }

    /// <summary>
    /// Gets or sets the text the user has typed as their answer.
    /// </summary>
    public string TextInput
    {
        get => _textInput;
        set => SetProperty(ref _textInput, value);
    }

    /// <summary>
    /// Gets the letter-reveal hint text (shown when TypingRevealLetters is enabled).
    /// </summary>
    public string HintText
    {
        get => _hintText;
        private set => SetProperty(ref _hintText, value);
    }

    /// <summary>
    /// Gets the result message (Correct!, Wrong!, etc.).
    /// </summary>
    public string ResultMessage
    {
        get => _resultMessage;
        private set => SetProperty(ref _resultMessage, value);
    }

    /// <summary>
    /// Gets the color for the result message.
    /// </summary>
    public string ResultColor
    {
        get => _resultColor;
        private set => SetProperty(ref _resultColor, value);
    }

    /// <summary>
    /// Gets a value indicating whether the quiz is completed.
    /// </summary>
    public bool IsQuizCompleted
    {
        get => _isQuizCompleted;
        private set => SetProperty(ref _isQuizCompleted, value);
    }

    /// <summary>
    /// Command executed when the user submits their typed answer.
    /// </summary>
    public RelayCommand SubmitCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypingQuizViewModel"/> class.
    /// </summary>
    /// <param name="session">The quiz session.</param>
    /// <param name="onQuizCompleted">Callback when the quiz is completed.</param>
    public TypingQuizViewModel(QuizSession session, Action onQuizCompleted)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _onQuizCompleted = onQuizCompleted ?? throw new ArgumentNullException(nameof(onQuizCompleted));

        Question = _session.Quiz.Question;

        SubmitCommand = new RelayCommand(OnSubmit, () => !IsQuizCompleted);
    }

    private void OnSubmit()
    {
        if (IsQuizCompleted || string.IsNullOrEmpty(TextInput))
            return;

        _session.Presenter.OnAnswerSelected(TextInput);
        var result = _session.Presenter.GetResult();

        switch (result)
        {
            case QuizResult.Correct:
                ResultMessage = "Correct!";
                ResultColor = ColorCorrect;
                IsQuizCompleted = true;
                SubmitCommand.RaiseCanExecuteChanged();
                StartAutoCloseTimer();
                break;

            case QuizResult.WrongArticle:
                ResultMessage = "Wrong article!";
                ResultColor = ColorArticle;
                UpdateHint();
                break;

            case QuizResult.Wrong:
                ResultMessage = "Wrong!";
                ResultColor = ColorWrong;
                UpdateHint();
                break;
        }

        SubmitCommand.RaiseCanExecuteChanged();
    }

    private void UpdateHint()
    {
        var hint = _session.Presenter.GetHint();
        HintText = hint ?? string.Empty;
    }

    private void StartAutoCloseTimer()
    {
        if (_autoCloseTimer != null)
            return;

        var interval = _session.Configuration.AutoCloseAfterCorrectSeconds * 1000;
        _autoCloseTimer = new Timer(interval);
        _autoCloseTimer.Elapsed += (_, _) =>
        {
            _autoCloseTimer.Stop();
            Dispatcher.UIThread.Post(() => _onQuizCompleted());
        };
        _autoCloseTimer.AutoReset = false;
        _autoCloseTimer.Start();
    }

    /// <summary>
    /// Handles manual quiz closure (e.g., clicking on window when completed).
    /// </summary>
    public void OnWindowClicked()
    {
        if (IsQuizCompleted)
        {
            _autoCloseTimer?.Stop();
            _onQuizCompleted();
        }
    }

    /// <summary>
    /// Disposes resources used by the view model.
    /// </summary>
    public void Dispose()
    {
        _autoCloseTimer?.Stop();
        _autoCloseTimer?.Dispose();
    }
}
