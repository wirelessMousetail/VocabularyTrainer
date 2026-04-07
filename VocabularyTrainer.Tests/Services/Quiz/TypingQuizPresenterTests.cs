using System.Collections.Generic;
using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services.Quiz;
using VocabularyTrainer.Services.Vocabulary;
using VocabularyTrainer.Tests.Fixtures;
using Xunit;
using QuizModel = VocabularyTrainer.Models.Quiz;

namespace VocabularyTrainer.Tests.Services.Quiz;

public class TypingQuizPresenterTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();
    private readonly WordListService _wordListService;
    private readonly WordWeightStrategy _strategy = new();

    public TypingQuizPresenterTests()
    {
        _wordListService = new WordListService(string.Empty, _tempFile);
    }

    public void Dispose() => File.Delete(_tempFile);

    // ── Single-attempt result ─────────────────────────────────────────────────

    [Theory]
    [InlineData("de hond", "dog",    "dog",      QuizResult.Correct)]       // exact match
    [InlineData("de hond", "dog",    "DOG",      QuizResult.Correct)]       // case insensitive
    [InlineData("de hond", "dog",    "  dog  ",  QuizResult.Correct)]       // surrounding whitespace
    [InlineData("de hond", "dog",    "the dog",  QuizResult.Correct)]       // leading "the"
    [InlineData("de hond", "dog",    "a dog",    QuizResult.Correct)]       // leading "a"
    [InlineData("de appel", "apple", "an apple", QuizResult.Correct)]       // leading "an"
    [InlineData("de hond", "dog",    "cat",      QuizResult.Wrong)]         // wrong answer
    [InlineData("hond",    "de hond","het hond", QuizResult.WrongArticle)]  // wrong Dutch article
    [InlineData("hond",    "de hond","de hond",  QuizResult.Correct)]       // correct Dutch article
    [InlineData("hond",    "de hond","hond",     QuizResult.Wrong)]         // no article typed (not WrongArticle)
    public void SingleAttempt_Result(string question, string correct, string typed, QuizResult expected)
    {
        var presenter = MakePresenter(question, correct);
        presenter.OnAnswerSelected(typed);
        presenter.GetResult().Should().Be(expected);
    }

    // ── State after first result ──────────────────────────────────────────────

    [Fact]
    public void CorrectAfterWrong_SetsResultToCorrect()
    {
        var presenter = MakePresenter("de hond", "dog");
        presenter.OnAnswerSelected("cat");
        presenter.GetResult().Should().Be(QuizResult.Wrong);

        presenter.OnAnswerSelected("dog");
        presenter.GetResult().Should().Be(QuizResult.Correct);
    }

    [Fact]
    public void SubsequentCallsAfterCorrect_AreIgnored()
    {
        var presenter = MakePresenter("de hond", "dog");
        presenter.OnAnswerSelected("dog");
        presenter.GetResult().Should().Be(QuizResult.Correct);

        presenter.OnAnswerSelected("cat");
        presenter.GetResult().Should().Be(QuizResult.Correct);
    }

    // ── GetHint — returns null ────────────────────────────────────────────────

    [Theory]
    [InlineData(false, "de hond", "dog",    "cat")]    // reveal disabled, no partial match
    [InlineData(false, "nice",    "bekend", "bekent")] // reveal disabled, partial match that would open gate — still null
    [InlineData(true,  "de hond", "dog",    null)]     // reveal enabled but no attempt yet
    public void GetHint_ReturnsNull(bool revealLetters, string question, string correct, string? typed)
    {
        var presenter = MakePresenter(question, correct, revealLetters);
        if (typed != null)
            presenter.OnAnswerSelected(typed);
        presenter.GetHint().Should().BeNull();
    }

    // ── GetHint — letter reveal ───────────────────────────────────────────────

    [Theory]
    // substitution: "bekent" vs "bekend" — 5 matching chars then substitution
    [InlineData("nice", "bekend",   "bekent",   "beken_")]
    // deletion: "bezeten" vs "bezetten" — user dropped one 't'
    [InlineData("nice", "bezetten", "bezeten",  "bezet_en")]
    // gate opens on "etten"(5): isolated "b" also revealed
    [InlineData("nice", "bezetten", "bisetten", "b__etten")]
    // space always visible; block "de h"(4) opens gate
    [InlineData("hond", "de hond",  "de hand",  "de h_nd")]
    public void RevealLetters_GateOpen_ShowsMatchedChars(string question, string correct, string typed, string expectedHint)
    {
        var presenter = MakePresenter(question, correct, revealLetters: true);
        presenter.OnAnswerSelected(typed);
        presenter.GetHint().Should().Be(expectedHint);
    }

    [Fact]
    public void RevealLetters_MaskAccumulates_BetterAttemptExpandsReveal()
    {
        // First attempt produces a reveal; weaker second attempt must not shrink it
        var presenter = MakePresenter("nice", "bekend", revealLetters: true);
        presenter.OnAnswerSelected("bekent"); // → "beken_"
        var firstHint = presenter.GetHint();
        firstHint.Should().Be("beken_");

        presenter.OnAnswerSelected("bek___"); // weaker — gate still open, mask must not shrink
        presenter.GetHint().Should().Be(firstHint);
    }

    // ── Weight updates ────────────────────────────────────────────────────────

    [Fact]
    public void CorrectAnswer_RegistersCorrectWeight()
    {
        var word = WordEntryFixture.Make("de hond", "dog", weight: 5);
        var quiz = MakeTypingQuiz("de hond", "dog", word);
        var presenter = new TypingQuizPresenter(quiz, _strategy, _wordListService, false);

        presenter.OnAnswerSelected("dog");

        word.WeightData.Weight.Should().BeLessThan(5);
    }

    [Fact]
    public void WrongAnswer_RegistersMistakeWeight()
    {
        var word = WordEntryFixture.Make("de hond", "dog", weight: 0);
        var quiz = MakeTypingQuiz("de hond", "dog", word);
        var presenter = new TypingQuizPresenter(quiz, _strategy, _wordListService, false);

        presenter.OnAnswerSelected("cat");

        word.WeightData.Weight.Should().BeGreaterThan(0);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private TypingQuizPresenter MakePresenter(string question, string correctAnswer, bool revealLetters = false)
    {
        var word = WordEntryFixture.Make(question, correctAnswer);
        var quiz = MakeTypingQuiz(question, correctAnswer, word);
        return new TypingQuizPresenter(quiz, _strategy, _wordListService, revealLetters);
    }

    private static QuizModel MakeTypingQuiz(string question, string correctAnswer, WordEntry? word = null)
    {
        word ??= WordEntryFixture.Make(question, correctAnswer);
        return new QuizModel(question, correctAnswer, [], word, new Dictionary<string, WordEntry>());
    }
}
