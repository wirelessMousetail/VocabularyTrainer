using FluentAssertions;
using VocabularyTrainer.Services;
using Xunit;

namespace VocabularyTrainer.Tests.Services;

public class SequenceAlignerTests
{
    private static string Render(bool[] mask, string correct)
        => new string(correct.Select((c, i) => mask[i] ? c : '_').ToArray());

    [Fact]
    public void ExactMatch_AllPositionsMatched()
    {
        var mask = SequenceAligner.FindMatches("dog", "dog");
        Render(mask, "dog").Should().Be("dog");
    }

    [Fact]
    public void NoOverlap_NoPositionsMatched()
    {
        var mask = SequenceAligner.FindMatches("xyz", "dog");
        Render(mask, "dog").Should().Be("___");
    }

    [Fact]
    public void Substitution_MatchedExceptSubstituted()
    {
        var mask = SequenceAligner.FindMatches("bekent", "bekend");
        Render(mask, "bekend").Should().Be("beken_");
    }

    [Fact]
    public void Deletion_SkipCorrectTieBreaking_PrefersEarlierAlignment()
    {
        var mask = SequenceAligner.FindMatches("bezeten", "bezetten");
        Render(mask, "bezetten").Should().Be("bezet_en");
    }

    [Fact]
    public void ResultLength_EqualsCorrectLength()
    {
        var mask = SequenceAligner.FindMatches("anything", "dog");
        mask.Length.Should().Be(3);
    }
}
