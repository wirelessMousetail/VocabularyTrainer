using System;
using VocabularyTrainer.Models;
using VocabularyTrainer.Services;

namespace VocabularyTrainer.ViewModels;

/// <summary>
/// View model for the options/settings window.
/// </summary>
public class OptionsViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly Action _onSettingsApplied;
    private readonly Action _onClosed;

    private int _quizIntervalSeconds;
    private int _autoCloseSeconds;
    private int _optionCount;
    private bool _isDirectMode;
    private bool _isReverseMode;
    private bool _isRandomMode;
    private bool _isEasyMode;
    private bool _isHardMode;

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
        set => SetProperty(ref _isEasyMode, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether Hard difficulty is selected.
    /// </summary>
    public bool IsHardMode
    {
        get => _isHardMode;
        set => SetProperty(ref _isHardMode, value);
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
    /// <param name="settingsService">The settings service.</param>
    /// <param name="onSettingsApplied">Callback when settings are saved.</param>
    /// <param name="onClosed">Callback when the window is closed.</param>
    public OptionsViewModel(SettingsService settingsService, Action onSettingsApplied, Action onClosed)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _onSettingsApplied = onSettingsApplied ?? throw new ArgumentNullException(nameof(onSettingsApplied));
        _onClosed = onClosed ?? throw new ArgumentNullException(nameof(onClosed));

        LoadFromSettings();

        SaveCommand = new RelayCommand(SaveAndClose);
        CancelCommand = new RelayCommand(_onClosed);
    }

    private void LoadFromSettings()
    {
        var settings = _settingsService.GetSettings();
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

        IsEasyMode = settings.QuizConfiguration.Difficulty == QuizDifficulty.Easy;
        IsHardMode = settings.QuizConfiguration.Difficulty == QuizDifficulty.Hard;
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
        IsHardMode ? QuizDifficulty.Hard : QuizDifficulty.Easy;

    private void SaveAndClose()
    {
        _settingsService.UpdateSettings(
            QuizIntervalSeconds,
            AutoCloseSeconds,
            OptionCount,
            GetSelectedDirection(),
            GetSelectedDifficulty()
        );

        _onSettingsApplied();
        _onClosed();
    }
}
