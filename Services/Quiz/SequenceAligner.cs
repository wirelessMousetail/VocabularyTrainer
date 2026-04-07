namespace VocabularyTrainer.Services.Quiz;

/// <summary>
/// Pure sequence alignment: aligns <c>typed</c> against <c>correct</c> using edit distance,
/// returning a bool[] marking which positions in <c>correct</c> were matched.
/// No gate logic — always returns a result.
/// </summary>
public static class SequenceAligner
{
    /// <summary>
    /// Aligns <paramref name="typed"/> against <paramref name="correct"/> using classic
    /// Levenshtein edit distance and returns a <c>bool[correct.Length]</c> where each
    /// <c>true</c> entry marks a position in <paramref name="correct"/> that was matched
    /// by a character in <paramref name="typed"/>.
    /// </summary>
    /// <remarks>
    /// <para><b>Algorithm overview:</b></para>
    /// <list type="number">
    ///   <item><description>
    ///     Build an (m+1) × (n+1) DP table where <c>dp[i,j]</c> is the minimum edit distance
    ///     between the first <c>i</c> chars of <paramref name="typed"/> and the first <c>j</c>
    ///     chars of <paramref name="correct"/>. Cells are filled bottom-up with the standard
    ///     recurrence: match costs 0, substitution/insertion/deletion each cost 1.
    ///   </description></item>
    ///   <item><description>
    ///     Backtrace from <c>dp[m,n]</c> to <c>dp[0,0]</c>, reconstructing the optimal
    ///     alignment. At each step the backtrace chooses among match, substitution, typed-delete
    ///     (advance typed only), or correct-skip (advance correct only).
    ///   </description></item>
    ///   <item><description>
    ///     <b>Skip-correct tie-breaking:</b> when a diagonal match (typed[i]==correct[j]) is
    ///     possible but skipping the current correct char costs the same (<c>dp[i,j-1]+1 == dp[i,j]</c>),
    ///     the skip is preferred. This biases the alignment so that typed characters are
    ///     matched against the earliest (leftmost) possible positions in <paramref name="correct"/>,
    ///     producing more intuitive hints — e.g. "bezet_en" instead of "beze_ten" when the
    ///     user types "bezeten" for "bezetten".
    ///   </description></item>
    /// </list>
    /// <para>No gate logic is applied — the method always returns a result regardless of how
    /// many positions were matched. Gate policy is the caller's responsibility.</para>
    /// </remarks>
    /// <param name="typed">The user's typed attempt (should be pre-normalized: trimmed, lowercase).</param>
    /// <param name="correct">The correct answer to align against (should be pre-normalized).</param>
    /// <returns>
    /// A <c>bool[]</c> of length <c>correct.Length</c>. Entry <c>i</c> is <c>true</c> when
    /// position <c>i</c> of <paramref name="correct"/> was matched during the optimal alignment.
    /// </returns>
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
