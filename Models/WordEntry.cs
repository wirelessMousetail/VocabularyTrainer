using VocabularyTrainer.Services.Utils;
using VocabularyTrainer.Services.Vocabulary;

namespace VocabularyTrainer.Models;

/// <summary>
/// Represents a vocabulary word entry with its translation, learning progress, and categorization.
/// </summary>
public class WordEntry
{
    /// <summary>
    /// Gets the question word or phrase 
    /// </summary>
    public string Question { get; }

    /// <summary>
    /// Gets the answer translation
    /// </summary>
    public string Answer { get; }

    /// <summary>
    /// Gets the answer with parenthetical groups stripped, ready for display.
    /// </summary>
    public string CanonicalAnswer => AnswerParser.Canonical(Answer);

    /// <summary>
    /// Gets the grammatical group classification of the word (Noun, Verb, or Other).
    /// </summary>
    public WordGroup Group { get; }

    /// <summary>
    /// Gets the weight data tracking learning progress (weight and correct streak).
    /// </summary>
    public WeightData WeightData { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WordEntry"/> class.
    /// </summary>
    /// <param name="question">The question word or phrase.</param>
    /// <param name="answer">The answer translation.</param>
    /// <param name="weightData">Optional weight data. If null, initializes with default values (weight=0, streak=0).</param>
    /// <param name="group">Optional word group. If null, automatically detects the group using <see cref="WordGrouping.Detect"/>.</param>
    public WordEntry(string question, string answer, WeightData? weightData = null, WordGroup? group = null)
    {
        Question = question;
        Answer = answer;
        Group = group ?? WordGrouping.Detect(question);
        WeightData = weightData ?? new WeightData();
    }
}