namespace VocabularyTrainer.Services;

public static class StringDistance
{
    public static int Levenshtein(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        var prev = new int[b.Length + 1];
        var curr = new int[b.Length + 1];

        for (int j = 0; j <= b.Length; j++) prev[j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(
                    Math.Min(curr[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + cost);
            }
            Array.Copy(curr, prev, b.Length + 1);
        }
        return prev[b.Length];
    }

    public static double NormalizedLevenshtein(string a, string b)
    {
        int maxLen = Math.Max(a.Length, b.Length);
        if (maxLen == 0) return 0.0;
        return (double)Levenshtein(a, b) / maxLen;
    }

    /// <summary>
    /// Jaro-Winkler similarity. Returns 1.0 for identical strings, 0.0 for completely dissimilar.
    /// Gives a prefix bonus: strings sharing the first 1–4 characters score higher,
    /// reflecting that words with a common start look more similar to the human eye.
    /// </summary>
    public static double JaroWinkler(string a, string b)
    {
        if (a == b) return 1.0;
        if (a.Length == 0 || b.Length == 0) return 0.0;

        int matchWindow = Math.Max(Math.Max(a.Length, b.Length) / 2 - 1, 0);

        var aMatched = new bool[a.Length];
        var bMatched = new bool[b.Length];
        int matches = 0;

        for (int i = 0; i < a.Length; i++)
        {
            int start = Math.Max(0, i - matchWindow);
            int end   = Math.Min(b.Length - 1, i + matchWindow);
            for (int j = start; j <= end; j++)
            {
                if (bMatched[j] || a[i] != b[j]) continue;
                aMatched[i] = true;
                bMatched[j] = true;
                matches++;
                break;
            }
        }

        if (matches == 0) return 0.0;

        int transpositions = 0;
        int k = 0;
        for (int i = 0; i < a.Length; i++)
        {
            if (!aMatched[i]) continue;
            while (!bMatched[k]) k++;
            if (a[i] != b[k]) transpositions++;
            k++;
        }

        double jaro = (
            (double)matches / a.Length +
            (double)matches / b.Length +
            (double)(matches - transpositions / 2) / matches
        ) / 3.0;

        // Prefix bonus: up to 4 leading characters, scaling factor p = 0.1
        int prefix = 0;
        for (int i = 0; i < Math.Min(4, Math.Min(a.Length, b.Length)); i++)
        {
            if (a[i] == b[i]) prefix++;
            else break;
        }

        return jaro + prefix * 0.1 * (1.0 - jaro);
    }
}
