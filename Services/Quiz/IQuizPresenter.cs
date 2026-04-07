namespace VocabularyTrainer.Services.Quiz;

public interface IQuizPresenter
{
    void OnAnswerSelected(string selectedAnswer);
    QuizResult GetResult();
    string GetCorrectAnswer();
    string? GetHint() => null;
}

public enum QuizResult
{
    Pending,
    Correct,
    Wrong,
    WrongArticle,
    MaxAttemptsReached
}
