using System.Collections.Generic;

namespace VocabularyTrainer.Models;

/// <summary>
/// Immutable data model for a single quiz question, including the correct answer, multiple-choice options, and distractor metadata.
/// </summary>
public class Quiz
{
    /// <summary>
    /// Gets the text shown to the user as the question.
    /// </summary>
    public string Question { get; }

    /// <summary>
    /// Gets the correct answer string that the user must select or type.
    /// </summary>
    public string CorrectAnswer { get; }

    /// <summary>
    /// Gets the shuffled list of answer option strings (includes the correct answer).
    /// Empty for typing-mode quizzes.
    /// </summary>
    public List<string> Options { get; }

    /// <summary>
    /// Gets the vocabulary word entry that this quiz is testing.
    /// </summary>
    public WordEntry WordEntry { get; }

    /// <summary>
    /// Maps each distractor option string to its source <see cref="WordEntry"/>.
    /// Used to apply a weight penalty to the word the user confused with the correct answer.
    /// Does not contain the correct answer itself.
    /// </summary>
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
