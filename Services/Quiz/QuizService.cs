using VocabularyTrainer.Models;
using VocabularyTrainer.Services.Quiz.Distractors;
using VocabularyTrainer.Services.Quiz.Presenters;
using VocabularyTrainer.Services.Vocabulary;
using QuizModel = VocabularyTrainer.Models.Quiz;

namespace VocabularyTrainer.Services.Quiz;

/// <summary>
/// Service responsible for creating quiz sessions with weight-based word selection.
/// </summary>
public class QuizService
{
    private readonly List<WordEntry> _words;
    private readonly WordWeightStrategy _weightStrategy;
    private IDistractorSelector _selector;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuizService"/> class.
    /// </summary>
    /// <param name="words">The list of vocabulary words to use for quizzes.</param>
    /// <param name="weightStrategy">The strategy for calculating word weights and selection probability.</param>
    /// <param name="selector">The distractor selection strategy.</param>
    public QuizService(List<WordEntry> words, WordWeightStrategy weightStrategy, IDistractorSelector selector)
    {
        _words = words;
        _weightStrategy = weightStrategy;
        _selector = selector;
    }

    /// <summary>
    /// Updates the distractor selector when the difficulty setting changes.
    /// </summary>
    public void SetSelector(IDistractorSelector selector) => _selector = selector;

    /// <summary>
    /// Creates a new quiz session with selected word, options, and presenter logic.
    /// Returns null if the word list is empty.
    /// </summary>
    /// <param name="configuration">The quiz configuration settings.</param>
    /// <param name="wordListService">The word list service for saving progress.</param>
    /// <returns>A new <see cref="QuizSession"/> ready for user interaction, or null if no words are available.</returns>
    public QuizSession? CreateQuizSession(QuizConfiguration configuration, WordListService wordListService)
    {
        if (_words.Count == 0)
            return null;

        var word = SelectWordByWeight();

        if (configuration.Difficulty.IsTypingMode())
        {
            var quiz = CreateTypingQuiz(configuration.Direction, word);
            var presenter = new TypingQuizPresenter(quiz, _weightStrategy, wordListService, configuration.TypingRevealLetters);
            return new QuizSession(quiz, presenter, configuration);
        }

        var mcQuiz = CreateQuiz(configuration.OptionCount, configuration.Direction, word);
        var mcPresenter = new QuizPresenter(mcQuiz, _weightStrategy, wordListService, configuration.MaxAttemptsPerQuiz);
        return new QuizSession(mcQuiz, mcPresenter, configuration);
    }

    /// <summary>
    /// Creates a quiz session with a specific word as the question. Intended for testing.
    /// </summary>
    internal QuizSession CreateQuizSessionForWord(WordEntry word, QuizConfiguration configuration, WordListService wordListService)
    {
        if (configuration.Difficulty.IsTypingMode())
        {
            var quiz = CreateTypingQuiz(configuration.Direction, word);
            var presenter = new TypingQuizPresenter(quiz, _weightStrategy, wordListService, configuration.TypingRevealLetters);
            return new QuizSession(quiz, presenter, configuration);
        }

        var mcQuiz = CreateQuiz(configuration.OptionCount, configuration.Direction, word);
        var mcPresenter = new QuizPresenter(mcQuiz, _weightStrategy, wordListService, configuration.MaxAttemptsPerQuiz);
        return new QuizSession(mcQuiz, mcPresenter, configuration);
    }

    /// <summary>
    /// Creates a typing quiz (no options) for the specified word and direction.
    /// </summary>
    private QuizModel CreateTypingQuiz(QuizDirection direction, WordEntry correct)
    {
        bool isReversed = direction switch
        {
            QuizDirection.Direct => false,
            QuizDirection.Reverse => true,
            QuizDirection.Random => Random.Shared.Next(2) == 1,
            _ => false
        };

        return new QuizModel(
            isReversed ? correct.CanonicalAnswer : correct.Question,
            isReversed ? correct.Question : correct.CanonicalAnswer,
            [],
            correct,
            new Dictionary<string, WordEntry>()
        );
    }

    /// <summary>
    /// Creates a quiz with the specified number of options and direction.
    /// Selects a word by weight, generates options from the same word group, and applies quiz direction.
    /// </summary>
    /// <param name="optionCount">Number of multiple-choice options.</param>
    /// <param name="direction">Quiz direction (Direct, Reverse, or Random).</param>
    /// <returns>A configured <see cref="QuizModel"/> instance.</returns>
    private QuizModel CreateQuiz(int optionCount, QuizDirection direction, WordEntry correct)
    {
        // Determine actual direction for this quiz
        bool isReversed = direction switch
        {
            QuizDirection.Direct => false,
            QuizDirection.Reverse => true,
            QuizDirection.Random => Random.Shared.Next(2) == 1,
            _ => false
        };

        var pool = GetOptionPool(correct, optionCount, isReversed).ToList();

        var selected = _selector.Select(pool, correct, optionCount - 1);

        var options = selected
            .Select(w => isReversed ? w.Question : w.CanonicalAnswer)
            .Distinct()
            .ToList();

        options.Add(isReversed ? correct.Question : correct.CanonicalAnswer);
        options = options.OrderBy(_ => Random.Shared.Next()).ToList();

        var optionEntries = new Dictionary<string, WordEntry>();
        foreach (var w in selected)
            optionEntries.TryAdd(isReversed ? w.Question : w.CanonicalAnswer, w);

        return new QuizModel(
            isReversed ? correct.CanonicalAnswer : correct.Question,
            isReversed ? correct.Question : correct.CanonicalAnswer,
            options,
            correct,
            optionEntries
        );
    }

    private IEnumerable<WordEntry> GetOptionPool(WordEntry correct, int optionCount, bool isReversed)
    {
        var correctTarget = (isReversed ? correct.Question : correct.Answer).Trim();
        bool IsSynonym(WordEntry w) =>
            string.Equals((isReversed ? w.Question : w.Answer).Trim(), correctTarget, StringComparison.OrdinalIgnoreCase);

        var correctSource = (isReversed ? correct.Answer : correct.Question).Trim();
        bool IsAlsoCorrect(WordEntry w) =>
            string.Equals((isReversed ? w.Answer : w.Question).Trim(), correctSource, StringComparison.OrdinalIgnoreCase);

        var sameGroupItems = _words
            .Where(i => i != correct && i.Group == correct.Group && !IsSynonym(i) && !IsAlsoCorrect(i))
            .ToList();

        if (sameGroupItems.Count >= optionCount - 1)
            return sameGroupItems;

        return _words.Where(w => w != correct && !IsSynonym(w) && !IsAlsoCorrect(w));
    }

    /// <summary>
    /// Selects a word from the vocabulary using weight-based probability.
    /// Builds a ticket pool where each word receives (1 + weight) tickets, then randomly selects one.
    /// </summary>
    /// <returns>The selected <see cref="WordEntry"/>.</returns>
    private WordEntry SelectWordByWeight()
    {
        // Build ticket pool
        var tickets = new List<WordEntry>();
        foreach (var word in _words)
        {
            var ticketCount = _weightStrategy.CalculateTickets(word);
            for (int i = 0; i < ticketCount; i++)
            {
                tickets.Add(word);
            }
        }

        // Select random ticket
        return tickets[Random.Shared.Next(tickets.Count)];
    }
}
