using VocabularyTrainer.Services;

namespace VocabularyTrainer.Models;

public class WordEntry
{
    public string Question { get; }
    public string Answer { get; }
    
    public WordGroup Group { get; }
    public WeightData WeightData { get; }

    public WordEntry(string question, string answer, WeightData? weightData = null, WordGroup? group = null)
    {
        Question = question;
        Answer = answer;
        Group = group ?? WordGrouping.Detect(question);
        WeightData = weightData ?? new WeightData();
    }
}