using FluentAssertions;
using VocabularyTrainer.Services;
using Xunit;

namespace VocabularyTrainer.Tests.Services;

public class LetterHintTrackerTests
{
    // ── GetHint returns null ──────────────────────────────────────────────────

    [Theory]
    [InlineData(null,   "bekend")]        // no attempt yet
    [InlineData("xy",   "bezetten")]      // gate closed — no matches at all
    [InlineData("psae", "de prestatie")]  // gate closed — isolated matches exist but no 3-char block
    public void GetHint_ReturnsNull(string? typed, string correct)
    {
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        if (typed != null) tracker.Update(typed, correct);
        tracker.GetHint(correct).Should().BeNull();
    }

    // ── Gate open — single attempt ────────────────────────────────────────────

    [Theory]
    [InlineData("bekent",   "bekend",   "beken_")]   // substitution at last char
    [InlineData("bezeten",  "bezetten", "bezet_en")] // deletion — gap between bezet and en
    [InlineData("bisetten", "bezetten", "b__etten")] // "etten"(5) opens gate; isolated "b" also revealed
    [InlineData("de hand",  "de hond",  "de h_nd")]  // spaces always visible; "de h"(4) opens gate
    public void GateOpen_SingleAttempt_ShowsMatchedChars(string typed, string correct, string expected)
    {
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.Update(typed, correct);
        tracker.GetHint(correct).Should().Be(expected);
    }

    // ── Mask accumulation ─────────────────────────────────────────────────────

    [Fact]
    public void MaskAccumulates_WeakerAttemptDoesNotShrinkReveal()
    {
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.Update("bekent", "bekend");
        tracker.GetHint("bekend").Should().Be("beken_");

        tracker.Update("bek___", "bekend"); // weaker — gate still open, mask must not shrink
        tracker.GetHint("bekend").Should().Be("beken_");
    }

    // ── Bonus reveal ──────────────────────────────────────────────────────────

    [Fact]
    public void BonusReveal_GateClosed_RevealsLeftmostNonSpaceChar()
    {
        // Gate stays closed on "xy" vs "bezetten"; bonus=true reveals first char
        var tracker = new LetterHintTracker(bonusRevealDecider: () => true);
        tracker.Update("xy", "bezetten");
        tracker.GetHint("bezetten").Should().Be("b_______");
    }

    [Fact]
    public void BonusReveal_SkipsAlreadyRevealedChars()
    {
        // Gate opens on "bekent" → "beken_"; then gate-closed attempt with bonus=true
        // reveals the next unrevealed non-space char ("d" at index 5)
        var tracker = new LetterHintTracker(bonusRevealDecider: () => true);
        tracker.Update("bekent", "bekend"); // gate opens: "beken_"
        tracker.Update("xy", "bekend");     // gate closed, bonus=true: reveals "d"
        tracker.GetHint("bekend").Should().Be("bekend");
    }
}
