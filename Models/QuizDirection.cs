namespace VocabularyTrainer.Models;

/// <summary>
/// Specifies the direction of translation for quiz questions.
/// </summary>
public enum QuizDirection
{
    /// <summary>
    /// Direct mode - translate from question language to answer language (e.g., Dutch → English).
    /// </summary>
    Direct,

    /// <summary>
    /// Reverse mode - translate from answer language to question language (e.g., English → Dutch).
    /// </summary>
    Reverse,

    /// <summary>
    /// Random mode - randomly alternate between Direct and Reverse for each quiz.
    /// </summary>
    Random
}
