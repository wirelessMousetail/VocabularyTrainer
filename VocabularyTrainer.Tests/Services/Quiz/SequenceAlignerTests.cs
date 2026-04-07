using FluentAssertions;
using VocabularyTrainer.Services.Quiz;
using Xunit;

namespace VocabularyTrainer.Tests.Services.Quiz;

public class SequenceAlignerTests
{
    private static string Render(bool[] mask, string correct)
        => new string(correct.Select((c, i) => mask[i] ? c : '_').ToArray());

    [Theory]
    [InlineData("dog",     "dog",      "dog")]       // exact match
    [InlineData("xyz",     "dog",      "___")]       // no overlap
    [InlineData("bekent",  "bekend",   "beken_")]    // substitution at last char
    [InlineData("bezeten", "bezetten", "bezet_en")]  // deletion — skip-correct tie-breaking prefers earlier alignment
    public void FindMatches_Render(string typed, string correct, string expected)
    {
        var mask = SequenceAligner.FindMatches(typed, correct);
        Render(mask, correct).Should().Be(expected);
    }

    [Fact]
    public void ResultLength_EqualsCorrectLength()
    {
        var mask = SequenceAligner.FindMatches("anything", "dog");
        mask.Length.Should().Be("dog".Length);
    }
}
