using System;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;

namespace VocabularyTrainer.Services;

/// <summary>
/// Service for managing the system tray icon and menu.
/// </summary>
public class TrayIconService : IDisposable
{
    private const string PauseLabel = "Pause";
    private const string ResumeLabel = "Resume";
    private const string ActiveIconUri = "avares://VocabularyTrainer/Resources/tray.ico";
    private const string PausedIconUri = "avares://VocabularyTrainer/Resources/tray-paused.ico";

    private readonly ApplicationService _applicationService;
    private readonly EventHandler<QuizSession> _onQuizRequested;
    private TrayIcon? _trayIcon;
    private NativeMenuItem? _pauseResumeMenuItem;
    private System.Timers.Timer? _tooltipTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrayIconService"/> class.
    /// </summary>
    /// <param name="applicationService">The application service.</param>
    public TrayIconService(ApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
        _onQuizRequested = (_, _) => UpdateTooltip();
    }

    /// <summary>
    /// Initializes the tray icon with menu items.
    /// </summary>
    /// <param name="trayIcon">The TrayIcon control from XAML.</param>
    public void Initialize(TrayIcon trayIcon)
    {
        _trayIcon = trayIcon ?? throw new ArgumentNullException(nameof(trayIcon));

        // Create menu items
        _pauseResumeMenuItem = new NativeMenuItem
        {
            Header = PauseLabel
        };
        _pauseResumeMenuItem.Click += OnPauseResumeClicked;

        var optionsMenuItem = new NativeMenuItem
        {
            Header = "Options"
        };
        optionsMenuItem.Click += (_, _) => _applicationService.OpenOptions();

        var exitMenuItem = new NativeMenuItem
        {
            Header = "Exit"
        };
        exitMenuItem.Click += (_, _) => _applicationService.Exit();

        // Create menu
        var menu = new NativeMenu();
        menu.Items.Add(_pauseResumeMenuItem);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(optionsMenuItem);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(exitMenuItem);

        _trayIcon.Menu = menu;
        _trayIcon.Clicked += (_, _) => _applicationService.ShowQuizNow();
        _applicationService.QuizRequested += _onQuizRequested;
        _applicationService.QuizClosed += OnApplicationStateChanged;
        _applicationService.TimerRestarted += OnApplicationStateChanged;

        _tooltipTimer = new System.Timers.Timer(10_000) { AutoReset = true };
        _tooltipTimer.Elapsed += (_, _) => UpdateTooltip();
        _tooltipTimer.Start();
        UpdateTooltip();
    }

    private void OnPauseResumeClicked(object? sender, EventArgs e)
    {
        if (_applicationService.IsPaused)
        {
            _applicationService.Resume();
            if (_pauseResumeMenuItem != null)
                _pauseResumeMenuItem.Header = PauseLabel;
            SetTrayIcon(ActiveIconUri);
            _tooltipTimer?.Start();
        }
        else
        {
            _applicationService.Pause();
            if (_pauseResumeMenuItem != null)
                _pauseResumeMenuItem.Header = ResumeLabel;
            SetTrayIcon(PausedIconUri);
            _tooltipTimer?.Stop();
        }
        UpdateTooltip();
    }

    private void OnApplicationStateChanged(object? sender, EventArgs e) => UpdateTooltip();

    private void SetTrayIcon(string uri)
    {
        if (_trayIcon == null) return;
        WindowIcon icon;
        using (var stream = AssetLoader.Open(new Uri(uri)))
            icon = new WindowIcon(stream);
        Dispatcher.UIThread.Post(() => _trayIcon.Icon = icon);
    }

    private void UpdateTooltip()
    {
        if (_trayIcon == null) return;
        string text;
        if (_applicationService.IsPaused)
            text = "VocabularyTrainer (Paused)";
        else
        {
            var remaining = _applicationService.GetTimeUntilNextQuiz();
            text = remaining == null
                ? "VocabularyTrainer"
                : remaining.Value.TotalMinutes < 1
                    ? "VocabularyTrainer \u2014 next quiz in less than a minute"
                    : $"VocabularyTrainer \u2014 next quiz in approximately {(int)Math.Ceiling(remaining.Value.TotalMinutes)} min";
        }
        Dispatcher.UIThread.Post(() => _trayIcon.ToolTipText = text);
    }

    /// <summary>
    /// Disposes resources used by the service.
    /// </summary>
    public void Dispose()
    {
        _applicationService.QuizRequested -= _onQuizRequested;
        _applicationService.QuizClosed -= OnApplicationStateChanged;
        _applicationService.TimerRestarted -= OnApplicationStateChanged;
        _tooltipTimer?.Stop();
        _tooltipTimer?.Dispose();
    }
}
