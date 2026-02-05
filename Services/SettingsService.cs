using System.IO;
using System.Text.Json;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings _currentSettings;

    public SettingsService(string settingsPath)
    {
        _settingsPath = settingsPath;
        _currentSettings = Load();
    }

    public AppSettings GetSettings()
    {
        return _currentSettings;
    }

    public void UpdateSettings(int quizIntervalSeconds, int autoCloseAfterCorrectSeconds, int optionCount)
    {
        _currentSettings = new AppSettings
        {
            QuizIntervalSeconds = quizIntervalSeconds,
            QuizConfiguration = new QuizConfiguration
            {
                OptionCount = optionCount,
                AutoCloseAfterCorrectSeconds = autoCloseAfterCorrectSeconds,
                ShowCorrectAnswerOnWrong = _currentSettings.QuizConfiguration.ShowCorrectAnswerOnWrong,
                MaxAttemptsPerQuiz = _currentSettings.QuizConfiguration.MaxAttemptsPerQuiz
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

        try
        {
            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? CreateDefault();
        }
        catch
        {
            //todo add logging here
            return CreateDefault();
        }
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
