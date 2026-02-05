using System.Collections.Generic;
using System.Linq;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

public class QuizService
{
    private readonly List<WordEntry> _words;

    public QuizService(List<WordEntry> words)
    {
        _words = words;
    }

    public QuizSession CreateQuizSession(QuizConfiguration configuration)
    {
        var quiz = CreateQuiz(configuration.OptionCount);
        var presenter = new QuizPresenter(quiz, configuration.MaxAttemptsPerQuiz);
        return new QuizSession(quiz, presenter, configuration);
    }

    private Quiz CreateQuiz(int optionCount)
    {
        var correct = _words[Random.Shared.Next(_words.Count)];

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
            .Select(w => w.Answer)
            .Distinct()
            .ToList();

        options.Add(correct.Answer);

        options = options
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        return new Quiz(
            correct.Question,
            correct.Answer,
            options
        );
    }
}