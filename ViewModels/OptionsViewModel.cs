using System;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.ViewModels;

/// <summary>
/// View model for the options/settings window.
/// </summary>
public class OptionsViewModel : ViewModelBase
{
    private readonly Action<int, int, int, QuizDirection, QuizDifficulty, bool> _onSave;
    private readonly Action _onClosed;

    private int _quizIntervalSeconds;
    private int _autoCloseSeconds;
    private int _optionCount;
    private bool _isDirectMode;
    private bool _isReverseMode;
    private bool _isRandomMode;
    private bool _isEasyMode;
    private bool _isHardMode;
    private bool _isTypingMode;
    private bool _isTypingRevealLetters;

    /// <summary>
    /// Gets or sets the quiz interval in seconds.
    /// </summary>
    public int QuizIntervalSeconds
    {
        get => _quizIntervalSeconds;
        set => SetProperty(ref _quizIntervalSeconds, value);
    }

    /// <summary>
    /// Gets or sets the auto-close delay in seconds.
    /// </summary>
    public int AutoCloseSeconds
    {
        get => _autoCloseSeconds;
        set => SetProperty(ref _autoCloseSeconds, value);
    }

    /// <summary>
    /// Gets or sets the number of answer options.
    /// </summary>
    public int OptionCount
    {
        get => _optionCount;
        set => SetProperty(ref _optionCount, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether Direct mode is selected.
    /// </summary>
    public bool IsDirectMode
    {
        get => _isDirectMode;
        set => SetProperty(ref _isDirectMode, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether Reverse mode is selected.
    /// </summary>
    public bool IsReverseMode
    {
        get => _isReverseMode;
        set => SetProperty(ref _isReverseMode, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether Random mode is selected.
    /// </summary>
    public bool IsRandomMode
    {
        get => _isRandomMode;
        set => SetProperty(ref _isRandomMode, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether Easy difficulty is selected.
    /// </summary>
    public bool IsEasyMode
    {
        get => _isEasyMode;
        set
        {
            SetProperty(ref _isEasyMode, value);
            if (value) IsTypingRevealLetters = false;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether Hard difficulty is selected.
    /// </summary>
    public bool IsHardMode
    {
        get => _isHardMode;
        set
        {
            SetProperty(ref _isHardMode, value);
            if (value) IsTypingRevealLetters = false;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether Typing difficulty is selected.
    /// </summary>
    public bool IsTypingMode
    {
        get => _isTypingMode;
        set => SetProperty(ref _isTypingMode, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether letters should be revealed on wrong attempts in Typing mode.
    /// </summary>
    public bool IsTypingRevealLetters
    {
        get => _isTypingRevealLetters;
        set
        {
            SetProperty(ref _isTypingRevealLetters, value);
            if (value && !_isTypingMode)
            {
                IsEasyMode = false;
                IsHardMode = false;
                IsTypingMode = true;
            }
        }
    }

    /// <summary>
    /// Command to save settings and close the window.
    /// </summary>
    public RelayCommand SaveCommand { get; }

    /// <summary>
    /// Command to cancel and close the window without saving.
    /// </summary>
    public RelayCommand CancelCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsViewModel"/> class.
    /// </summary>
    /// <param name="initialSettings">The current settings to populate the form.</param>
    /// <param name="onSave">Callback invoked with the new settings values when saving.</param>
    /// <param name="onClosed">Callback when the window is closed.</param>
    public OptionsViewModel(AppSettings initialSettings, Action<int, int, int, QuizDirection, QuizDifficulty, bool> onSave, Action onClosed)
    {
        _onSave = onSave ?? throw new ArgumentNullException(nameof(onSave));
        _onClosed = onClosed ?? throw new ArgumentNullException(nameof(onClosed));

        LoadFromSettings(initialSettings ?? throw new ArgumentNullException(nameof(initialSettings)));

        SaveCommand = new RelayCommand(SaveAndClose);
        CancelCommand = new RelayCommand(_onClosed);
    }

    private void LoadFromSettings(AppSettings settings)
    {
        QuizIntervalSeconds = settings.QuizIntervalSeconds;
        AutoCloseSeconds = settings.QuizConfiguration.AutoCloseAfterCorrectSeconds;
        OptionCount = settings.QuizConfiguration.OptionCount;

        switch (settings.QuizConfiguration.Direction)
        {
            case QuizDirection.Direct:
                IsDirectMode = true;
                break;
            case QuizDirection.Reverse:
                IsReverseMode = true;
                break;
            case QuizDirection.Random:
                IsRandomMode = true;
                break;
        }

        switch (settings.QuizConfiguration.Difficulty)
        {
            case QuizDifficulty.Easy:   IsEasyMode = true; break;
            case QuizDifficulty.Hard:   IsHardMode = true; break;
            case QuizDifficulty.Typing: IsTypingMode = true; break;
        }
        IsTypingRevealLetters = settings.QuizConfiguration.TypingRevealLetters;
    }

    private QuizDirection GetSelectedDirection()
    {
        if (IsDirectMode)
            return QuizDirection.Direct;
        if (IsReverseMode)
            return QuizDirection.Reverse;
        return QuizDirection.Random;
    }

    private QuizDifficulty GetSelectedDifficulty() =>
        IsHardMode   ? QuizDifficulty.Hard   :
        IsTypingMode ? QuizDifficulty.Typing :
        QuizDifficulty.Easy;

    private void SaveAndClose()
    {
        _onSave(QuizIntervalSeconds, AutoCloseSeconds, OptionCount, GetSelectedDirection(), GetSelectedDifficulty(), IsTypingRevealLetters);
        _onClosed();
    }
}
