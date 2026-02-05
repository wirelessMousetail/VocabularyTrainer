using VocabularyTrainer.Services;

namespace VocabularyTrainer.Models;

public class WordEntry
{
    public string Question { get; }
    public string Answer { get; }
    
    public WordGroup Group { get; }

    public WordEntry(string question, string answer)
    {
        Question = question;
        Answer = answer;
        Group = WordGrouping.Detect(question);
    }
}