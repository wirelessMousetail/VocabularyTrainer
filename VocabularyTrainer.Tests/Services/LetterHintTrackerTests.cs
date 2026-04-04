using FluentAssertions;
using VocabularyTrainer.Services;
using Xunit;

namespace VocabularyTrainer.Tests.Services;

public class LetterHintTrackerTests
{
    [Fact]
    public void BeforeUpdate_GetHint_ReturnsNull()
    {
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.GetHint("bekend").Should().BeNull();
    }

    [Fact]
    public void GateClosed_AfterUpdate_GetHint_ReturnsNull()
    {
        // "xy" vs "bezetten" — no contiguous block of 3 matches
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.Update("xy", "bezetten");
        tracker.GetHint("bezetten").Should().BeNull();
    }

    [Fact]
    public void GateOpen_Substitution_ShowsMatchedChars()
    {
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.Update("bekent", "bekend");
        tracker.GetHint("bekend").Should().Be("beken_");
    }

    [Fact]
    public void GateOpen_Deletion_ShowsMatchedCharsAroundGap()
    {
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.Update("bezeten", "bezetten");
        tracker.GetHint("bezetten").Should().Be("bezet_en");
    }

    [Fact]
    public void GateOpen_IsolatedMatchesAlsoRevealed()
    {
        // "bisetten" vs "bezetten" — gate opens on "etten"(5), isolated "b" also shown
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.Update("bisetten", "bezetten");
        tracker.GetHint("bezetten").Should().Be("b__etten");
    }

    [Fact]
    public void SpacesAlwaysVisible()
    {
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.Update("de hand", "de hond");
        tracker.GetHint("de hond").Should().Be("de h_nd");
    }

    [Fact]
    public void MaskAccumulates_WeakerAttemptDoesNotShrinkReveal()
    {
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.Update("bekent", "bekend");
        tracker.GetHint("bekend").Should().Be("beken_");

        tracker.Update("bek___", "bekend");
        tracker.GetHint("bekend").Should().Be("beken_");
    }

    [Fact]
    public void BonusReveal_GateClosed_RevealsLeftmostNonSpaceChar()
    {
        // "xy" vs "bezetten" — gate stays closed, but bonus=true reveals first char
        var tracker = new LetterHintTracker(bonusRevealDecider: () => true);
        tracker.Update("xy", "bezetten");
        tracker.GetHint("bezetten").Should().Be("b_______");
    }

    [Fact]
    public void BonusReveal_SkipsAlreadyRevealedChars()
    {
        // After gate-open reveals "beken_", a gate-closed attempt with bonus=true
        // should reveal the next unrevealed non-space char (the 'd')
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.Update("bekent", "bekend");
        tracker.GetHint("bekend").Should().Be("beken_");

        // Now switch to bonus=true and do a gate-closed attempt
        var tracker2 = new LetterHintTracker(bonusRevealDecider: () => true);
        tracker2.Update("bekent", "bekend"); // gate opens — reveals "beken_"
        // Force a gate-closed attempt: typed "xy" vs "bekend" — no 3-char block
        // but bonus=true, leftmost unrevealed non-space is 'd' (index 5)
        tracker2.Update("xy", "bekend");
        tracker2.GetHint("bekend").Should().Be("bekend");
    }

    [Fact]
    public void BonusReveal_WhenDeciderFalse_GetHintStillNull()
    {
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.Update("xy", "bezetten");
        tracker.GetHint("bezetten").Should().BeNull();
    }
}
