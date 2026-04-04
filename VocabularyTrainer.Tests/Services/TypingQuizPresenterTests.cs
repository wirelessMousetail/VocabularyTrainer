using System.Collections.Generic;
using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;
using VocabularyTrainer.Tests.Fixtures;
using Xunit;

namespace VocabularyTrainer.Tests.Services;

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

    // ── Correct answer matching ───────────────────────────────────────────────

    [Fact]
    public void ExactMatch_IsCorrect()
    {
        var presenter = MakePresenter("de hond", "dog");
        presenter.OnAnswerSelected("dog");
        presenter.GetResult().Should().Be(QuizResult.Correct);
    }

    [Fact]
    public void CaseInsensitive_IsCorrect()
    {
        var presenter = MakePresenter("de hond", "dog");
        presenter.OnAnswerSelected("DOG");
        presenter.GetResult().Should().Be(QuizResult.Correct);
    }

    [Fact]
    public void Whitespace_IsIgnored()
    {
        var presenter = MakePresenter("de hond", "dog");
        presenter.OnAnswerSelected("  dog  ");
        presenter.GetResult().Should().Be(QuizResult.Correct);
    }

    [Fact]
    public void LeadingThe_IsIgnored()
    {
        var presenter = MakePresenter("de hond", "dog");
        presenter.OnAnswerSelected("the dog");
        presenter.GetResult().Should().Be(QuizResult.Correct);
    }

    [Fact]
    public void LeadingA_IsIgnored()
    {
        var presenter = MakePresenter("de hond", "dog");
        presenter.OnAnswerSelected("a dog");
        presenter.GetResult().Should().Be(QuizResult.Correct);
    }

    [Fact]
    public void LeadingAn_IsIgnored()
    {
        var presenter = MakePresenter("de appel", "apple");
        presenter.OnAnswerSelected("an apple");
        presenter.GetResult().Should().Be(QuizResult.Correct);
    }

    // ── Wrong answer ──────────────────────────────────────────────────────────

    [Fact]
    public void WrongAnswer_SetsResultToWrong()
    {
        var presenter = MakePresenter("de hond", "dog");
        presenter.OnAnswerSelected("cat");
        presenter.GetResult().Should().Be(QuizResult.Wrong);
    }

    [Fact]
    public void WrongDutchArticle_SetsResultToWrongArticle()
    {
        // correct is "de hond", typing "het hond" — same noun, wrong article
        var presenter = MakePresenter("hond", "de hond");
        presenter.OnAnswerSelected("het hond");
        presenter.GetResult().Should().Be(QuizResult.WrongArticle);
    }

    [Fact]
    public void CorrectDutchArticle_IsCorrect()
    {
        var presenter = MakePresenter("hond", "de hond");
        presenter.OnAnswerSelected("de hond");
        presenter.GetResult().Should().Be(QuizResult.Correct);
    }

    [Fact]
    public void NoArticleTyped_WhenCorrectHasArticle_IsWrong()
    {
        // typing just "hond" when correct is "de hond" — not WrongArticle, just Wrong
        var presenter = MakePresenter("hond", "de hond");
        presenter.OnAnswerSelected("hond");
        presenter.GetResult().Should().Be(QuizResult.Wrong);
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

    // ── GetHint (reveal letters) ──────────────────────────────────────────────

    [Fact]
    public void NoReveal_GetHint_ReturnsNull()
    {
        var presenter = MakePresenter("de hond", "dog", revealLetters: false);
        presenter.OnAnswerSelected("cat");
        presenter.GetHint().Should().BeNull();
    }

    [Fact]
    public void RevealLetters_BeforeFirstAttempt_ReturnsNull()
    {
        var presenter = MakePresenter("de hond", "dog", revealLetters: true);
        presenter.GetHint().Should().BeNull();
    }

    [Fact]
    public void RevealLetters_SubstitutionOnly_ShowsMatchedChars()
    {
        // "bekent" vs "bekend": 5 matching chars then substitution → "beken_"
        var presenter = MakePresenter("nice", "bekend", revealLetters: true);
        presenter.OnAnswerSelected("bekent");
        presenter.GetHint().Should().Be("beken_");
    }

    [Fact]
    public void RevealLetters_Deletion_ShowsMatchedCharsAroundGap()
    {
        // "bezeten" vs "bezetten": user dropped one 't' → gap between bezet and en
        var presenter = MakePresenter("nice", "bezetten", revealLetters: true);
        presenter.OnAnswerSelected("bezeten");
        presenter.GetHint().Should().Be("bezet_en");
    }

    [Fact]
    public void RevealLetters_GateOpens_IsolatedMatchesAlsoRevealed()
    {
        // "bisetten" vs "bezetten": block "etten"(5) opens gate → isolated "b" also shown
        var presenter = MakePresenter("nice", "bezetten", revealLetters: true);
        presenter.OnAnswerSelected("bisetten");
        presenter.GetHint().Should().Be("b__etten");
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

    [Fact]
    public void RevealLetters_SpacesAlwaysVisible()
    {
        // "de hand" vs "de hond": substitution a→o, space always shown
        // blocks: "de h"(4) ≥ 3 opens gate; "nd"(2) also revealed via gate
        var presenter = MakePresenter("hond", "de hond", revealLetters: true);
        presenter.OnAnswerSelected("de hand");
        presenter.GetHint().Should().Be("de h_nd");
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

    private static Quiz MakeTypingQuiz(string question, string correctAnswer, WordEntry? word = null)
    {
        word ??= WordEntryFixture.Make(question, correctAnswer);
        return new Quiz(question, correctAnswer, [], word, new Dictionary<string, WordEntry>());
    }
}
