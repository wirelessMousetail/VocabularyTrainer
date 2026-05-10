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
    public void IsAdditionalInfoVisible_IsTrue_AfterCorrectAnswer_WhenAnswerHasBrackets()
    {
        var word = WordEntryFixture.Make("stroom", "the current (water, air, electricity)");
        var vm = MakeViewModel(word);

        vm.TextInput = "the current";
        vm.SubmitCommand.Execute(null);

        vm.IsAdditionalInfoVisible.Should().BeTrue();
    }

    [Theory]
    [InlineData("stroom", "the current (water, air, electricity)", null)]          // before answering
    [InlineData("stroom", "the current (water, air, electricity)", "wrong answer")] // wrong answer
    [InlineData("hond",   "dog",                                   "dog")]          // correct, no brackets, single option
    [InlineData("getal",  "the number, the amount",                null)]           // before answering, multiple options
    [InlineData("getal",  "the number, the amount",                "wrong answer")] // wrong answer, multiple options
    public void IsAdditionalInfoVisible_IsFalse(string question, string answer, string? typed)
    {
        var word = WordEntryFixture.Make(question, answer);
        var vm = MakeViewModel(word);

        if (typed != null)
        {
            vm.TextInput = typed;
            vm.SubmitCommand.Execute(null);
        }

        vm.IsAdditionalInfoVisible.Should().BeFalse();
    }

    // ── IsAdditionalInfoVisible — multiple options ────────────────────────────

    [Fact]
    public void IsAdditionalInfoVisible_IsTrue_AfterCorrectAnswer_WhenAnswerHasMultipleOptions()
    {
        var word = WordEntryFixture.Make("getal", "the number, the amount");
        var vm = MakeViewModel(word);

        vm.TextInput = "the number";
        vm.SubmitCommand.Execute(null);

        vm.IsAdditionalInfoVisible.Should().BeTrue();
    }

    [Fact]
    public void AdditionalInfo_IsFullAnswer_WhenMultipleOptions()
    {
        var word = WordEntryFixture.Make("getal", "the number, the amount");
        var vm = MakeViewModel(word);

        vm.AdditionalInfo.Should().Be("the number, the amount");
    }

    // ── SwitchToOptionsCommand ────────────────────────────────────────────────

    [Fact]
    public void SwitchToOptionsCommand_CanExecute_IsTrue_Initially()
    {
        var word = WordEntryFixture.Make("hond", "dog");
        var vm = MakeViewModel(word);

        vm.SwitchToOptionsCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void SwitchToOptionsCommand_CanExecute_IsFalse_AfterCorrectAnswer()
    {
        var word = WordEntryFixture.Make("hond", "dog");
        var vm = MakeViewModel(word);

        vm.TextInput = "dog";
        vm.SubmitCommand.Execute(null);

        vm.SwitchToOptionsCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void SwitchToOptionsCommand_InvokesSwitchCallback()
    {
        var word = WordEntryFixture.Make("hond", "dog");
        bool callbackInvoked = false;
        var vm = MakeViewModel(word, onSwitchToOptions: () => callbackInvoked = true);

        vm.SwitchToOptionsCommand.Execute(null);

        callbackInvoked.Should().BeTrue();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private TypingQuizViewModel MakeViewModel(WordEntry word, Action? onSwitchToOptions = null)
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
        return new TypingQuizViewModel(session, () => { }, onSwitchToOptions ?? (() => { }));
    }
}
