using Avalonia.Controls;
using VocabularyTrainer.Services;

namespace VocabularyTrainer.Services;

/// <summary>
/// Service for managing the system tray icon and menu.
/// </summary>
public class TrayIconService
{
    private readonly ApplicationService _applicationService;
    private TrayIcon? _trayIcon;
    private NativeMenuItem? _pauseResumeMenuItem;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrayIconService"/> class.
    /// </summary>
    /// <param name="applicationService">The application service.</param>
    public TrayIconService(ApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
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
            Header = "Pause"
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
    }

    private void OnPauseResumeClicked(object? sender, EventArgs e)
    {
        if (_applicationService.IsPaused)
        {
            _applicationService.Resume();
            if (_pauseResumeMenuItem != null)
            {
                _pauseResumeMenuItem.Header = "Pause";
            }
        }
        else
        {
            _applicationService.Pause();
            if (_pauseResumeMenuItem != null)
            {
                _pauseResumeMenuItem.Header = "Resume";
            }
        }
    }
}
