using FluentAssertions;
using VocabularyTrainer.Services.Quiz;
using Xunit;

namespace VocabularyTrainer.Tests.Services.Quiz;

public class AnswerParserTests
{
    // ── Canonical ─────────────────────────────────────────────────────────────

    [Fact]
    public void Canonical_NoBrackets_ReturnsTrimmed()
    {
        AnswerParser.Canonical("  hello  ").Should().Be("hello");
    }

    [Fact]
    public void Canonical_WithBrackets_RemovesBracketsAndTrims()
    {
        AnswerParser.Canonical("the appointment, (but also:) the agreement")
            .Should().Be("the appointment, the agreement");
    }

    [Fact]
    public void Canonical_BracketsWithCommaInside_StripsWholeGroup()
    {
        AnswerParser.Canonical("to pass on (передать, например документы)")
            .Should().Be("to pass on");
    }

    [Fact]
    public void Canonical_MultipleBrackets()
    {
        AnswerParser.Canonical("foo (bar) baz (qux)")
            .Should().Be("foo baz");
    }

    // ── Options ───────────────────────────────────────────────────────────────

    [Fact]
    public void Options_SingleOption_ReturnsList()
    {
        AnswerParser.Options("agree").Should().Equal("agree");
    }

    [Fact]
    public void Options_MultipleOptions_SplitsOnComma()
    {
        AnswerParser.Options("the appointment, the agreement")
            .Should().Equal("the appointment", "the agreement");
    }

    [Fact]
    public void Options_BracketsStrippedBeforeSplit()
    {
        AnswerParser.Options("the appointment, (but also:) the agreement")
            .Should().Equal("the appointment", "the agreement");
    }

    [Fact]
    public void Options_BracketWithCommaInside_NotSplit()
    {
        AnswerParser.Options("to pass on (передать, например документы)")
            .Should().Equal("to pass on");
    }

    [Fact]
    public void Options_EmptyPartsDropped()
    {
        AnswerParser.Options("foo, , bar").Should().Equal("foo", "bar");
    }
}
