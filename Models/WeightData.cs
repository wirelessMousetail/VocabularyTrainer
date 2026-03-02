namespace VocabularyTrainer.Models;

/// <summary>
/// Encapsulates weight and streak data for tracking learning progress of a vocabulary word.
/// </summary>
public class WeightData
{
    /// <summary>
    /// Gets or sets the difficulty weight of the word (0-100).
    /// Higher weight indicates more difficulty and increases selection probability.
    /// </summary>
    public int Weight { get; set; }

    /// <summary>
    /// Gets or sets the number of consecutive correct answers for this word.
    /// Reset to 0 on any incorrect answer.
    /// </summary>
    public int CorrectStreak { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeightData"/> class.
    /// </summary>
    /// <param name="weight">Initial weight value. Default is 0.</param>
    /// <param name="correctStreak">Initial correct streak count. Default is 0.</param>
    public WeightData(int weight = 0, int correctStreak = 0)
    {
        Weight = weight;
        CorrectStreak = correctStreak;
    }
}
