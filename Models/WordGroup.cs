namespace VocabularyTrainer.Models;

/// <summary>
/// Represents the grammatical category of a vocabulary word.
/// </summary>
public enum WordGroup
{
    /// <summary>
    /// Noun - typically identified by "de" or "het" article in Dutch.
    /// </summary>
    Noun,

    /// <summary>
    /// Verb - typically identified by "-en" ending in Dutch (infinitive form) for words without an article
    /// </summary>
    Verb,

    /// <summary>
    /// Other - words that don't match Noun or Verb patterns (adjectives, adverbs, phrases).
    /// </summary>
    Other
}