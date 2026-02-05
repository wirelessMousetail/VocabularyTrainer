namespace VocabularyTrainer.Services;

public interface IQuizPresenter
{
    void OnAnswerSelected(string selectedAnswer);
    QuizResult GetResult();
}

public enum QuizResult
{
    Pending,
    Correct,
    Wrong,
    MaxAttemptsReached
}
