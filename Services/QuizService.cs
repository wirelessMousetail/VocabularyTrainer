using System.Collections.Generic;
using System.Linq;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

public class QuizService
{
    private readonly List<WordEntry> _words;
    private readonly WordWeightStrategy _weightStrategy;

    public QuizService(List<WordEntry> words, WordWeightStrategy weightStrategy)
    {
        _words = words;
        _weightStrategy = weightStrategy;
    }

    public QuizSession CreateQuizSession(QuizConfiguration configuration, WordListService wordListService)
    {
        var quiz = CreateQuiz(configuration.OptionCount, configuration.Direction);
        var presenter = new QuizPresenter(quiz, _weightStrategy, wordListService, configuration.MaxAttemptsPerQuiz);
        return new QuizSession(quiz, presenter, configuration);
    }

    private Quiz CreateQuiz(int optionCount, QuizDirection direction)
    {
        var correct = SelectWordByWeight();

        // Determine actual direction for this quiz
        bool isReversed = direction switch
        {
            QuizDirection.Direct => false,
            QuizDirection.Reverse => true,
            QuizDirection.Random => Random.Shared.Next(2) == 1,
            _ => false
        };

        var sameGroupItems = _words
            .Where(i =>
                i != correct &&
                i.Group == correct.Group)
            .ToList();

        IEnumerable<WordEntry> pool = sameGroupItems.Count >= optionCount - 1
            ? sameGroupItems
            : _words.Where(w => w != correct);

        var options = pool
            .OrderBy(_ => Random.Shared.Next())
            .Take(optionCount - 1)
            .Select(w => isReversed ? w.Question : w.Answer)
            .Distinct()
            .ToList();

        options.Add(isReversed ? correct.Question : correct.Answer);

        options = options
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        return new Quiz(
            isReversed ? correct.Answer : correct.Question,
            isReversed ? correct.Question : correct.Answer,
            options,
            correct
        );
    }

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