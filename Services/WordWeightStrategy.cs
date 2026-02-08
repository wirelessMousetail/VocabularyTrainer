using System;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

public class WordWeightStrategy
{
    private const int MaxWeight = 100;
    private const int LinearStreakThreshold = 5;

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

    public void RegisterMistake(WordEntry wordEntry)
    {
        var weightData = wordEntry.WeightData;
        weightData.CorrectStreak = 0;
        
        // Exponential growth with cap
        weightData.Weight = (int)(weightData.Weight * 3 + 1);
        weightData.Weight = Math.Min(weightData.Weight, MaxWeight);
    }

    public int CalculateTickets(WordEntry wordEntry)
    {
        // 1 base ticket + weight tickets
        return 1 + wordEntry.WeightData.Weight;
    }
}
