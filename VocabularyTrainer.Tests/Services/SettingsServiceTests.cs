using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;
using Xunit;

namespace VocabularyTrainer.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();

    public void Dispose()
    {
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
    }

    // ── Defaults ──────────────────────────────────────────────────────────────

    [Fact]
    public void GetSettings_ReturnsDefaults_WhenFileDoesNotExist()
    {
        File.Delete(_tempFile);
        var service = new SettingsService(_tempFile);

        var settings = service.GetSettings();

        settings.QuizIntervalSeconds.Should().Be(300);
        settings.QuizConfiguration.OptionCount.Should().Be(3);
        settings.QuizConfiguration.AutoCloseAfterCorrectSeconds.Should().Be(5);
        settings.QuizConfiguration.ShowCorrectAnswerOnWrong.Should().BeFalse();
        settings.QuizConfiguration.MaxAttemptsPerQuiz.Should().BeNull();
        settings.QuizConfiguration.Direction.Should().Be(QuizDirection.Direct);
    }

    [Fact]
    public void GetSettings_ThrowsException_WhenFileIsCorrupt()
    {
        File.WriteAllText(_tempFile, "not valid json {{{{");

        var act = () => new SettingsService(_tempFile);

        act.Should().Throw<Exception>();
    }

    // ── UpdateSettings ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateSettings_PersistsValues_RoundTrip()
    {
        var service = new SettingsService(_tempFile);
        service.UpdateSettings(600, 10, 4, QuizDirection.Reverse, QuizDifficulty.Easy);

        // Re-create from same file to verify persistence
        var reloaded = new SettingsService(_tempFile);
        var settings = reloaded.GetSettings();

        settings.QuizIntervalSeconds.Should().Be(600);
        settings.QuizConfiguration.AutoCloseAfterCorrectSeconds.Should().Be(10);
        settings.QuizConfiguration.OptionCount.Should().Be(4);
        settings.QuizConfiguration.Direction.Should().Be(QuizDirection.Reverse);
        settings.QuizConfiguration.ShowCorrectAnswerOnWrong.Should().BeFalse();
        settings.QuizConfiguration.MaxAttemptsPerQuiz.Should().BeNull();
    }

    [Fact]
    public void GetSettings_LoadsAllProperties_FromExistingFile()
    {
        var json = """
            {
              "QuizIntervalSeconds": 120,
              "QuizConfiguration": {
                "OptionCount": 5,
                "AutoCloseAfterCorrectSeconds": 8,
                "ShowCorrectAnswerOnWrong": true,
                "MaxAttemptsPerQuiz": 2,
                "Direction": 1
              }
            }
            """;
        File.WriteAllText(_tempFile, json);

        var settings = new SettingsService(_tempFile).GetSettings();

        settings.QuizIntervalSeconds.Should().Be(120);
        settings.QuizConfiguration.OptionCount.Should().Be(5);
        settings.QuizConfiguration.AutoCloseAfterCorrectSeconds.Should().Be(8);
        settings.QuizConfiguration.ShowCorrectAnswerOnWrong.Should().BeTrue();
        settings.QuizConfiguration.MaxAttemptsPerQuiz.Should().Be(2);
        settings.QuizConfiguration.Direction.Should().Be(QuizDirection.Reverse);
    }

    [Fact]
    public void UpdateSettings_PreservesUnchangedFields() 
    {
        var json = """
            {
              "QuizIntervalSeconds": 300,
              "QuizConfiguration": {
                "OptionCount": 3,
                "AutoCloseAfterCorrectSeconds": 5,
                "ShowCorrectAnswerOnWrong": true,
                "MaxAttemptsPerQuiz": 3,
                "Direction": 0
              }
            }
            """;
        File.WriteAllText(_tempFile, json);

        var service = new SettingsService(_tempFile);
        service.UpdateSettings(600, 10, 4, QuizDirection.Reverse, QuizDifficulty.Easy);

        var settings = service.GetSettings();
        settings.QuizConfiguration.ShowCorrectAnswerOnWrong.Should().BeTrue();
        settings.QuizConfiguration.MaxAttemptsPerQuiz.Should().Be(3);
    }
}
