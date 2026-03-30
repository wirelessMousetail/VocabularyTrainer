using FluentAssertions;
using VocabularyTrainer.Services;
using Xunit;

namespace VocabularyTrainer.Tests.Services;

public class StringDistanceTests
{
    [Fact]
    public void Levenshtein_IdenticalStrings_ReturnsZero() =>
        StringDistance.Levenshtein("dog", "dog").Should().Be(0);

    [Fact]
    public void Levenshtein_EmptyAndNonEmpty_ReturnsLength() =>
        StringDistance.Levenshtein("", "dog").Should().Be(3);

    [Fact]
    public void Levenshtein_NonEmptyAndEmpty_ReturnsLength() =>
        StringDistance.Levenshtein("dog", "").Should().Be(3);

    [Theory]
    [InlineData("dog", "log", 1)]
    [InlineData("dog", "cat", 3)]
    [InlineData("kitten", "sitting", 3)]
    public void Levenshtein_KnownPairs(string a, string b, int expected) =>
        StringDistance.Levenshtein(a, b).Should().Be(expected);

    // ── NormalizedLevenshtein ─────────────────────────────────────────────────

    [Fact]
    public void NormalizedLevenshtein_IdenticalStrings_ReturnsZero() =>
        StringDistance.NormalizedLevenshtein("dog", "dog").Should().Be(0.0);

    [Fact]
    public void NormalizedLevenshtein_BothEmpty_ReturnsZero() =>
        StringDistance.NormalizedLevenshtein("", "").Should().Be(0.0);

    [Fact]
    public void NormalizedLevenshtein_EmptyAndNonEmpty_ReturnsOne() =>
        StringDistance.NormalizedLevenshtein("", "dog").Should().Be(1.0);

    [Theory]
    [InlineData("hond", "bond", 0.25)]  // 1 substitution / 4
    [InlineData("dog",  "cat",  1.0)]   // 3 substitutions / 3
    public void NormalizedLevenshtein_KnownPairs(string a, string b, double expected) =>
        StringDistance.NormalizedLevenshtein(a, b).Should().BeApproximately(expected, 0.001);
}
