namespace Faerie.Parsing;

/// <summary>Classic edit-distance for fuzzy word matching.</summary>
public static class Levenshtein
{
    public static int Distance(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        int[] prev = new int[b.Length + 1];
        int[] cur = new int[b.Length + 1];

        for (int j = 0; j <= b.Length; j++) prev[j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            cur[0] = i;
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                cur[j] = Math.Min(
                    Math.Min(cur[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + cost);
            }

            (prev, cur) = (cur, prev);
        }

        return prev[b.Length];
    }
}
