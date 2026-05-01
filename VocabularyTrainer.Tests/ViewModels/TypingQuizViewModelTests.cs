using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services.Quiz.Presenters;
using VocabularyTrainer.Services.Vocabulary;
using VocabularyTrainer.Tests.Fixtures;
using VocabularyTrainer.ViewModels;
using Xunit;
using QuizModel = VocabularyTrainer.Models.Quiz;

namespace VocabularyTrainer.Tests.ViewModels;

public class TypingQuizViewModelTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();
    private readonly WordListService _wordListService;
    private readonly WordWeightStrategy _strategy = new();

    public TypingQuizViewModelTests()
    {
        _wordListService = new WordListService(string.Empty, _tempFile);
    }

    public void Dispose() => File.Delete(_tempFile);

    // ── AnswerHintText ────────────────────────────────────────────────────────

    [Fact]
    public void AnswerHintText_IsFullUnstrippedAnswer()
    {
        var word = WordEntryFixture.Make("stroom", "the current (water, air, electricity)");
        var vm = MakeViewModel(word);

        vm.AnswerHintText.Should().Be("the current (water, air, electricity)");
    }

    // ── IsAnswerHintVisible ───────────────────────────────────────────────────

    [Fact]
    public void IsAnswerHintVisible_IsFalse_BeforeAnswering()
    {
        var word = WordEntryFixture.Make("stroom", "the current (water, air, electricity)");
        var vm = MakeViewModel(word);

        vm.IsAnswerHintVisible.Should().BeFalse();
    }

    [Fact]
    public void IsAnswerHintVisible_IsTrue_AfterCorrectAnswer_WhenAnswerHasBrackets()
    {
        var word = WordEntryFixture.Make("stroom", "the current (water, air, electricity)");
        var vm = MakeViewModel(word);

        vm.TextInput = "the current";
        vm.SubmitCommand.Execute(null);

        vm.IsAnswerHintVisible.Should().BeTrue();
    }

    [Fact]
    public void IsAnswerHintVisible_IsFalse_AfterCorrectAnswer_WhenAnswerHasNoBrackets()
    {
        var word = WordEntryFixture.Make("hond", "dog");
        var vm = MakeViewModel(word);

        vm.TextInput = "dog";
        vm.SubmitCommand.Execute(null);

        vm.IsAnswerHintVisible.Should().BeFalse();
    }

    [Fact]
    public void IsAnswerHintVisible_IsFalse_AfterWrongAnswer()
    {
        var word = WordEntryFixture.Make("stroom", "the current (water, air, electricity)");
        var vm = MakeViewModel(word);

        vm.TextInput = "wrong answer";
        vm.SubmitCommand.Execute(null);

        vm.IsAnswerHintVisible.Should().BeFalse();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private TypingQuizViewModel MakeViewModel(WordEntry word)
    {
        var quiz = new QuizModel(
            word.Question,
            word.CanonicalAnswer,
            [],
            word,
            new Dictionary<string, WordEntry>()
        );
        var presenter = new TypingQuizPresenter(quiz, _strategy, _wordListService, false);
        var config = new QuizConfiguration { AutoCloseAfterCorrectSeconds = 9999 };
        var session = new QuizSession(quiz, presenter, config);
        return new TypingQuizViewModel(session, () => { });
    }
}
