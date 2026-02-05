using VocabularyTrainer.Services;

namespace VocabularyTrainer.Models;

public class QuizSession
{
    public Quiz Quiz { get; }
    public IQuizPresenter Presenter { get; }
    public QuizConfiguration Configuration { get; }

    public QuizSession(Quiz quiz, IQuizPresenter presenter, QuizConfiguration configuration)
    {
        Quiz = quiz;
        Presenter = presenter;
        Configuration = configuration;
    }
}
