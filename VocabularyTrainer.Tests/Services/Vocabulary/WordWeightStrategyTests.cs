using FluentAssertions;
using VocabularyTrainer.Services.Vocabulary;
using VocabularyTrainer.Tests.Fixtures;
using Xunit;

namespace VocabularyTrainer.Tests.Services.Vocabulary;

public class WordWeightStrategyTests
{
    private readonly WordWeightStrategy _strategy = new();

    // ── RegisterMistake ───────────────────────────────────────────────────────

    [Fact]
    public void RegisterMistake_IncreasesWeight_AndResetsStreak()
    {
        var word = WordEntryFixture.Make("lopen", "to walk", weight: 10, streak: 3);
        _strategy.RegisterMistake(word);
        word.WeightData.Weight.Should().Be(31); // 10×3+1
        word.WeightData.CorrectStreak.Should().Be(0);
    }

    [Fact]
    public void RegisterMistake_AtZeroWeight_SetsWeightToOne()
    {
        var word = WordEntryFixture.Make("lopen", "to walk", weight: 0);
        _strategy.RegisterMistake(word);
        word.WeightData.Weight.Should().Be(1); // 0×3+1
    }

    [Fact]
    public void RegisterMistake_CapsWeightAt100()
    {
        var word = WordEntryFixture.Make("lopen", "to walk", weight: 50);
        _strategy.RegisterMistake(word);
        word.WeightData.Weight.Should().Be(100); // 50×3+1=151 → capped
    }

    // ── RegisterCorrect – linear branch (streak after increment ≤ 5) ─────────

    [Theory]
    [InlineData(0, 10, 9)]  // streak 0→1
    [InlineData(1, 10, 9)]  // streak 1→2
    [InlineData(4, 10, 9)]  // streak 4→5 (still linear)
    [InlineData(0,  0,  0)] // weight already 0 — stays at 0
    public void RegisterCorrect_LinearDecrease_ForStreakUpTo5(
        int initialStreak, int initialWeight, int expectedWeight)
    {
        var word = WordEntryFixture.Make("lopen", "to walk", weight: initialWeight, streak: initialStreak);
        _strategy.RegisterCorrect(word);
        word.WeightData.Weight.Should().Be(expectedWeight);
        word.WeightData.CorrectStreak.Should().Be(initialStreak + 1);
    }

    // ── RegisterCorrect – exponential branch (streak after increment > 5) ────

    [Theory]
    [InlineData(5,  20, 10)] // streak 5→6, 20×0.5
    [InlineData(5,  50, 25)] // streak 5→6, 50×0.5
    [InlineData(6, 100, 50)] // streak 6→7, already above threshold
    [InlineData(9,  40, 20)] // streak 9→10, high streak
    public void RegisterCorrect_ExponentialDecrease_WhenStreakExceedsThreshold(
        int initialStreak, int initialWeight, int expectedWeight)
    {
        var word = WordEntryFixture.Make("lopen", "to walk", weight: initialWeight, streak: initialStreak);
        _strategy.RegisterCorrect(word);
        word.WeightData.Weight.Should().Be(expectedWeight);
        word.WeightData.CorrectStreak.Should().Be(initialStreak + 1);
    }

    [Fact]
    public void RegisterCorrect_ExponentialDecrease_DoesNotGoBelowZero()
    {
        var word = WordEntryFixture.Make("lopen", "to walk", weight: 0, streak: 5);
        _strategy.RegisterCorrect(word);
        word.WeightData.Weight.Should().Be(0);
    }

    // ── CalculateTickets ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(0,   1)]
    [InlineData(5,   6)]
    [InlineData(100, 101)]
    public void CalculateTickets_ReturnsOneBasePlusWeight(int weight, int expected)
    {
        var word = WordEntryFixture.Make("lopen", "to walk", weight: weight);
        _strategy.CalculateTickets(word).Should().Be(expected);
    }
}
