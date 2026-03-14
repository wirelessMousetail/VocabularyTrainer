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

    // ── Option invariants (run many iterations due to randomness) ─────────────

    [Fact]
    public void CorrectAnswer_IsAlwaysAmongOptions()
    {
        var words = FiveDistinctWords();
        var service = Build(words);

        for (int i = 0; i < 500; i++)
        {
            var session = service.CreateQuizSession(Config(), null!)!;
            session.Quiz.Options.Should().Contain(session.Quiz.CorrectAnswer);
        }
    }

    [Fact]
    public void Options_NeverContainDuplicates()
    {
        var words = FiveDistinctWords();
        var service = Build(words);

        for (int i = 0; i < 500; i++)
        {
            var session = service.CreateQuizSession(Config(), null!)!;
            session.Quiz.Options
                .GroupBy(o => o.Trim(), StringComparer.OrdinalIgnoreCase)
                .Should().AllSatisfy(g => g.Count().Should().Be(1));
        }
    }

    [Fact]
    public void Options_CountMatchesConfiguration()
    {
        var words = FiveDistinctWords();
        var service = Build(words);

        for (int i = 0; i < 200; i++)
        {
            var session = service.CreateQuizSession(Config(options: 3), null!)!;
            session.Quiz.Options.Should().HaveCount(3);
        }
    }

    // ── IsSynonym – Direct mode ───────────────────────────────────────────────

    [Fact]
    public void Options_NeverShowSameAnswerTwice_DirectMode()
    {
        // "auto" and "wagen" both translate to "car" — only one should appear
        var words = new[]
        {
            WordEntryFixture.Make("auto",  "car",  WordGroup.Other),
            WordEntryFixture.Make("wagen", "car",  WordGroup.Other), // synonym
            WordEntryFixture.Make("vis",   "fish", WordGroup.Other),
            WordEntryFixture.Make("vogel", "bird", WordGroup.Other),
            WordEntryFixture.Make("boom",  "tree", WordGroup.Other),
        };
        var service = Build(words);

        for (int i = 0; i < 500; i++)
        {
            var session = service.CreateQuizSession(Config(), null!)!;
            var carCount = session.Quiz.Options
                .Count(o => string.Equals(o.Trim(), "car", StringComparison.OrdinalIgnoreCase));
            carCount.Should().BeLessOrEqualTo(1,
                because: "two words with the same answer must not both appear as options");
        }
    }

    // ── IsAlsoCorrect – Reverse mode ──────────────────────────────────────────

    public static IEnumerable<object[]> DecideSynonymCases()
    {
        var beslissen = WordEntryFixture.Make("beslissen", "to decide", WordGroup.Verb);
        var besluiten = WordEntryFixture.Make("besluiten", "to decide", WordGroup.Verb);
        WordEntry[] words =
        [
            beslissen,
            besluiten,
            WordEntryFixture.Make("lopen",  "to walk", WordGroup.Verb),
            WordEntryFixture.Make("rennen", "to run",  WordGroup.Verb),
            WordEntryFixture.Make("werken", "to work", WordGroup.Verb),
        ];
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
