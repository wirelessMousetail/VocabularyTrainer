using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services.Quiz.Distractors;
using VocabularyTrainer.Tests.Fixtures;
using Xunit;

namespace VocabularyTrainer.Tests.Services.Quiz.Distractors;

public class EasyDistractorSelectorTests
{
    private static readonly IDistractorSelector Selector = new EasyDistractorSelector();

    private static WordEntry Word(string q, string a) =>
        WordEntryFixture.Make(q, a, WordGroup.Other);

    [Fact]
    public void Select_ReturnsRequestedCount()
    {
        var candidates = new[] { Word("a", "1"), Word("b", "2"), Word("c", "3"), Word("d", "4") };
        var correct    = Word("x", "x");

        Selector.Select(candidates, correct, 2).Should().HaveCount(2);
    }

    [Fact]
    public void Select_ReturnsAllCandidates_WhenCountExceedsPool()
    {
        var candidates = new[] { Word("a", "1"), Word("b", "2") };
        var correct    = Word("x", "x");

        Selector.Select(candidates, correct, 5).Should().HaveCount(2);
    }

    [Fact]
    public void Select_ReturnsEmpty_WhenCandidatesIsEmpty()
    {
        Selector.Select([], Word("x", "x"), 3).Should().BeEmpty();
    }

    [Fact]
    public void Select_OnlyContainsCandidates()
    {
        var candidates = new[] { Word("a", "1"), Word("b", "2"), Word("c", "3") };
        var correct    = Word("x", "x");

        var result = Selector.Select(candidates, correct, 2);
        result.Should().OnlyContain(w => candidates.Contains(w));
    }

    [Fact]
    public void Select_IsRandom_AcrossMultipleRuns()
    {
        var candidates = new[] { Word("a", "1"), Word("b", "2"), Word("c", "3"), Word("d", "4") };
        var correct    = Word("x", "x");

        // With 4 candidates choosing 1, the same entry would appear 100% of the time
        // with probability (1/4)^50 ≈ 10^-30 — effectively impossible.
        var results = Enumerable.Range(0, 50)
            .Select(_ => Selector.Select(candidates, correct, 1).Single())
            .Select(w => w.Question)
            .ToHashSet();

        results.Should().HaveCountGreaterThan(1, because: "selection must be random");
    }
}
