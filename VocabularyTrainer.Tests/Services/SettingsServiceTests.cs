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
        settings.QuizConfiguration.AutoCloseAfterCorrectSeconds.Should().Be(5);//todo check all properties 
    }

    [Fact]
    public void GetSettings_ReturnsDefaults_WhenFileIsCorrupt()
    {
        File.WriteAllText(_tempFile, "not valid json {{{{");
        var service = new SettingsService(_tempFile);

        service.GetSettings().QuizIntervalSeconds.Should().Be(300); //todo let's fail if corrupt
    }

    // ── UpdateSettings ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateSettings_PersistsValues_RoundTrip()
    {
        var service = new SettingsService(_tempFile);
        service.UpdateSettings(600, 10, 4, QuizDirection.Reverse);

        // Re-create from same file to verify persistence
        var reloaded = new SettingsService(_tempFile);
        var settings = reloaded.GetSettings();

        settings.QuizIntervalSeconds.Should().Be(600);
        settings.QuizConfiguration.AutoCloseAfterCorrectSeconds.Should().Be(10);
        settings.QuizConfiguration.OptionCount.Should().Be(4);
        settings.QuizConfiguration.Direction.Should().Be(QuizDirection.Reverse); //todo check all properties
    }
    
    //todo test that loads existing file correctly (json string as an input)

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
        service.UpdateSettings(600, 10, 4, QuizDirection.Reverse);

        var settings = service.GetSettings();
        settings.QuizConfiguration.ShowCorrectAnswerOnWrong.Should().BeTrue();
        settings.QuizConfiguration.MaxAttemptsPerQuiz.Should().Be(3);
    }
}
