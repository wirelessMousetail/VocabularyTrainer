using FluentAssertions;
using VocabularyTrainer.Services.Quiz.Presenters;
using Xunit;

namespace VocabularyTrainer.Tests.Services.Quiz.Presenters;

public class AnswerParserTests
{
    // ── Canonical ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("  hello  ", "hello")]
    [InlineData("the appointment, (but also:) the agreement", "the appointment, the agreement")]
    [InlineData("to pass on (something, like docs)", "to pass on")]
    [InlineData("foo (bar) baz (qux)", "foo baz")]
    public void Canonical(string input, string expected) =>
        AnswerParser.Canonical(input).Should().Be(expected);

    // ── Options ───────────────────────────────────────────────────────────────

    public static TheoryData<string, string[]> OptionsData => new()
    {
        { "agree",                                              new[] { "agree" } },
        { "the appointment, the agreement",                     new[] { "the appointment", "the agreement" } },
        { "the appointment, (but also:) the agreement",         new[] { "the appointment", "the agreement" } },
        { "to pass on (like, for example, documents)",          new[] { "to pass on" } },
        { "foo, , bar",                                         new[] { "foo", "bar" } },
    };

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Options(string input, string[] expected) =>
        AnswerParser.Options(input).Should().Equal(expected);
}
