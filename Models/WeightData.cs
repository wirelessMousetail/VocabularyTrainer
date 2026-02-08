namespace VocabularyTrainer.Models;

public class WeightData
{
    public int Weight { get; set; }
    public int CorrectStreak { get; set; }

    public WeightData(int weight = 0, int correctStreak = 0)
    {
        Weight = weight;
        CorrectStreak = correctStreak;
    }
}
