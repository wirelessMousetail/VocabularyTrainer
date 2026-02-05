using VocabularyTrainer.Services;
using Timer = System.Windows.Forms.Timer;
using VocabularyTrainer.Infrastructure;
using VocabularyTrainer.Models;


namespace VocabularyTrainer;

public class VocabApplicationContext : ApplicationContext
{
    private readonly QuizService _quizService;
    private readonly SettingsService _settingsService;
    private readonly Timer _nextQuizTimer;
    private readonly TrayIconManager _trayIcon;
    private bool _isPaused;


    public VocabApplicationContext(SettingsService settingsService)
    {
        _settingsService = settingsService;
        
        var csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "words.csv");

        var repository = new CsvWordRepository();
        var words = repository.Load(csvPath);

        _quizService = new QuizService(words);

        _nextQuizTimer = new Timer();
        _nextQuizTimer.Interval = _settingsService.GetSettings().QuizIntervalSeconds * 1000;
        _nextQuizTimer.Tick += (_, _) =>
        {
            _nextQuizTimer.Stop();
            if (_isPaused)
                return;
            ShowQuiz();
        };

        // Start first quiz immediately
        ShowQuiz();

        _trayIcon = new TrayIconManager(Pause, Resume, OpenOptions, ExitApplication);
    }

    public void Pause()
    {
        _isPaused = true;
        _nextQuizTimer.Stop();
        _trayIcon.SetPaused(true);
    }

    public void Resume()
    {
        if (!_isPaused)
            return;

        _isPaused = false;
        _trayIcon.SetPaused(false);

        _nextQuizTimer.Start();
    }

    private void ShowQuiz()
    {
        var settings = _settingsService.GetSettings();
        var session = _quizService.CreateQuizSession(settings.QuizConfiguration);
        var form = new QuizForm(session);

        form.FormClosed += (_, _) =>
        {
            if (!_isPaused)
            {
                _nextQuizTimer.Start();
            }
        };

        form.Show();
    }

    private void ExitApplication()
    {
        _trayIcon.Dispose();
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _nextQuizTimer.Dispose();
            _trayIcon.Dispose();
        }

        base.Dispose(disposing);
    }
    
    private void OpenOptions()
    {
        using var form = new OptionsForm(_settingsService, ApplySettings);
        form.ShowDialog();
    }
    
    private void ApplySettings()
    {
        var settings = _settingsService.GetSettings();
        _nextQuizTimer.Interval = settings.QuizIntervalSeconds * 1000;

        if (!_isPaused)
        {
            _nextQuizTimer.Stop();
            _nextQuizTimer.Start();
        }
    }
}