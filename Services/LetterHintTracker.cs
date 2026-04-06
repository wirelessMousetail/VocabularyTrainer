namespace VocabularyTrainer.Services;

/// <summary>
/// Tracks which letters of the correct answer have been revealed based on the user's typed attempts.
/// Encapsulates gate policy (contiguous block ≥ minBlock), random bonus reveal, and mask accumulation.
/// Supports multi-option answers: locks to the option the user aligns best with once the gate opens.
/// </summary>
public class LetterHintTracker
{
    private bool[]? _revealMask;
    private int? _lockedOptionIndex;
    private readonly int _minBlock;
    private readonly Func<bool> _bonusRevealDecider;

    public LetterHintTracker(int minBlock = 3, Func<bool>? bonusRevealDecider = null)
    {
        _minBlock = minBlock;
        _bonusRevealDecider = bonusRevealDecider ?? (() => Random.Shared.Next(3) == 0);
    }

    /// <summary>
    /// Call after each wrong attempt. Normalizes typed input, selects the best-matching option
    /// (or uses the locked option if already set), applies gate + random bonus, and merges into
    /// the accumulated mask.
    /// </summary>
    public void Update(string typed, IReadOnlyList<string> options)
    {
        var lowerTyped = typed.Trim().ToLowerInvariant();

        if (_lockedOptionIndex.HasValue)
        {
            // Already locked — only align against the locked option
            var lockedOption = options[_lockedOptionIndex.Value].Trim().ToLowerInvariant();
            var matched = SequenceAligner.FindMatches(lowerTyped, lockedOption);
            if (GateOpen(matched) && HasNewReveals(matched))
            {
                MergeMask(matched);
            }
            else if (_bonusRevealDecider())
            {
                RevealRandomUnrevealed(lockedOption);
            }
        }
        else
        {
            // Not locked — try all options and pick the best match
            int bestIdx = 0;
            bool[] bestMatched = [];
            int bestRun = 0;

            for (int i = 0; i < options.Count; i++)
            {
                var lowerOption = options[i].Trim().ToLowerInvariant();
                var matched = SequenceAligner.FindMatches(lowerTyped, lowerOption);
                int run = LongestContiguousRun(matched);
                // Ties prefer index 0 (lower index wins when runs are equal)
                if (run > bestRun)
                {
                    bestRun = run;
                    bestIdx = i;
                    bestMatched = matched;
                }
            }

            if (bestRun >= _minBlock)
            {
                _lockedOptionIndex = bestIdx;
                MergeMask(bestMatched);
            }
            else if (_bonusRevealDecider())
            {
                // Gate closed and not locked — lock to primary option (index 0) and reveal a random char
                _lockedOptionIndex = 0;
                RevealRandomUnrevealed(options[0].Trim().ToLowerInvariant());
            }
        }
    }

    /// <summary>
    /// Returns null when nothing has been revealed yet.
    /// Returns the hint string (e.g. "bezet_en") once any reveal has occurred.
    /// Spaces are always visible. Uses original (non-normalized) casing of the locked option.
    /// </summary>
    public string? GetHint(IReadOnlyList<string> options)
    {
        if (_revealMask == null || !_lockedOptionIndex.HasValue)
            return null;

        var option = options[_lockedOptionIndex.Value].Trim();
        var chars = new char[option.Length];
        for (int i = 0; i < option.Length; i++)
        {
            if (i < _revealMask.Length && (_revealMask[i] || option[i] == ' '))
                chars[i] = option[i];
            else
                chars[i] = '_';
        }
        return new string(chars);
    }

    private void MergeMask(bool[] matched)
    {
        if (_revealMask == null)
            _revealMask = matched;
        else
            for (int i = 0; i < _revealMask.Length; i++)
                _revealMask[i] |= matched[i];
    }

    private void RevealRandomUnrevealed(string lowerOption)
    {
        _revealMask ??= new bool[lowerOption.Length];
        var candidates = new List<int>();
        for (int i = 0; i < lowerOption.Length; i++)
            if (!_revealMask[i] && lowerOption[i] != ' ')
                candidates.Add(i);
        if (candidates.Count > 1)
            _revealMask[candidates[Random.Shared.Next(candidates.Count)]] = true;
    }

    private bool HasNewReveals(bool[] matched)
    {
        if (_revealMask == null) return true;
        for (int i = 0; i < matched.Length && i < _revealMask.Length; i++)
            if (matched[i] && !_revealMask[i]) return true;
        return false;
    }

    private bool GateOpen(bool[] matched)
    {
        int blockLen = 0;
        for (int j = 0; j < matched.Length; j++)
        {
            blockLen = matched[j] ? blockLen + 1 : 0;
            if (blockLen >= _minBlock) return true;
        }
        return false;
    }

    private static int LongestContiguousRun(bool[] matched)
    {
        int best = 0;
        int current = 0;
        foreach (var b in matched)
        {
            current = b ? current + 1 : 0;
            if (current > best) best = current;
        }
        return best;
    }
}
