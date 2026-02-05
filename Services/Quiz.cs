using System.Collections.Generic;

namespace VocabularyTrainer.Services;

public class Quiz
{
    public string Question { get; }
    public string CorrectAnswer { get; }
    public List<string> Options { get; }

    public Quiz(string question, string correctAnswer, List<string> options)
    {
        Question = question;
        CorrectAnswer = correctAnswer;
        Options = options;
    }
}
