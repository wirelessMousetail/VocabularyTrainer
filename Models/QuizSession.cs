using VocabularyTrainer.Services.Quiz.Presenters;

namespace VocabularyTrainer.Models;

/// <summary>
/// Represents a complete quiz session bundling the quiz data, interaction logic, and configuration.
/// </summary>
public class QuizSession
{
    /// <summary>
    /// Gets the quiz data including question, correct answer, and options.
    /// </summary>
    public Quiz Quiz { get; }

    /// <summary>
    /// Gets the presenter handling quiz interaction logic.
    /// </summary>
    public IQuizPresenter Presenter { get; }

    /// <summary>
    /// Gets the configuration settings for this quiz session.
    /// </summary>
    public QuizConfiguration Configuration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuizSession"/> class.
    /// </summary>
    /// <param name="quiz">The quiz data.</param>
    /// <param name="presenter">The quiz presenter handling interaction logic.</param>
    /// <param name="configuration">The quiz configuration settings.</param>
    public QuizSession(Quiz quiz, IQuizPresenter presenter, QuizConfiguration configuration)
    {
        Quiz = quiz;
        Presenter = presenter;
        Configuration = configuration;
    }
}
