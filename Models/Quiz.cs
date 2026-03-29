using System.Collections.Generic;

namespace VocabularyTrainer.Models;

public class Quiz
{
    public string Question { get; }
    public string CorrectAnswer { get; }
    public List<string> Options { get; }
    public WordEntry WordEntry { get; }
    public IReadOnlyDictionary<string, WordEntry> OptionEntries { get; }

    public Quiz(string question, string correctAnswer, List<string> options, WordEntry wordEntry,
        IReadOnlyDictionary<string, WordEntry> optionEntries)
    {
        Question = question;
        CorrectAnswer = correctAnswer;
        Options = options;
        WordEntry = wordEntry;
        OptionEntries = optionEntries;
    }
}
