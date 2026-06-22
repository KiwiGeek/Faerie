namespace Faerie.Runtime;

/// <summary>
/// Signed mood counters for creatures whose disposition escalates toward two extremes
/// (e.g. Zork cyclops wrath: negative = thirsty, positive = angry).
/// </summary>
public static class CreatureMood
{
    /// <summary>Moves the mood one step away from zero (more negative or more positive).</summary>
    public static int Escalate(int mood) => mood < 0 ? mood - 1 : mood + 1;

    /// <summary>True when <see cref="Math.Abs(int)"/> of <paramref name="mood"/> reaches <paramref name="threshold"/>.</summary>
    public static bool IsLethal(int mood, int threshold) => Math.Abs(mood) >= threshold;

    /// <summary>Zero-based index into a staged message table for the current mood level.</summary>
    public static int StageIndex(int mood, int maxIndex) =>
        Math.Clamp(Math.Abs(mood) - 1, 0, maxIndex);

    /// <summary>Flips the sign (e.g. waking a creature who was thirsty).</summary>
    public static int FlipSign(int mood) => -mood;

    /// <summary>Sets a negative mood after a meal, preserving magnitude when already negative.</summary>
    public static int ThirstyAfterMeal(int mood) => Math.Min(-1, -Math.Abs(mood));
}
