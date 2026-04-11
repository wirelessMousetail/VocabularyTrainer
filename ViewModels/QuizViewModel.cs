using System.Collections.ObjectModel;
using Avalonia.Threading;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;
using VocabularyTrainer.Services.Quiz.Presenters;
using Timer = System.Timers.Timer;

namespace VocabularyTrainer.ViewModels;

/// <summary>
/// View model for the quiz window, handling quiz presentation logic.
/// </summary>
public class QuizViewModel : ViewModelBase
{
    private const string ColorDefault = "Black";
    private const string ColorCorrect = "Green";
    private const string ColorWrong = "Red";
    private const string ColorMaxAttempts = "Orange";

    private readonly QuizSession _session;
    private readonly Action _onQuizCompleted;
    private Timer? _autoCloseTimer;

    private string _question = string.Empty;
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
    /// Gets the collection of answer options.
    /// </summary>
    public ObservableCollection<string> AnswerOptions { get; }

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
    /// Command executed when an answer is selected.
    /// </summary>
    public RelayCommand<string> AnswerCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuizViewModel"/> class.
    /// </summary>
    /// <param name="session">The quiz session.</param>
    /// <param name="onQuizCompleted">Callback when the quiz is completed.</param>
    public QuizViewModel(QuizSession session, Action onQuizCompleted)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _onQuizCompleted = onQuizCompleted ?? throw new ArgumentNullException(nameof(onQuizCompleted));

        Question = _session.Quiz.Question;
        AnswerOptions = new ObservableCollection<string>(_session.Quiz.Options);

        AnswerCommand = new RelayCommand<string>(OnAnswerSelected, _ => !IsQuizCompleted);
    }

    private void OnAnswerSelected(string? selectedAnswer)
    {
        if (string.IsNullOrEmpty(selectedAnswer) || IsQuizCompleted)
            return;

        _session.Presenter.OnAnswerSelected(selectedAnswer);
        var result = _session.Presenter.GetResult();

        switch (result)
        {
            case QuizResult.Correct:
                ResultMessage = "Correct!";
                ResultColor = ColorCorrect;
                IsQuizCompleted = true;
                StartAutoCloseTimer();
                break;

            case QuizResult.Wrong:
                ResultMessage = "Wrong!";
                ResultColor = ColorWrong;

                if (_session.Configuration.ShowCorrectAnswerOnWrong)
                {
                    var correctAnswer = _session.Presenter.GetCorrectAnswer();
                    ResultMessage += $" (Correct: {correctAnswer})";
                }
                break;

            case QuizResult.MaxAttemptsReached:
                var answer = _session.Presenter.GetCorrectAnswer();
                ResultMessage = $"Max attempts reached. Answer: {answer}";
                ResultColor = ColorMaxAttempts;
                IsQuizCompleted = true;
                StartAutoCloseTimer();
                break;
        }

        AnswerCommand.RaiseCanExecuteChanged();
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
            // Marshal to UI thread since timer runs on background thread
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
