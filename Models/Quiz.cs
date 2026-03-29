using System.Collections.Generic;

namespace VocabularyTrainer.Models;

public class Quiz
{
    public string Question { get; }
    public string CorrectAnswer { get; }
    public List<string> Options { get; }
    public WordEntry WordEntry { get; }

    public Quiz(string question, string correctAnswer, List<string> options, WordEntry wordEntry)
    {
        Question = question;
        CorrectAnswer = correctAnswer;
        Options = options;
        WordEntry = wordEntry;
    }
}
