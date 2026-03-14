using System.IO;
using System.Text.Json;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

/// <summary>
/// Service responsible for loading, storing, and managing application settings.
/// </summary>
public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings _currentSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class.
    /// </summary>
    /// <param name="settingsPath">The file path to the settings JSON file.</param>
    public SettingsService(string settingsPath)
    {
        _settingsPath = settingsPath;
        _currentSettings = Load();
    }

    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    /// <returns>The current <see cref="AppSettings"/> instance.</returns>
    public AppSettings GetSettings()
    {
        return _currentSettings;
    }

    /// <summary>
    /// Updates the application settings with new values and persists them to disk.
    /// Creates a new immutable <see cref="AppSettings"/> instance with updated values.
    /// </summary>
    /// <param name="quizIntervalSeconds">Quiz interval in seconds.</param>
    /// <param name="autoCloseAfterCorrectSeconds">Auto-close delay in seconds.</param>
    /// <param name="optionCount">Number of answer options.</param>
    /// <param name="direction">Quiz direction mode.</param>
    public void UpdateSettings(int quizIntervalSeconds, int autoCloseAfterCorrectSeconds, int optionCount, QuizDirection direction)
    {
        _currentSettings = new AppSettings
        {
            QuizIntervalSeconds = quizIntervalSeconds,
            QuizConfiguration = new QuizConfiguration
            {
                OptionCount = optionCount,
                AutoCloseAfterCorrectSeconds = autoCloseAfterCorrectSeconds,
                ShowCorrectAnswerOnWrong = _currentSettings.QuizConfiguration.ShowCorrectAnswerOnWrong,
                MaxAttemptsPerQuiz = _currentSettings.QuizConfiguration.MaxAttemptsPerQuiz,
                Direction = direction
            }
        };

        Save(_currentSettings);
    }

    private AppSettings Load()
    {
        if (!File.Exists(_settingsPath))
        {
            return CreateDefault();
        }

        var json = File.ReadAllText(_settingsPath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return CreateDefault();
        }

        return JsonSerializer.Deserialize<AppSettings>(json) ?? CreateDefault();
    }

    private void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_settingsPath, json);
    }

    private static AppSettings CreateDefault()
    {
        return new AppSettings
        {
            QuizIntervalSeconds = 300,
            QuizConfiguration = new QuizConfiguration
            {
                OptionCount = 3,
                AutoCloseAfterCorrectSeconds = 5
            }
        };
    }
}
