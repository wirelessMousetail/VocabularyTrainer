using VocabularyTrainer.Models;
using VocabularyTrainer.Services;

namespace VocabularyTrainer;

using System;
using System.Windows.Forms;

public partial class OptionsForm : Form
{
    private readonly SettingsService _settingsService;
    private readonly Action _onSettingsApplied;

    private NumericUpDown _intervalInput;
    private NumericUpDown _autoCloseInput;
    private NumericUpDown _optionCountInput;
    private RadioButton _directRadio;
    private RadioButton _reverseRadio;
    private RadioButton _randomRadio;

    public OptionsForm(SettingsService settingsService, Action onSettingsApplied)
    {
        _settingsService = settingsService;
        _onSettingsApplied = onSettingsApplied;

        Text = "Options";
        MinimumSize = new Size(350, 260);
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        InitializeUI();
        LoadFromSettings();
    }
    
    private void InitializeUI()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            Padding = new Padding(15),
        };

        layout.RowStyles.Clear();

        _intervalInput = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 100000,
            Width = 150,
            Anchor = AnchorStyles.Left
        };

        var intervalPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 10, 0, 10)
        };

        intervalPanel.Controls.Add(new Label
        {
            Text = "Quiz interval (seconds):",
            AutoSize = true,
            Padding = new Padding(0, 5, 5, 0)
        });

        intervalPanel.Controls.Add(_intervalInput);

        _autoCloseInput = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 300,
            Width = 150,
            Anchor = AnchorStyles.Left
        };

        var autoClosePanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 15, 0, 10)
        };

        autoClosePanel.Controls.Add(new Label
        {
            Text = "Auto close after correct (seconds):",
            AutoSize = true,
            Padding = new Padding(0, 5, 5, 0)
        });

        autoClosePanel.Controls.Add(_autoCloseInput);

        _optionCountInput = new NumericUpDown
        {
            Minimum = 2,
            Maximum = 10,
            Width = 150,
            Anchor = AnchorStyles.Left
        };

        var optionCountPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 15, 0, 10)
        };

        optionCountPanel.Controls.Add(new Label
        {
            Text = "Number of answer options:",
            AutoSize = true,
            Padding = new Padding(0, 5, 5, 0)
        });

        optionCountPanel.Controls.Add(_optionCountInput);

        var directionGroupBox = new GroupBox
        {
            Text = "Quiz Direction",
            AutoSize = true,
            Margin = new Padding(0, 15, 0, 10),
            Padding = new Padding(10)
        };

        var directionPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            Dock = DockStyle.Fill
        };

        _directRadio = new RadioButton
        {
            Text = "Direct (Dutch → English)",
            AutoSize = true,
            Checked = true
        };

        _reverseRadio = new RadioButton
        {
            Text = "Reverse (English → Dutch)",
            AutoSize = true
        };

        _randomRadio = new RadioButton
        {
            Text = "Random",
            AutoSize = true
        };

        directionPanel.Controls.Add(_directRadio);
        directionPanel.Controls.Add(_reverseRadio);
        directionPanel.Controls.Add(_randomRadio);
        directionGroupBox.Controls.Add(directionPanel);

        var buttonsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 20, 0, 0)
        };

        var saveButton = new Button
        {
            Text = "Save",
            AutoSize = true,
            Padding = new Padding(10, 5, 10, 5)
        };

        saveButton.Click += (_, _) => SaveAndClose();

        buttonsPanel.Controls.Add(saveButton);

        layout.Controls.Add(intervalPanel);
        layout.Controls.Add(autoClosePanel);
        layout.Controls.Add(optionCountPanel);
        layout.Controls.Add(directionGroupBox);
        layout.Controls.Add(buttonsPanel);

        Controls.Add(layout);
    }

    private void LoadFromSettings()
    {
        var settings = _settingsService.GetSettings();
        _intervalInput.Value = settings.QuizIntervalSeconds;
        _autoCloseInput.Value = settings.QuizConfiguration.AutoCloseAfterCorrectSeconds;
        _optionCountInput.Value = settings.QuizConfiguration.OptionCount;

        switch (settings.QuizConfiguration.Direction)
        {
            case QuizDirection.Direct:
                _directRadio.Checked = true;
                break;
            case QuizDirection.Reverse:
                _reverseRadio.Checked = true;
                break;
            case QuizDirection.Random:
                _randomRadio.Checked = true;
                break;
        }
    }
    
    private void SaveAndClose()
    {
        QuizDirection direction;
        if (_directRadio.Checked)
            direction = QuizDirection.Direct;
        else if (_reverseRadio.Checked)
            direction = QuizDirection.Reverse;
        else
            direction = QuizDirection.Random;

        _settingsService.UpdateSettings(
            (int)_intervalInput.Value,
            (int)_autoCloseInput.Value,
            (int)_optionCountInput.Value,
            direction
        );

        _onSettingsApplied();
        Close();
    }

}