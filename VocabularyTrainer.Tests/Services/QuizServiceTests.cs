using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;
using VocabularyTrainer.Tests.Fixtures;
using Xunit;

namespace VocabularyTrainer.Tests.Services;

public class QuizServiceTests
{
    private static QuizService Build(IEnumerable<WordEntry> words) =>
        new(words.ToList(), new WordWeightStrategy());

    private static QuizConfiguration Config(int options = 3, QuizDirection dir = QuizDirection.Direct) =>
        new() { OptionCount = options, Direction = dir };

    // ── Null guard ────────────────────────────────────────────────────────────

    [Fact]
    public void CreateQuizSession_ReturnsNull_WhenWordListIsEmpty()
    {
        var service = Build([]);
        service.CreateQuizSession(Config(), null!).Should().BeNull();
    }

    // ── Option invariants ─────────────────────────────────────────────────────

    public static IEnumerable<object[]> FiveDistinctWordCases()
    {
        var words = FiveDistinctWords();
        return words.Select(w => new object[] { w, words });
    }

    [Theory]
    [MemberData(nameof(FiveDistinctWordCases))]
    public void CorrectAnswer_AppearsExactlyOnceInOptions(WordEntry word, WordEntry[] words)
    {
        var session = Build(words).CreateQuizSessionForWord(word, Config(), null!);
        session.Quiz.Options
            .Count(o => string.Equals(o.Trim(), session.Quiz.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
            .Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(FiveDistinctWordCases))]
    public void Options_NeverContainDuplicates(WordEntry word, WordEntry[] words)
    {
        var session = Build(words).CreateQuizSessionForWord(word, Config(), null!);
        session.Quiz.Options
            .GroupBy(o => o.Trim(), StringComparer.OrdinalIgnoreCase)
            .Should().AllSatisfy(g => g.Count().Should().Be(1));
    }

    [Theory]
    [MemberData(nameof(FiveDistinctWordCases))]
    public void Options_CountMatchesConfiguration(WordEntry word, WordEntry[] words)
    {
        var session = Build(words).CreateQuizSessionForWord(word, Config(options: 3), null!);
        session.Quiz.Options.Should().HaveCount(3);
    }

    // ── Direction mapping ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(QuizDirection.Direct,  "hond", "dog")]
    [InlineData(QuizDirection.Reverse, "dog",  "hond")]
    public void Quiz_QuestionAndAnswer_MatchDirection(
        QuizDirection direction, string expectedQuestion, string expectedAnswer)
    {
        var words = FiveDistinctWords();
        var hond = words.Single(w => w.Question == "hond");
        var session = Build(words).CreateQuizSessionForWord(hond, Config(dir: direction), null!);
        session.Quiz.Question.Should().Be(expectedQuestion);
        session.Quiz.CorrectAnswer.Should().Be(expectedAnswer);
    }

    [Fact]
    public void Quiz_QuestionAndAnswer_AreValid_ForRandomDirection() //todo run 100 times to make sure direction changes
    {
        var words = FiveDistinctWords();
        var hond = words.Single(w => w.Question == "hond");
        var session = Build(words).CreateQuizSessionForWord(hond, Config(dir: QuizDirection.Random), null!);

        // Either direct ("hond"→"dog") or reverse ("dog"→"hond") is valid
        (session.Quiz.Question, session.Quiz.CorrectAnswer).Should().BeOneOf(
            ("hond", "dog"),
            ("dog",  "hond"));
    }

    // ── IsSynonym – Direct mode ───────────────────────────────────────────────

    public static IEnumerable<object[]> CarSynonymCases()
    {
        var auto  = WordEntryFixture.Make("auto",  "car",  WordGroup.Other);
        var wagen = WordEntryFixture.Make("wagen", "car",  WordGroup.Other);
        // Only one non-synonym distractor so the synonym is always forced into
        // the candidate pool when not filtered (pool size == optionCount - 1).
        WordEntry[] words = [auto, wagen, WordEntryFixture.Make("vis", "fish", WordGroup.Other)];
        yield return [auto,  words];
        yield return [wagen, words];
    }

    [Theory]
    [MemberData(nameof(CarSynonymCases))]
    public void Options_NeverShowSameAnswerTwice_DirectMode(WordEntry word, WordEntry[] words)
    {
        // "auto" and "wagen" both translate to "car" — only one should appear
        var session = Build(words).CreateQuizSessionForWord(word, Config(), null!);
        session.Quiz.Options
            .Count(o => string.Equals(o.Trim(), "car", StringComparison.OrdinalIgnoreCase))
            .Should().Be(1, because: "the correct answer must appear exactly once; the synonym must be excluded");
    }

    // ── IsAlsoCorrect – Reverse mode ──────────────────────────────────────────

    public static IEnumerable<object[]> DecideSynonymCases()
    {
        var beslissen = WordEntryFixture.Make("beslissen", "to decide", WordGroup.Verb);
        var besluiten = WordEntryFixture.Make("besluiten", "to decide", WordGroup.Verb);
        // Only one non-synonym distractor so the ambiguous word is always forced
        // into the candidate pool when not filtered (pool size == optionCount - 1).
        WordEntry[] words = [beslissen, besluiten, WordEntryFixture.Make("lopen", "to walk", WordGroup.Verb)];
        yield return [beslissen, words];
        yield return [besluiten, words];
    }

    [Theory]
    [MemberData(nameof(DecideSynonymCases))]
    public void Options_NeverIncludeAmbiguousAnswer_ReverseMode(WordEntry word, WordEntry[] words)
    {
        // "beslissen" and "besluiten" both mean "to decide"
        // In reverse mode the question is "to decide"; showing either Dutch word as a
        // wrong option is misleading because both are correct answers.
        var session = Build(words).CreateQuizSessionForWord(word, Config(dir: QuizDirection.Reverse), null!);

        // Neither Dutch word for "to decide" must appear as a wrong option
        // (the correct one will be in Options, but not the other)
        var decideCount = session.Quiz.Options.Count(o =>
            string.Equals(o.Trim(), "beslissen", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(o.Trim(), "besluiten", StringComparison.OrdinalIgnoreCase));
        decideCount.Should().Be(1,
            because: "the selected correct answer must appear exactly once; the other synonym must be excluded");
    }

    // ── Constrained pool ──────────────────────────────────────────────────────

    [Fact]
    public void Options_FewerThanRequested_WhenPoolIsConstrained()
    {
        var auto = WordEntryFixture.Make("auto", "car", WordGroup.Other);
        WordEntry[] words =
        [
            auto,
            WordEntryFixture.Make("wagen", "car",  WordGroup.Other), // synonym, excluded from pool
            WordEntryFixture.Make("vis",   "fish", WordGroup.Other), // only available distractor
        ];
        // OptionCount = 3 needs 2 distractors, but only 1 is available after synonym exclusion
        var session = Build(words).CreateQuizSessionForWord(auto, Config(options: 3), null!);
        session.Quiz.Options.Should().HaveCount(2);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static WordEntry[] FiveDistinctWords() =>
    [
        WordEntryFixture.Make("hond",  "dog",   WordGroup.Other),
        WordEntryFixture.Make("kat",   "cat",   WordGroup.Other),
        WordEntryFixture.Make("vis",   "fish",  WordGroup.Other),
        WordEntryFixture.Make("vogel", "bird",  WordGroup.Other),
        WordEntryFixture.Make("boom",  "tree",  WordGroup.Other),
    ];
}
