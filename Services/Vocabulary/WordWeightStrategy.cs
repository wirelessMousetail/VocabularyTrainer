using System;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services.Vocabulary;

/// <summary>
/// Implements the weight management algorithm for vocabulary learning.
/// Adjusts word weights based on correct/incorrect answers to optimize learning focus.
/// </summary>
public class WordWeightStrategy
{
    public const int MaxWeight = 100;
    public const int LinearStreakThreshold = 5;

    /// <summary>
    /// Registers a correct answer for a word, decreasing its weight and incrementing the correct streak.
    /// Uses linear decrease for first 5 correct answers, then exponential decrease.
    /// </summary>
    /// <param name="wordEntry">The word entry to update.</param>
    public void RegisterCorrect(WordEntry wordEntry)
    {
        var weightData = wordEntry.WeightData;
        weightData.CorrectStreak++;

        if (weightData.CorrectStreak <= LinearStreakThreshold)
        {
            // Linear decrease for first 5 correct answers
            weightData.Weight = Math.Max(0, weightData.Weight - 1);
        }
        else
        {
            // Exponential decrease after 5 correct answers
            weightData.Weight = Math.Max(0, (int)(weightData.Weight* 0.5));
        }
    }

    /// <summary>
    /// Registers an incorrect answer for a word, increasing its weight exponentially and resetting the correct streak.
    /// Formula: weight = (weight × 3) + 1, capped at 100.
    /// </summary>
    /// <param name="wordEntry">The word entry to update.</param>
    public void RegisterMistake(WordEntry wordEntry)
    {
        var weightData = wordEntry.WeightData;
        weightData.CorrectStreak = 0;

        // Exponential growth with cap
        weightData.Weight = (int)(weightData.Weight * 3 + 1);
        weightData.Weight = Math.Min(weightData.Weight, MaxWeight);
    }

    /// <summary>
    /// Calculates the number of tickets a word receives in the selection pool.
    /// Higher weight words receive more tickets, increasing their probability of selection.
    /// </summary>
    /// <param name="wordEntry">The word entry to calculate tickets for.</param>
    /// <returns>The number of tickets (1 + weight).</returns>
    public int CalculateTickets(WordEntry wordEntry)
    {
        // 1 base ticket + weight tickets
        return 1 + wordEntry.WeightData.Weight;
    }
}
