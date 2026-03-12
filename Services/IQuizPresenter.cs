namespace VocabularyTrainer.Services;

public interface IQuizPresenter
{
    void OnAnswerSelected(string selectedAnswer);
    QuizResult GetResult();
    string GetCorrectAnswer();
}

public enum QuizResult
{
    Pending,
    Correct,
    Wrong,
    MaxAttemptsReached
}
