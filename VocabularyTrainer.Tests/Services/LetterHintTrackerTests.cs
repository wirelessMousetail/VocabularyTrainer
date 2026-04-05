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
        if (typed != null) tracker.Update(typed, new[] { correct });
        tracker.GetHint(new[] { correct }).Should().BeNull();
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
        tracker.Update(typed, new[] { correct });
        tracker.GetHint(new[] { correct }).Should().Be(expected);
    }

    // ── Mask accumulation ─────────────────────────────────────────────────────

    [Fact]
    public void MaskAccumulates_WeakerAttemptDoesNotShrinkReveal()
    {
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.Update("bekent", new[] { "bekend" });
        tracker.GetHint(new[] { "bekend" }).Should().Be("beken_");

        tracker.Update("bek___", new[] { "bekend" }); // weaker — gate still open, mask must not shrink
        tracker.GetHint(new[] { "bekend" }).Should().Be("beken_");
    }

    // ── Bonus reveal ──────────────────────────────────────────────────────────

    [Fact]
    public void BonusReveal_GateClosed_NotLocked_LocksAndRevealsPrimaryOption()
    {
        // Gate stays closed on "xy" vs options; bonus fires → locks to index 0, reveals first char.
        var tracker = new LetterHintTracker(bonusRevealDecider: () => true);
        tracker.Update("xy", new[] { "bezetten", "other" });
        // Locked to "bezetten" (index 0), first non-space char 'b' revealed
        tracker.GetHint(new[] { "bezetten", "other" }).Should().Be("b_______");
    }

    [Fact]
    public void BonusReveal_GateClosed_WhenLocked_RevealsLeftmostNonSpaceChar()
    {
        // First update opens gate (locks to "bezetten"); second update closes gate but
        // bonus fires, revealing the next unrevealed non-space char.
        var tracker = new LetterHintTracker(bonusRevealDecider: () => true);
        tracker.Update("bezeten", new[] { "bezetten" }); // gate opens: "bezet_en"
        tracker.Update("xy", new[] { "bezetten" });      // gate closed, locked, bonus=true: reveals next char
        // After first update: "bezet_en" — the '_' is index 5 ('t'), so bonus reveals it
        tracker.GetHint(new[] { "bezetten" }).Should().Be("bezetten");
    }

    [Fact]
    public void BonusReveal_SkipsAlreadyRevealedChars()
    {
        // Gate opens on "bekent" → "beken_"; then gate-closed attempt with bonus=true
        // reveals the next unrevealed non-space char ("d" at index 5)
        var tracker = new LetterHintTracker(bonusRevealDecider: () => true);
        tracker.Update("bekent", new[] { "bekend" }); // gate opens: "beken_"
        tracker.Update("xy", new[] { "bekend" });     // gate closed, bonus=true: reveals "d"
        tracker.GetHint(new[] { "bekend" }).Should().Be("bekend");
    }

    [Fact]
    public void BonusReveal_GateOpenButNoNewReveals_BonusFires()
    {
        // "regeaaaa" aligns to "regelen" — "rege" (run of 4) opens gate, mask becomes "rege___"
        // "regebbbb" — same prefix match, gate would open but reveals nothing new;
        // bonus should fire and reveal next unrevealed char 'l' at index 4
        var tracker = new LetterHintTracker(bonusRevealDecider: () => true);
        tracker.Update("regeaaaa", new[] { "regelen" });
        tracker.GetHint(new[] { "regelen" }).Should().Be("rege___");

        tracker.Update("regebbbb", new[] { "regelen" });
        tracker.GetHint(new[] { "regelen" }).Should().Be("regel__");
    }

    // ── Multi-option behavior ─────────────────────────────────────────────────

    [Fact]
    public void MultiOption_BestMatchChosen_GateOpen()
    {
        // "caat" aligns better to "cat" (run of 3) than to "dog" (no match).
        // Gate opens, locks to "cat", hint renders against "cat".
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        tracker.Update("caat", new[] { "dog", "cat" });
        tracker.GetHint(new[] { "dog", "cat" }).Should().Be("cat");
    }

    [Fact]
    public void MultiOption_LockedAfterGateOpen_StaysLocked()
    {
        // First update aligns to "appointment" (long run of 8), gate opens, locks to index 0.
        // Second update types "agreement" which is closer to options[1], but the lock must hold.
        var tracker = new LetterHintTracker(bonusRevealDecider: () => false);
        string[] options = ["appointment", "agreement"];

        // "appointmnt" vs "appointment": run of 8 ("appointm"), gate opens → locked to index 0
        tracker.Update("appointmnt", options);
        var firstHint = tracker.GetHint(options);
        firstHint.Should().NotBeNull();
        // Hint is from "appointment", not "agreement"
        firstHint.Should().StartWith("a");
        firstHint!.Length.Should().Be("appointment".Length);

        // Now type the exact text of the second option — tracker must stay locked to "appointment"
        tracker.Update("agreement", options);
        // Still locked to "appointment": hint length must be 11 (not 9 for "agreement")
        tracker.GetHint(options)!.Length.Should().Be("appointment".Length);
    }

    [Fact]
    public void MultiOption_Bonus_WhenNotLocked_LocksToFirstOption()
    {
        // "xyz" matches neither "foo" nor "bar" well enough to open gate;
        // bonus fires → locks to index 0 ("foo"), reveals first char 'f'.
        var tracker = new LetterHintTracker(bonusRevealDecider: () => true);
        tracker.Update("xyz", new[] { "foo", "bar" });
        tracker.GetHint(new[] { "foo", "bar" }).Should().Be("f__");
    }
}
