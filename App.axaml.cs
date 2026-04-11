using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;
using VocabularyTrainer.Services.Quiz.Distractors;
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
            try
            {
                _settingsService = new SettingsService(settingsPath);
            }
            catch (Exception ex)
            {
                ShowFatalError($"The settings file could not be read.\n\n{ex.Message}\n\nPlease fix or delete 'appsettings.json' and restart.");
                base.OnFrameworkInitializationCompleted();
                return;
            }

            // Create application service
            ApplicationService appService;
            try
            {
                appService = new ApplicationService(_settingsService);
            }
            catch (Exception ex) when (ex is InvalidDataException or FormatException or IOException)
            {
                ShowFatalError(ex.Message);
                base.OnFrameworkInitializationCompleted();
                return;
            }
            _applicationService = appService;

            // Subscribe to events
            appService.QuizRequested += OnQuizRequested;
            appService.OptionsRequested += OnOptionsRequested;
            appService.ExitRequested += OnExitRequested;
            appService.ErrorOccurred += (_, message) => ShowFatalError(message);

            // Initialize tray icon
            var trayIcon = TrayIcon.GetIcons(this)?.FirstOrDefault();
            if (trayIcon != null)
            {
                _trayIconService = new TrayIconService(appService);
                _trayIconService.Initialize(trayIcon);
            }

            // Handle application shutdown
            desktop.ShutdownRequested += (_, _) =>
            {
                appService.Dispose();
            };

            // Start the application service
            appService.Start();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnQuizRequested(object? sender, QuizSession session)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window? window = null;
            bool completedNotified = false;

            void NotifyCompleted()
            {
                if (completedNotified) return;
                completedNotified = true;
                _applicationService!.OnQuizCompleted();
            }

            if (session.Configuration.Difficulty.IsTypingMode())
            {
                var vm = new TypingQuizViewModel(session, () =>
                {
                    window?.Close();
                    NotifyCompleted();
                });
                window = new TypingQuizView { DataContext = vm };
            }
            else
            {
                var vm = new QuizViewModel(session, () =>
                {
                    window?.Close();
                    NotifyCompleted();
                });
                window = new QuizView { DataContext = vm };
            }

            // Ensure timer restarts even when window is closed via OS controls
            window.Closed += (_, _) => NotifyCompleted();

            window.Show();
        });
    }

    private void OnOptionsRequested(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var appService = _applicationService;
            var settingsService = _settingsService;

            if (appService == null || settingsService == null)
                return;

            OptionsView? window = null;

            var viewModel = new OptionsViewModel(
                settingsService.GetSettings(),
                (interval, autoClose, count, dir, diff, revealLetters) =>
                {
                    settingsService.UpdateSettings(interval, autoClose, count, dir, diff, revealLetters);
                    appService.ApplySettings();
                },
                () => window?.Close()
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

    private void ShowFatalError(string message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var button = new Button
            {
                Content = "OK",
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var window = new Window
            {
                Title = "VocabularyTrainer — Error",
                Width = 440,
                Height = 170,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new StackPanel
                {
                    Margin = new Thickness(20),
                    Spacing = 16,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap
                        },
                        button
                    }
                }
            };

            button.Click += (_, _) => window.Close();
            window.Closed += (_, _) =>
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    desktop.Shutdown();
            };

            window.Show();
        });
    }
}
