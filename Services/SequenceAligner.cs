namespace VocabularyTrainer.Services;

/// <summary>
/// Pure sequence alignment: aligns <c>typed</c> against <c>correct</c> using edit distance,
/// returning a bool[] marking which positions in <c>correct</c> were matched.
/// No gate logic — always returns a result.
/// </summary>
public static class SequenceAligner
{
    /// <summary>
    /// Returns a <c>bool[correct.Length]</c> marking which positions in <paramref name="correct"/>
    /// are matched by <paramref name="typed"/> via edit-distance alignment.
    /// Includes skip-correct tie-breaking: prefers aligning typed chars to earlier (leftmost)
    /// correct positions, giving more intuitive hints.
    /// </summary>
    public static bool[] FindMatches(string typed, string correct)
    {
        int m = typed.Length;
        int n = correct.Length;

        // Build DP table for edit distance
        var dp = new int[m + 1, n + 1];
        for (int i = 0; i <= m; i++) dp[i, 0] = i;
        for (int j = 0; j <= n; j++) dp[0, j] = j;

        for (int i = 1; i <= m; i++)
        for (int j = 1; j <= n; j++)
        {
            dp[i, j] = typed[i - 1] == correct[j - 1]
                ? dp[i - 1, j - 1]
                : 1 + Math.Min(dp[i - 1, j - 1], Math.Min(dp[i - 1, j], dp[i, j - 1]));
        }

        // Backtrace to find which positions in correct are matched.
        // Tie-breaking rule: when a diagonal match ties with skipping a correct char,
        // prefer the skip — this aligns typed chars with earlier (leftmost) correct chars,
        // giving more intuitive hints (e.g. "bezet_en" instead of "beze_ten" for bezeten/bezetten).
        var matched = new bool[n];
        int ci = m, cj = n;
        while (ci > 0 && cj > 0)
        {
            if (typed[ci - 1] == correct[cj - 1] && dp[ci, cj] == dp[ci - 1, cj - 1])
            {
                // Match is possible here. Check for skip-correct tie:
                // if there is at least one more correct char to the left AND skipping this
                // correct char costs the same, do the skip so the typed char aligns earlier.
                if (cj > 1 && dp[ci, cj - 1] + 1 == dp[ci, cj])
                    cj--;
                else
                {
                    matched[cj - 1] = true;
                    ci--; cj--;
                }
            }
            else if (dp[ci, cj] == dp[ci - 1, cj - 1] + 1)
            {
                // Substitution
                ci--; cj--;
            }
            else if (dp[ci, cj] == dp[ci - 1, cj] + 1)
            {
                // Delete from typed (advance typed only)
                ci--;
            }
            else
            {
                // Skip correct char (advance correct only)
                cj--;
            }
        }

        return matched;
    }
}
