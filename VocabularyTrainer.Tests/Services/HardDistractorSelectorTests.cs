using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;
using VocabularyTrainer.Tests.Fixtures;
using Xunit;

namespace VocabularyTrainer.Tests.Services;

public class HardDistractorSelectorTests
{
    private static readonly IDistractorSelector Selector = new HardDistractorSelector();

    private static WordEntry Word(string q, string a) =>
        WordEntryFixture.Make(q, a, WordGroup.Other);

    [Fact]
    public void Select_ReturnsRequestedCount_WhenPoolIsLargeEnough()
    {
        WordEntry correct = Word("hond", "dog");
        var candidates = new[]
        {
            Word("bond", "bond"), Word("fond", "fond"),
            Word("pond", "pound"), Word("rond", "round"),
        };

        Selector.Select(candidates, correct, 2).Should().HaveCount(2);
    }

    [Fact]
    public void Select_ReturnsAllCandidates_WhenCountExceedsPool()
    {
        WordEntry correct    = Word("hond", "dog");
        var candidates = new[] { Word("bond", "bond"), Word("fond", "fond") };

        Selector.Select(candidates, correct, 5).Should().HaveCount(2);
    }

    [Fact]
    public void Select_ReturnsEmpty_WhenCandidatesIsEmpty()
    {
        Selector.Select([], Word("hond", "dog"), 2).Should().BeEmpty();
    }

    [Fact]
    public void Select_NeverPicksDissimilarWords_WhenSimilarPoolIsLargeEnough()
    {
        // Jaro-Winkler on Dutch (Question) field:
        //   bond/fond/pond/rond → JW ≈ 0.833 from "hond" (above 0.7 threshold)
        //   bibliotheek/vliegtuig → JW < 0.6 (below threshold, outside top-K)
        // K = 2 × 2 = 4 similar candidates — dissimilar ones fall outside K.
        WordEntry correct  = Word("hond",        "dog");
        var similar = new[]
        {
            Word("bond",        "bond"),
            Word("fond",        "fond"),
            Word("pond",        "pound"),
            Word("rond",        "round"),
        };
        var dissimilar = new[]
        {
            Word("bibliotheek", "library"),
            Word("vliegtuig",   "airplane"),
        };
        var candidates = similar.Concat(dissimilar).ToArray();

        for (int i = 0; i < 20; i++)
        {
            var result = Selector.Select(candidates, correct, 2);
            result.Should().NotContain(w => w.Answer == "library");
            result.Should().NotContain(w => w.Answer == "airplane");
        }
    }

    [Fact]
    public void Select_FallsBackToRandom_WhenNoSimilarWordExists()
    {
        // "enzovoort" has no visually similar neighbours in this pool —
        // all Jaro-Winkler scores fall below the 0.7 threshold.
        WordEntry correct = Word("enzovoort", "etcetera");
        var candidates = new[]
        {
            Word("ongeveer",  "approximately"),  // JW < 0.7
            Word("bijzonder", "special"),        // JW < 0.7
            Word("misschien", "maybe"),          // JW < 0.7
        };

        // Run many times; if the selector always returned the same word the result
        // set would have size 1, which can't happen with true random selection.
        var results = Enumerable.Range(0, 50)
            .Select(_ => Selector.Select(candidates, correct, 1).Single().Answer)
            .ToHashSet();

        results.Should().HaveCountGreaterThan(1, because: "fallback must be random, not always the same word");
    }

    [Fact]
    public void Select_PrefersCloserWords_OverFarWords()
    {
        // Two candidates: bond (JW ≈ 0.833, above threshold) and bibliotheek (JW < 0.6).
        // Best similarity 0.833 > 0.7 → no fallback. K = 2 × 1 = 2 → both in pool.
        // Pick count=1 randomly; "bond" must appear at least once across many runs.
        WordEntry correct = Word("hond", "dog");
        var candidates = new[]
        {
            Word("bond",        "bond"),     // JW ≈ 0.833
            Word("bibliotheek", "library"),  // JW < 0.6
        };

        var results = Enumerable.Range(0, 30)
            .Select(_ => Selector.Select(candidates, correct, 1).Single().Answer)
            .ToHashSet();

        // "bond" must appear at least once — it is always in top-K
        results.Should().Contain("bond");
    }
}
