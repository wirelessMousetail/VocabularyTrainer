using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;
using VocabularyTrainer.ViewModels;
using VocabularyTrainer.Views;

namespace VocabularyTrainer;

public partial class App : Application
{
    private ApplicationService? _applicationService;
    private SettingsService? _settingsService;
    private TrayIconService? _trayIconService;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Set shutdown mode to manual - app won't exit when windows are closed
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Initialize services
            var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            _settingsService = new SettingsService(settingsPath);

            // Create application service
            _applicationService = new ApplicationService(_settingsService);

            // Subscribe to events
            _applicationService.QuizRequested += OnQuizRequested;
            _applicationService.OptionsRequested += OnOptionsRequested;
            _applicationService.ExitRequested += OnExitRequested;

            // Initialize tray icon
            var trayIcon = TrayIcon.GetIcons(this)?.FirstOrDefault();
            if (trayIcon != null && _applicationService != null)
            {
                _trayIconService = new TrayIconService(_applicationService);
                _trayIconService.Initialize(trayIcon);
            }

            // Handle application shutdown
            desktop.ShutdownRequested += (_, _) =>
            {
                _applicationService?.Dispose();
            };

            // Start the application service
            _applicationService.Start();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnQuizRequested(object? sender, QuizSession session)
    {
        Dispatcher.UIThread.Post(() =>
        {
            QuizView? window = null;

            var viewModel = new QuizViewModel(session, () =>
            {
                // Close the window
                window?.Close();
                // Notify application service to start next quiz timer
                _applicationService?.OnQuizCompleted();
            });

            window = new QuizView
            {
                DataContext = viewModel
            };

            window.Show();
        });
    }

    private void OnOptionsRequested(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_applicationService == null || _settingsService == null)
                return;

            OptionsView? window = null;

            var viewModel = new OptionsViewModel(
                _settingsService,
                () => _applicationService.ApplySettings(),
                () => window?.Close() // Close the window
            );

            window = new OptionsView
            {
                DataContext = viewModel
            };

            window.Show();
        });
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        });
    }
}
