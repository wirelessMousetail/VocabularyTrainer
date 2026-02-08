using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;
using Timer = System.Windows.Forms.Timer;

namespace VocabularyTrainer;

public partial class QuizForm : Form
{
    private readonly QuizSession _session;
    private readonly List<Button> _answerButtons = new();
    private Timer? _autoCloseTimer;

    public QuizForm(QuizSession session)
    {
        _session = session;

        InitializeComponent();
        LoadQuiz();

        MouseDown += OnAnyClick;
    }

    private void LoadQuiz()
    {
        questionLabel.Text = _session.Quiz.Question;
        resultLabel.Text = string.Empty;
        
        CreateAnswerButtons();
        PerformLayoutManually();
    }

    private void CreateAnswerButtons()
    {
        int buttonIndex = 0;
        foreach (var option in _session.Quiz.Options)
        {
            var button = new Button
            {
                Text = option,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Name = $"answerButton{buttonIndex++}",
                MinimumSize = new System.Drawing.Size(100, 30)
            };
            
            button.Click += AnswerClicked;
            button.MouseDown += OnAnyClick;
            
            _answerButtons.Add(button);
            Controls.Add(button);
        }
    }

    private void AnswerClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        _session.Presenter.OnAnswerSelected(button.Text);
        var result = _session.Presenter.GetResult();

        switch (result)
        {
            case QuizResult.Correct:
                resultLabel.Text = "Correct!";
                StartAutoCloseTimer();
                break;
            case QuizResult.Wrong:
                resultLabel.Text = "Wrong!";
                if (_session.Configuration.ShowCorrectAnswerOnWrong)
                {
                    resultLabel.Text += $" (Correct: {((QuizPresenter)_session.Presenter).GetCorrectAnswer()})";
                }
                break;
            case QuizResult.MaxAttemptsReached:
                resultLabel.Text = $"Max attempts reached. Answer: {((QuizPresenter)_session.Presenter).GetCorrectAnswer()}";
                StartAutoCloseTimer();
                break;
        }
    }
    
    private void PerformLayoutManually()
    {
        int padding = 12;
        int y = padding;

        questionLabel.Location = new System.Drawing.Point(padding, y);
        y += questionLabel.Height + padding;

        foreach (var button in _answerButtons)
        {
            button.Location = new System.Drawing.Point(padding, y);
            y += button.Height + padding;
        }

        resultLabel.Location = new System.Drawing.Point(padding, y);
        y += resultLabel.Height + padding;

        var maxButtonWidth = 0;
        foreach (var button in _answerButtons)
        {
            maxButtonWidth = Math.Max(maxButtonWidth, button.Width);
        }

        var maxElementWidth = Math.Max(questionLabel.Width, maxButtonWidth);
        var clientWidth = Math.Max(350, maxElementWidth + padding * 2);

        ClientSize = new System.Drawing.Size(clientWidth, y);
    }

    private void OnAnyClick(object? sender, MouseEventArgs e)
    {
        var result = _session.Presenter.GetResult();
        if (result == QuizResult.Correct || result == QuizResult.MaxAttemptsReached)
        {
            Close();
        }
    }
    
    private void StartAutoCloseTimer()
    {
        if (_autoCloseTimer != null)
            return;

        _autoCloseTimer = new Timer();
        _autoCloseTimer.Interval = _session.Configuration.AutoCloseAfterCorrectSeconds * 1000;
        _autoCloseTimer.Tick += (_, _) =>
        {
            _autoCloseTimer!.Stop();
            Close();
        };
        _autoCloseTimer.Start();
    }
    
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _autoCloseTimer?.Stop();
        _autoCloseTimer?.Dispose();
        base.OnFormClosed(e);
    }
}

// todo
//  Далее:
//  + сделать размеры автоподстраиваемыми
//  + Закрывать форму через 5 секунд после правильного ответа
//  + Таймер стартовать только после того, как предыдущее закрыто
//  + Сделать без хедера, сплошным бирюзовым цветом, кнопки - подстраиваемые (крестик должен быть)
//  + Сделать тайметр настраиваемым (добавить поле для насттройки сколько раз в день спрашивать?)
//  + Добавить возможность определять часть речи автоматически и выдавать только одинаковые
//  + Сохранять опции в конфиг файл (если еще не)
//  + Добавить в опции выбор количества вариантов
//  + Добавить веса, прописывать в цсвхе. Если ошибаешься - увеличивать вес, при достаточном количестве правильных ответов - уменьшать (Но не сразу до нормы)
//  + При обновлении цсв приложением сохранять ее отдельно. При старте программы мержить изначальную с ней
//  + Добавление новых слов
//  * Группировать по части речи при загрузке новых слов, читать ее из списка. Если пустое поле - вычислять
//  * Добавить возможность изменить "направление" опроса eng-dutch and vice versa
//  * Открывать нвоый вопрос двойным щелчком по трею
//  ** Возможность просить напечатать ответ (требуется нормализация списка)
//  * Логику таймера в отдельный интерфейс (чтобы можно было свичнуться в начало отсчета после окончания предыдущего вместо ответа)

//todo chatgpt next steps proposal:
// When you’re ready, we can:
//   + Prevent multiple windows
//   + Configure interval
//   + Add tray icon
//   + Remember wrong answers 
//   Track stats
//   live preview of “next quiz in X minutes”
//   Just tell me what you want next 😄