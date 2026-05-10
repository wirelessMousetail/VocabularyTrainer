using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services.Quiz.Presenters;
using VocabularyTrainer.Services.Vocabulary;
using VocabularyTrainer.Tests.Fixtures;
using VocabularyTrainer.ViewModels;
using Xunit;
using QuizModel = VocabularyTrainer.Models.Quiz;

namespace VocabularyTrainer.Tests.ViewModels;

public class QuizViewModelTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();
    private readonly WordListService _wordListService;
    private readonly WordWeightStrategy _strategy = new();

    public QuizViewModelTests()
    {
        _wordListService = new WordListService(string.Empty, _tempFile);
    }

    public void Dispose() => File.Delete(_tempFile);

    // ── AdditionalInfo ────────────────────────────────────────────────────────

    [Fact]
    public void AdditionalInfo_IsFullUnstrippedAnswer()
    {
        var word = WordEntryFixture.Make("stroom", "the current (water, air, electricity)");
        var vm = MakeViewModel(word);

        vm.AdditionalInfo.Should().Be("the current (water, air, electricity)");
    }

    // ── IsAdditionalInfoVisible ───────────────────────────────────────────────

    [Fact]
    public void IsAdditionalInfoVisible_IsFalse_BeforeAnswering()
    {
        var word = WordEntryFixture.Make("stroom", "the current (water, air, electricity)");
        var vm = MakeViewModel(word);

        vm.IsAdditionalInfoVisible.Should().BeFalse();
    }

    [Fact]
    public void IsAdditionalInfoVisible_IsTrue_AfterCorrectAnswer_WhenAnswerHasBrackets()
    {
        var word = WordEntryFixture.Make("stroom", "the current (water, air, electricity)");
        var vm = MakeViewModel(word);

        vm.AnswerCommand.Execute("the current");

        vm.IsAdditionalInfoVisible.Should().BeTrue();
    }

    [Fact]
    public void IsAdditionalInfoVisible_IsFalse_AfterCorrectAnswer_WhenAnswerHasNoBrackets()
    {
        var word = WordEntryFixture.Make("hond", "dog");
        var vm = MakeViewModel(word);

        vm.AnswerCommand.Execute("dog");

        vm.IsAdditionalInfoVisible.Should().BeFalse();
    }

    [Fact]
    public void IsAdditionalInfoVisible_IsFalse_AfterWrongAnswer()
    {
        var word = WordEntryFixture.Make("stroom", "the current (water, air, electricity)");
        var vm = MakeViewModel(word);

        vm.AnswerCommand.Execute("wrong answer");

        vm.IsAdditionalInfoVisible.Should().BeFalse();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private QuizViewModel MakeViewModel(WordEntry word)
    {
        var quiz = new QuizModel(
            word.Question,
            word.CanonicalAnswer,
            [word.CanonicalAnswer, "distractor"],
            word,
            new Dictionary<string, WordEntry>()
        );
        var presenter = new QuizPresenter(quiz, _strategy, _wordListService);
        var config = new QuizConfiguration { AutoCloseAfterCorrectSeconds = 9999 };
        var session = new QuizSession(quiz, presenter, config);
        return new QuizViewModel(session, () => { });
    }
}
