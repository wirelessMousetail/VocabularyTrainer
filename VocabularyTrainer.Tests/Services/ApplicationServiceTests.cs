using System;
using System.Collections.Generic;
using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;
using VocabularyTrainer.Services.Quiz;
using VocabularyTrainer.Services.Vocabulary;
using VocabularyTrainer.Tests.Fakes;
using VocabularyTrainer.Tests.Fixtures;
using Xunit;

namespace VocabularyTrainer.Tests.Services;

public class ApplicationServiceTests : IDisposable
{
    private readonly string _settingsFile = Path.GetTempFileName();
    private readonly string _precompiledPath = Path.GetTempFileName();
    private readonly string _managedPath = Path.GetTempFileName();

    public ApplicationServiceTests()
    {
        // Write minimal valid settings JSON
        File.WriteAllText(_settingsFile, """
            {
              "QuizIntervalSeconds": 300,
              "QuizConfiguration": {
                "OptionCount": 3,
                "AutoCloseAfterCorrectSeconds": 5,
                "ShowCorrectAnswerOnWrong": false,
                "Direction": 0,
                "Difficulty": 0
              }
            }
            """);

        // Write four words in the same group so CreateQuizSession can produce options
        File.WriteAllLines(_precompiledPath,
        [
            "hond;dog",
            "kat;cat",
            "vis;fish",
            "vogel;bird"
        ]);

        // Empty managed file — LoadAndMerge will populate it
        File.WriteAllText(_managedPath, string.Empty);
    }

    public void Dispose()
    {
        if (File.Exists(_settingsFile)) File.Delete(_settingsFile);
        if (File.Exists(_precompiledPath)) File.Delete(_precompiledPath);
        if (File.Exists(_managedPath)) File.Delete(_managedPath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private ApplicationService BuildService(FakeTimer timer, IReadOnlyList<WordEntry>? words = null)
    {
        var settingsService = new SettingsService(_settingsFile);

        WordListService wordListService;
        if (words != null)
        {
            // Build word list from provided words via a temp precompiled file
            var tempPrecompiled = Path.GetTempFileName();
            foreach (var w in words)
                File.AppendAllLines(tempPrecompiled, [$"{w.Question};{w.Answer}"]);
            var tempManaged = Path.GetTempFileName();
            File.WriteAllText(tempManaged, string.Empty);
            wordListService = new WordListService(tempPrecompiled, tempManaged);
        }
        else
        {
            wordListService = new WordListService(_precompiledPath, _managedPath);
        }

        var wordList = wordListService.LoadAndMerge();
        var quizService = new QuizService(wordList, new WordWeightStrategy(), new EasyDistractorSelector());

        return new ApplicationService(settingsService, wordListService, quizService, timer);
    }

    private ApplicationService BuildService() => BuildService(new FakeTimer());

    // ── Start ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Start_ShowsQuiz_Immediately()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);
        int quizCount = 0;
        service.QuizRequested += (_, _) => quizCount++;

        service.Start();

        quizCount.Should().Be(1);
    }

    // ── Timer fire ────────────────────────────────────────────────────────────

    [Fact]
    public void Timer_Fire_ShowsQuiz()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);
        int quizCount = 0;
        service.QuizRequested += (_, _) => quizCount++;

        service.Start();           // fires first quiz; isQuizOpen = true
        service.OnQuizCompleted(); // restarts timer; isQuizOpen = false
        timer.Fire();              // fires second quiz

        quizCount.Should().Be(2);
    }

    // ── Pause ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Pause_StopsTimer()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);

        service.Start();
        service.OnQuizCompleted(); // timer is now running

        timer.Enabled.Should().BeTrue();

        service.Pause();

        timer.Enabled.Should().BeFalse();
    }

    // ── Resume ────────────────────────────────────────────────────────────────

    [Fact]
    public void Resume_WithoutPause_DoesNothing()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);

        service.Start();
        service.OnQuizCompleted(); // timer running, not paused

        service.Resume(); // should be a no-op — not paused

        service.IsPaused.Should().BeFalse();
        timer.Enabled.Should().BeTrue();
    }

    [Fact]
    public void Resume_AfterPause_StartsTimer()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);

        service.Start();
        service.OnQuizCompleted();
        service.Pause();

        timer.Enabled.Should().BeFalse();

        service.Resume();

        timer.Enabled.Should().BeTrue();
    }

    // ── OnQuizCompleted ───────────────────────────────────────────────────────

    [Fact]
    public void OnQuizCompleted_RestartsTimer_WhenNotPaused()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);

        service.Start(); // isQuizOpen = true, timer stopped

        timer.Enabled.Should().BeFalse();

        service.OnQuizCompleted();

        timer.Enabled.Should().BeTrue();
    }

    [Fact]
    public void OnQuizCompleted_DoesNotRestartTimer_WhenPaused()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);

        service.Start();
        service.Pause();

        service.OnQuizCompleted();

        timer.Enabled.Should().BeFalse();
    }

    [Fact]
    public void OnQuizCompleted_FiresQuizClosedEvent()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);
        bool quizClosedFired = false;
        service.QuizClosed += (_, _) => quizClosedFired = true;

        service.OnQuizCompleted();

        quizClosedFired.Should().BeTrue();
    }

    // ── ApplySettings ─────────────────────────────────────────────────────────

    [Fact]
    public void ApplySettings_RestartsTimer_WhenNotPausedAndNoQuizOpen()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);
        bool timerRestartedFired = false;
        service.TimerRestarted += (_, _) => timerRestartedFired = true;

        service.OnQuizCompleted(); // no quiz open; not paused
        service.ApplySettings();

        timerRestartedFired.Should().BeTrue();
        timer.Enabled.Should().BeTrue();
    }

    [Fact]
    public void ApplySettings_OnlyUpdatesInterval_WhenPaused()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);
        bool timerRestartedFired = false;
        service.TimerRestarted += (_, _) => timerRestartedFired = true;

        service.Pause();
        service.ApplySettings();

        timerRestartedFired.Should().BeFalse();
        timer.Enabled.Should().BeFalse();
    }

    [Fact]
    public void ApplySettings_OnlyUpdatesInterval_WhenQuizIsOpen()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);
        bool timerRestartedFired = false;
        service.TimerRestarted += (_, _) => timerRestartedFired = true;

        service.Start(); // isQuizOpen = true
        service.ApplySettings();

        timerRestartedFired.Should().BeFalse();
    }

    // ── GetTimeUntilNextQuiz ──────────────────────────────────────────────────

    [Fact]
    public void GetTimeUntilNextQuiz_ReturnsNull_WhenPaused()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);

        service.Pause();

        service.GetTimeUntilNextQuiz().Should().BeNull();
    }

    [Fact]
    public void GetTimeUntilNextQuiz_ReturnsNull_WhenTimerNotRunning()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);

        // Fresh service — timer never started
        service.GetTimeUntilNextQuiz().Should().BeNull();
    }

    [Fact]
    public void GetTimeUntilNextQuiz_ReturnsPositiveValue_WhenRunning()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);

        service.OnQuizCompleted(); // starts timer, records _timerStartedAt

        var remaining = service.GetTimeUntilNextQuiz();

        remaining.Should().NotBeNull();
        remaining!.Value.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
        remaining.Value.Should().BeLessThanOrEqualTo(TimeSpan.FromMilliseconds(timer.Interval));
    }

    // ── ShowQuizNow ───────────────────────────────────────────────────────────

    [Fact]
    public void ShowQuizNow_DoesNothing_WhenPaused()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);
        int quizCount = 0;
        service.QuizRequested += (_, _) => quizCount++;

        service.Pause();
        service.ShowQuizNow();

        quizCount.Should().Be(0);
    }

    [Fact]
    public void ShowQuizNow_DoesNothing_WhenQuizAlreadyOpen()
    {
        var timer = new FakeTimer();
        var service = BuildService(timer);

        service.Start(); // opens quiz (isQuizOpen = true)

        int quizCount = 0;
        service.QuizRequested += (_, _) => quizCount++;

        service.ShowQuizNow();

        quizCount.Should().Be(0);
    }

    // ── Error handling ────────────────────────────────────────────────────────

    [Fact]
    public void EmptyWordList_FiresErrorOccurred()
    {
        var timer = new FakeTimer();

        var settingsService = new SettingsService(_settingsFile);
        var tempPrecompiled = Path.GetTempFileName();
        var tempManaged = Path.GetTempFileName();
        // Empty precompiled CSV — no words
        File.WriteAllText(tempPrecompiled, string.Empty);
        File.WriteAllText(tempManaged, string.Empty);

        var wordListService = new WordListService(tempPrecompiled, tempManaged);
        var wordList = wordListService.LoadAndMerge();
        var quizService = new QuizService(wordList, new WordWeightStrategy(), new EasyDistractorSelector());

        var service = new ApplicationService(settingsService, wordListService, quizService, timer);

        bool errorFired = false;
        service.ErrorOccurred += (_, _) => errorFired = true;

        service.Start();

        errorFired.Should().BeTrue();

        File.Delete(tempPrecompiled);
        File.Delete(tempManaged);
    }
}
