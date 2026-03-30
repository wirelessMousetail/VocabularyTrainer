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
}
