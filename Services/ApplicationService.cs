using System;
using System.Timers;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

/// <summary>
/// Service responsible for orchestrating the application lifecycle, quiz scheduling, and coordination between services.
/// Replaces the Windows Forms ApplicationContext with a cross-platform implementation.
/// </summary>
public class ApplicationService : IDisposable
{
    private readonly QuizService _quizService;
    private readonly WordListService _wordListService;
    private readonly SettingsService _settingsService;
    private readonly System.Timers.Timer _nextQuizTimer;
    private bool _isPaused;
    private bool _isQuizOpen;
    private DateTime _timerStartedAt;

    /// <summary>
    /// Event raised when a new quiz should be shown.
    /// </summary>
    public event EventHandler<QuizSession>? QuizRequested;

    /// <summary>
    /// Event raised when the options window should be shown.
    /// </summary>
    public event EventHandler? OptionsRequested;

    /// <summary>
    /// Event raised when the application should exit.
    /// </summary>
    public event EventHandler? ExitRequested;

    /// <summary>
    /// Event raised when a fatal error occurs that prevents normal operation.
    /// The string argument contains the error message to display to the user.
    /// </summary>
    public event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// Event raised when the current quiz window is closed.
    /// </summary>
    public event EventHandler? QuizClosed;

    /// <summary>
    /// Event raised when the quiz timer is restarted due to a settings change.
    /// </summary>
    public event EventHandler? TimerRestarted;

    /// <summary>
    /// Gets a value indicating whether the application is paused.
    /// </summary>
    public bool IsPaused => _isPaused;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationService"/> class.
    /// </summary>
    /// <param name="settingsService">The settings service.</param>
    public ApplicationService(SettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        var precompiledPath = Path.Combine(AppContext.BaseDirectory, "Data", "words.csv");
        var managedPath = Path.Combine(AppContext.BaseDirectory, "appdata.csv");

        _wordListService = new WordListService(precompiledPath, managedPath);
        var words = _wordListService.LoadAndMerge();

        var weightStrategy = new WordWeightStrategy();
        _quizService = new QuizService(words, weightStrategy);

        _nextQuizTimer = new System.Timers.Timer();
        _nextQuizTimer.Interval = _settingsService.GetSettings().QuizIntervalSeconds * 1000;
        _nextQuizTimer.Elapsed += OnTimerElapsed;
        _nextQuizTimer.AutoReset = false; // Manual restart after quiz closes
    }

    /// <summary>
    /// Starts the application, showing the first quiz immediately.
    /// </summary>
    public void Start()
    {
        ShowQuiz();
    }

    /// <summary>
    /// Pauses the quiz timer.
    /// </summary>
    public void Pause()
    {
        _isPaused = true;
        _nextQuizTimer.Stop();
    }

    /// <summary>
    /// Resumes the quiz timer.
    /// </summary>
    public void Resume()
    {
        if (!_isPaused)
            return;

        _isPaused = false;
        _timerStartedAt = DateTime.UtcNow;
        _nextQuizTimer.Start();
    }

    /// <summary>
    /// Opens the options/settings window.
    /// </summary>
    public void OpenOptions()
    {
        OptionsRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Applies updated settings to the application.
    /// </summary>
    public void ApplySettings()
    {
        var settings = _settingsService.GetSettings();
        _nextQuizTimer.Interval = settings.QuizIntervalSeconds * 1000;

        if (!_isPaused)
        {
            _nextQuizTimer.Stop();
            _timerStartedAt = DateTime.UtcNow;
            _nextQuizTimer.Start();
            TimerRestarted?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    public void Exit()
    {
        ExitRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when a quiz is completed, to restart the timer.
    /// </summary>
    public void OnQuizCompleted()
    {
        _isQuizOpen = false;
        if (!_isPaused)
        {
            _timerStartedAt = DateTime.UtcNow;
            _nextQuizTimer.Start();
        }
        QuizClosed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Shows a quiz immediately, unless the application is paused or already opened.
    /// </summary>
    public void ShowQuizNow()
    {
        if (_isPaused || _isQuizOpen)
            return;
        ShowQuiz();
    }

    /// <summary>
    /// Returns the time remaining until the next quiz, or null if paused or the timer is not running.
    /// </summary>
    public TimeSpan? GetTimeUntilNextQuiz()
    {
        if (_isPaused || !_nextQuizTimer.Enabled)
            return null;
        var elapsed = DateTime.UtcNow - _timerStartedAt;
        var remaining = TimeSpan.FromMilliseconds(_nextQuizTimer.Interval) - elapsed;
        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_isPaused)
            return;

        ShowQuiz();
    }

    private void ShowQuiz()
    {
        var settings = _settingsService.GetSettings();
        var session = _quizService.CreateQuizSession(settings.QuizConfiguration, _wordListService);

        if (session == null)
        {
            ErrorOccurred?.Invoke(this, "No words are available. The application cannot run without a word list.\n\nPlease add words to Data/words.csv and restart the application.");
            return;
        }

        _isQuizOpen = true;
        QuizRequested?.Invoke(this, session);
    }

    /// <summary>
    /// Disposes resources used by the service.
    /// </summary>
    public void Dispose()
    {
        _nextQuizTimer.Elapsed -= OnTimerElapsed;
        _nextQuizTimer.Stop();
        _nextQuizTimer.Dispose();
    }
}
