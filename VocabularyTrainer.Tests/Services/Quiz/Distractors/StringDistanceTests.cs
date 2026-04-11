using FluentAssertions;
using VocabularyTrainer.Services.Quiz.Distractors;
using Xunit;

namespace VocabularyTrainer.Tests.Services.Quiz.Distractors;

public class StringDistanceTests
{
    [Theory]
    [InlineData("dog", "dog", 0)]
    [InlineData("",    "dog", 3)]
    [InlineData("dog", "",   3)]
    [InlineData("dog", "log", 1)]
    [InlineData("dog", "cat", 3)]
    [InlineData("kitten", "sitting", 3)]
    public void Levenshtein_KnownPairs(string a, string b, int expected) =>
        StringDistance.Levenshtein(a, b).Should().Be(expected);

    // ── NormalizedLevenshtein ─────────────────────────────────────────────────

    [Theory]
    [InlineData("dog", "dog", 0.0)]
    [InlineData("",    "",   0.0)]
    [InlineData("",    "dog", 1.0)]
    [InlineData("hond", "bond", 0.25)]  // 1 substitution / 4
    [InlineData("dog",  "cat",  1.0)]   // 3 substitutions / 3
    public void NormalizedLevenshtein_KnownPairs(string a, string b, double expected) =>
        StringDistance.NormalizedLevenshtein(a, b).Should().BeApproximately(expected, 0.001);

    // ── JaroWinkler ───────────────────────────────────────────────────────────

    [Fact]
    public void JaroWinkler_CompletelyDifferent_ReturnsLowScore() =>
        StringDistance.JaroWinkler("hond", "bibliotheek").Should().BeLessThan(0.6);

    [Fact]
    public void JaroWinkler_PrefixBoost_ScoresHigherThanSuffixMatch()
    {
        // "houd" shares prefix "ho" with "hond" → higher than "bond" (only suffix "ond")
        double withPrefix    = StringDistance.JaroWinkler("hond", "houd");
        double withoutPrefix = StringDistance.JaroWinkler("hond", "bond");
        withPrefix.Should().BeGreaterThan(withoutPrefix);
    }

    [Theory]
    [InlineData("hond", "hond", 1.0)]
    [InlineData("",     "hond", 0.0)]
    [InlineData("hond", "bond", 0.833)]  // 3/4 chars match, no common prefix
    [InlineData("hond", "houd", 0.867)]  // 3/4 chars match + "ho" prefix bonus
    public void JaroWinkler_KnownPairs(string a, string b, double expected) =>
        StringDistance.JaroWinkler(a, b).Should().BeApproximately(expected, 0.001);
}
