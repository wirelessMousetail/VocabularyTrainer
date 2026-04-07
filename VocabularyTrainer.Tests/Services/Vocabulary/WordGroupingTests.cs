using FluentAssertions;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services.Vocabulary;
using Xunit;

namespace VocabularyTrainer.Tests.Services.Vocabulary;

public class WordGroupingTests
{
    [Theory]
    [InlineData("de hond")]
    [InlineData("  de hond ")]
    [InlineData("de kat")]
    [InlineData("het huis")]
    [InlineData("De Hond")]   // case-insensitive
    [InlineData("Het Huis")]  // case-insensitive
    public void Detect_ReturnsNoun_ForArticlePrefix(string word) =>
        WordGrouping.Detect(word).Should().Be(WordGroup.Noun);

    [Theory]
    [InlineData("lopen")]
    [InlineData("werken")]
    [InlineData("beslissen")]
    [InlineData("LOPEN")]       // case-insensitive
    [InlineData("  lopen  ")]   // leading/trailing whitespace
    [InlineData("deken")]       // starts with "de" but no space — not a noun, classified as verb
    [InlineData("heten")]       // starts with "het" but no space — not a noun, classified as verb
    public void Detect_ReturnsVerb_ForEnSuffix(string word) =>
        WordGrouping.Detect(word).Should().Be(WordGroup.Verb);

    [Theory]
    [InlineData("goed")]
    [InlineData("snel")]
    [InlineData("")]
    [InlineData("   ")]
    public void Detect_ReturnsOther_ForUnclassified(string word) =>
        WordGrouping.Detect(word).Should().Be(WordGroup.Other);

    [Theory]
    [InlineData("de spoken")]  // "de" article wins over "-en" suffix
    [InlineData("het heten")]  // "het" article wins over "-en" suffix
    public void Detect_ArticlePrefixTakesPrecedenceOverVerbSuffix(string word) =>
        WordGrouping.Detect(word).Should().Be(WordGroup.Noun);
}
