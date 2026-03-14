using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;
using VocabularyTrainer.Tests.Fixtures;
using Xunit;

namespace VocabularyTrainer.Tests.Services;

public class QuizPresenterTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();
    private readonly WordListService _wordListService;
    private readonly WordWeightStrategy _strategy = new();

    public QuizPresenterTests()
    {
        // WordListService with no precompiled path — only SaveWords() will be called,
        // which writes the (empty) internal word list to the temp file.
        _wordListService = new WordListService(string.Empty, _tempFile);
    }

    public void Dispose() => File.Delete(_tempFile);

    // ── Correct answer ────────────────────────────────────────────────────────

    [Fact]
    public void CorrectAnswer_SetsResultToCorrect()
    {
        var (presenter, quiz) = MakePresenter("dog");
        presenter.OnAnswerSelected(quiz.CorrectAnswer);
        presenter.GetResult().Should().Be(QuizResult.Correct);
    }

    [Fact]
    public void CorrectAnswer_DecreasesWeight()
    {
        var word = WordEntryFixture.Make("hond", "dog", weight: 5);
        var quiz = MakeQuiz("hond", "dog", ["cat", "fish"], word);
        var presenter = new QuizPresenter(quiz, _strategy, _wordListService);

        presenter.OnAnswerSelected("dog");

        word.WeightData.Weight.Should().BeLessThan(5);
    }

    [Fact]
    public void SubsequentCallsAfterCorrect_AreIgnored()
    {
        var (presenter, quiz) = MakePresenter("dog");
        presenter.OnAnswerSelected(quiz.CorrectAnswer);
        presenter.OnAnswerSelected("wrong");
        presenter.GetResult().Should().Be(QuizResult.Correct);
    }

    // ── Wrong answer ──────────────────────────────────────────────────────────

    [Fact]
    public void WrongAnswer_SetsResultToWrong()
    {
        var (presenter, _) = MakePresenter("dog");
        presenter.OnAnswerSelected("cat");
        presenter.GetResult().Should().Be(QuizResult.Wrong);
    }

    [Fact]
    public void WrongAnswer_IncreasesWeight()
    {
        var word = WordEntryFixture.Make("hond", "dog", weight: 0);
        var quiz = MakeQuiz("hond", "dog", ["cat", "fish"], word);
        var presenter = new QuizPresenter(quiz, _strategy, _wordListService);

        presenter.OnAnswerSelected("cat");

        word.WeightData.Weight.Should().BeGreaterThan(0);
    }

    [Fact]
    public void WrongAnswer_ResetsStreak()
    {
        var word = WordEntryFixture.Make("hond", "dog", weight: 0, streak: 3);
        var quiz = MakeQuiz("hond", "dog", ["cat", "fish"], word);
        var presenter = new QuizPresenter(quiz, _strategy, _wordListService);

        presenter.OnAnswerSelected("cat");

        word.WeightData.CorrectStreak.Should().Be(0);
    }

    // ── Max attempts ──────────────────────────────────────────────────────────

    [Fact]
    public void WrongAnswer_NeverReachesMaxAttempts_WhenMaxAttemptsIsNull()
    {
        var (presenter, _) = MakePresenter("dog", maxAttempts: null);
        presenter.OnAnswerSelected("wrong");
        presenter.OnAnswerSelected("wrong");
        presenter.OnAnswerSelected("wrong");
        presenter.GetResult().Should().Be(QuizResult.Wrong);
    }

    [Fact]
    public void MaxAttemptsReached_AfterConfiguredWrongAnswers()
    {
        var (presenter, _) = MakePresenter("dog", maxAttempts: 2);
        presenter.OnAnswerSelected("wrong");
        presenter.GetResult().Should().Be(QuizResult.Wrong);

        presenter.OnAnswerSelected("wrong");
        presenter.GetResult().Should().Be(QuizResult.MaxAttemptsReached);
    }

    [Fact]
    public void SubsequentCallsAfterMaxAttempts_AreIgnored()
    {
        var (presenter, _) = MakePresenter("dog", maxAttempts: 1);
        presenter.OnAnswerSelected("wrong");
        presenter.GetResult().Should().Be(QuizResult.MaxAttemptsReached);

        presenter.OnAnswerSelected("dog"); // correct, but should be ignored
        presenter.GetResult().Should().Be(QuizResult.MaxAttemptsReached);
    }

    // ── GetCorrectAnswer ──────────────────────────────────────────────────────

    [Fact]
    public void GetCorrectAnswer_ReturnsQuizCorrectAnswer()
    {
        var (presenter, quiz) = MakePresenter("dog");
        presenter.GetCorrectAnswer().Should().Be(quiz.CorrectAnswer);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private (QuizPresenter presenter, Quiz quiz) MakePresenter(
        string correctAnswer, int? maxAttempts = null)
    {
        var word = WordEntryFixture.Make("hond", correctAnswer);
        var quiz = MakeQuiz("hond", correctAnswer, ["cat", "fish"], word);
        var presenter = new QuizPresenter(quiz, _strategy, _wordListService, maxAttempts);
        return (presenter, quiz);
    }

    private static Quiz MakeQuiz(
        string question,
        string correctAnswer,
        IEnumerable<string> otherOptions,
        WordEntry? word = null)
    {
        word ??= WordEntryFixture.Make(question, correctAnswer);
        var options = otherOptions.Append(correctAnswer).ToList();
        return new Quiz(question, correctAnswer, options, word);
    }
}
