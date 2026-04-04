namespace VocabularyTrainer.Services;

/// <summary>
/// Tracks which letters of the correct answer have been revealed based on the user's typed attempts.
/// Encapsulates gate policy (contiguous block ≥ minBlock), random bonus reveal, and mask accumulation.
/// </summary>
public class LetterHintTracker
{
    private bool[]? _revealMask;
    private readonly int _minBlock;
    private readonly Func<bool> _bonusRevealDecider;

    public LetterHintTracker(int minBlock = 3, Func<bool>? bonusRevealDecider = null)
    {
        _minBlock = minBlock;
        _bonusRevealDecider = bonusRevealDecider ?? (() => Random.Shared.Next(3) == 0);
    }

    /// <summary>
    /// Call after each wrong attempt. Normalizes both strings, computes alignment,
    /// applies gate + random bonus, and merges into the accumulated mask.
    /// </summary>
    public void Update(string typed, string correct)
    {
        var lowerTyped = typed.Trim().ToLowerInvariant();
        var lowerCorrect = correct.Trim().ToLowerInvariant();

        var matched = SequenceAligner.FindMatches(lowerTyped, lowerCorrect);

        // Gate check: only open if at least one contiguous block of matches is >= _minBlock
        int blockLen = 0;
        var gateOpen = GateOpen(matched, blockLen);

        if (gateOpen)
        {
            // Merge with existing mask (revealed letters are never hidden again)
            if (_revealMask == null)
                _revealMask = matched;
            else
                for (int i = 0; i < _revealMask.Length; i++)
                    _revealMask[i] |= matched[i];
        }
        else if (_bonusRevealDecider())
        {
            // Gate still closed — random bonus: reveal the leftmost not-yet-revealed non-space char
            _revealMask ??= new bool[lowerCorrect.Length];
            for (int i = 0; i < lowerCorrect.Length; i++)
            {
                if (!_revealMask[i] && lowerCorrect[i] != ' ')
                {
                    _revealMask[i] = true;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Returns null when nothing has been revealed yet.
    /// Returns the hint string (e.g. "bezet_en") once any reveal has occurred.
    /// Spaces are always visible. Uses original (non-normalized) casing of <paramref name="correct"/>.
    /// </summary>
    public string? GetHint(string correct)
    {
        if (_revealMask == null)
            return null;

        var trimmed = correct.Trim();
        var chars = new char[trimmed.Length];
        for (int i = 0; i < trimmed.Length; i++)
        {
            if (_revealMask[i] || trimmed[i] == ' ')
                chars[i] = trimmed[i];
            else
                chars[i] = '_';
        }
        return new string(chars);
    }

    private bool GateOpen(bool[] matched, int blockLen)
    {
        bool gateOpen = false;
        for (int j = 0; j < matched.Length; j++)
        {
            blockLen = matched[j] ? blockLen + 1 : 0;
            if (blockLen >= _minBlock) { gateOpen = true; break; }
        }

        return gateOpen;
    }
}
