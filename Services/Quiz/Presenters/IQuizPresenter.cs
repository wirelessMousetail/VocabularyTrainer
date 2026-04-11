namespace VocabularyTrainer.Services.Quiz.Presenters;

/// <summary>
/// Handles user interaction for a single quiz attempt: evaluating answers, tracking state, and exposing results.
/// </summary>
public interface IQuizPresenter
{
    /// <summary>
    /// Processes the user's selected or typed answer, updates weight data, and persists progress.
    /// Has no effect once the quiz has reached a terminal state (Correct or MaxAttemptsReached).
    /// </summary>
    /// <param name="selectedAnswer">The answer string chosen or typed by the user.</param>
    void OnAnswerSelected(string selectedAnswer);

    /// <summary>
    /// Returns the current result of the quiz attempt.
    /// </summary>
    QuizResult GetResult();

    /// <summary>
    /// Returns the correct answer string to display (e.g. after max attempts are reached).
    /// </summary>
    string GetCorrectAnswer();

    /// <summary>
    /// Returns a letter-reveal hint string (e.g. "bezet_en"), or null if no hint is available.
    /// Only applicable in typing mode with reveal-letters enabled; returns null by default.
    /// </summary>
    string? GetHint() => null;
}

/// <summary>
/// Represents the outcome of a quiz answer attempt.
/// </summary>
public enum QuizResult
{
    /// <summary>
    /// The quiz is in progress; no answer has been submitted yet or the last answer was wrong and more attempts remain.
    /// </summary>
    Pending,

    /// <summary>
    /// The user answered correctly.
    /// </summary>
    Correct,

    /// <summary>
    /// The answer was incorrect. The user may try again if attempts remain.
    /// </summary>
    Wrong,

    /// <summary>
    /// Typing mode only: the user typed the correct noun but with the wrong Dutch article (e.g. "de" vs "het").
    /// </summary>
    WrongArticle,

    /// <summary>
    /// The user has used all allowed attempts without a correct answer.
    /// </summary>
    MaxAttemptsReached
}
