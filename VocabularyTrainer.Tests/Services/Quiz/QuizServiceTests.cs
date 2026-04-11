using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services.Quiz;
using VocabularyTrainer.Services.Quiz.Distractors;
using VocabularyTrainer.Services.Vocabulary;
using VocabularyTrainer.Tests.Fixtures;
using Xunit;

namespace VocabularyTrainer.Tests.Services.Quiz;

public class QuizServiceTests
{
    private static QuizService Build(IEnumerable<WordEntry> words, IDistractorSelector? selector = null) =>
        new(words.ToList(), new WordWeightStrategy(), selector ?? new EasyDistractorSelector());

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
    public void Quiz_QuestionAndAnswer_AreValid_ForRandomDirection()
    {
        var words = FiveDistinctWords();
        var hond = words.Single(w => w.Question == "hond");
        var service = Build(words);

        var results = Enumerable.Range(0, 100)
            .Select(_ => service.CreateQuizSessionForWord(hond, Config(dir: QuizDirection.Random), null!))
            .Select(s => (s.Quiz.Question, s.Quiz.CorrectAnswer))
            .ToList();

        // Every result must be one of the two valid direction combinations
        results.Should().AllSatisfy(r =>
            r.Should().BeOneOf(("hond", "dog"), ("dog", "hond")));

        // Both directions must appear at least once across 100 runs
        results.Should().Contain(("hond", "dog"));
        results.Should().Contain(("dog", "hond"));
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

    // ── Hard difficulty ───────────────────────────────────────────────────────

    [Fact]
    public void HardDifficulty_PrefersSimilarDutchDistractors()
    {
        // Distance is measured on the Dutch (Question) side.
        // "hond" correct; "bond","fond","pond","rond" are all Dutch distance 1.
        // "bibliotheek" (d≈8) and "vliegtuig" (d≈7) are always outside the
        // top-K=4 eligible pool, so their English answers must never appear.
        var correct     = WordEntryFixture.Make("hond",        "dog",      WordGroup.Other);
        var similar1    = WordEntryFixture.Make("bond",        "bond",     WordGroup.Other);
        var similar2    = WordEntryFixture.Make("fond",        "fond",     WordGroup.Other);
        var similar3    = WordEntryFixture.Make("pond",        "pound",    WordGroup.Other);
        var similar4    = WordEntryFixture.Make("rond",        "round",    WordGroup.Other);
        var dissimilar1 = WordEntryFixture.Make("bibliotheek", "library",  WordGroup.Other);
        var dissimilar2 = WordEntryFixture.Make("vliegtuig",   "airplane", WordGroup.Other);

        var words = new List<WordEntry> { correct, similar1, similar2, similar3, similar4, dissimilar1, dissimilar2 };
        var service = Build(words, new HardDistractorSelector());
        var config = new QuizConfiguration { OptionCount = 3, Direction = QuizDirection.Direct };

        // Run multiple times to account for randomness within the top-K pool
        for (int i = 0; i < 20; i++)
        {
            var session = service.CreateQuizSessionForWord(correct, config, null!);
            session.Quiz.Options.Should().NotContain("library");
            session.Quiz.Options.Should().NotContain("airplane");
        }
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
