using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;
using Xunit;

namespace VocabularyTrainer.Tests.Services;

public class WordGroupingTests
{
    [Theory]
    [InlineData("de hond")]
    [InlineData("de kat")]
    [InlineData("het huis")]
    [InlineData("De Hond")]
    [InlineData("Het Huis")]
    public void Detect_ReturnsNoun_ForArticlePrefix(string word) =>
        WordGrouping.Detect(word).Should().Be(WordGroup.Noun);

    [Theory]
    [InlineData("lopen")]
    [InlineData("werken")]
    [InlineData("beslissen")]
    [InlineData("LOPEN")]
    public void Detect_ReturnsVerb_ForEnSuffix(string word) =>
        WordGrouping.Detect(word).Should().Be(WordGroup.Verb);

    [Theory]
    [InlineData("goed")]
    [InlineData("snel")]
    [InlineData("")]
    [InlineData("   ")]
    public void Detect_ReturnsOther_ForUnclassified(string word) =>
        WordGrouping.Detect(word).Should().Be(WordGroup.Other);

    [Fact]
    public void Detect_ArticlePrefixTakesPrecedenceOverVerbSuffix() =>
        WordGrouping.Detect("de spoken").Should().Be(WordGroup.Noun);
}
