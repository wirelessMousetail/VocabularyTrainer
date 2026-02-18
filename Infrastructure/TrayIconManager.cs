using System;
using System.Windows.Forms;

namespace VocabularyTrainer.Infrastructure;

/// <summary>
/// Manages the system tray icon and context menu for the application.
/// </summary>
public class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly System.Drawing.Icon? _customIcon;
    private readonly ToolStripMenuItem _pauseItem;
    private readonly ToolStripMenuItem _resumeItem;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrayIconManager"/> class.
    /// </summary>
    /// <param name="onPauseRequested">Action to execute when Pause is selected.</param>
    /// <param name="onResumeRequested">Action to execute when Resume is selected.</param>
    /// <param name="onOptionsRequested">Action to execute when Options is selected.</param>
    /// <param name="onExitRequested">Action to execute when Exit is selected.</param>
    public TrayIconManager(
        Action onPauseRequested,
        Action onResumeRequested,
        Action onOptionsRequested,
        Action onExitRequested)
    {
        var contextMenu = new ContextMenuStrip();

        var pauseItem = new ToolStripMenuItem("Pause");
        pauseItem.Click += (_, _) => onPauseRequested();
        
        var resumeItem = new ToolStripMenuItem("Resume");
        resumeItem.Click += (_, _) => onResumeRequested();
        
        var optionsItem = new ToolStripMenuItem("Options");
        optionsItem.Click += (_, _) => onOptionsRequested();
        
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => onExitRequested();
        
        _pauseItem = pauseItem;
        _resumeItem = resumeItem;

        _resumeItem.Enabled = false;

        contextMenu.Items.Add(pauseItem);
        contextMenu.Items.Add(resumeItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(optionsItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitItem);

        var iconPath = Path.Combine(
            AppContext.BaseDirectory,
            "Resources",
            "tray.ico"
        );
        if (File.Exists(iconPath))
        {
            _customIcon = new System.Drawing.Icon(iconPath);
        }
        
        _notifyIcon = new NotifyIcon
        {
            Icon = _customIcon ?? System.Drawing.SystemIcons.Application,
            Text = "Vocabulary Trainer",
            ContextMenuStrip = contextMenu,
            Visible = true
        };
    }
    
    /// <summary>
    /// Updates the tray menu to reflect the paused/running state.
    /// </summary>
    /// <param name="paused">True if the application is paused, false if running.</param>
    public void SetPaused(bool paused)
    {
        _pauseItem.Enabled = !paused;
        _resumeItem.Enabled = paused;
    }

    /// <summary>
    /// Disposes of the tray icon and custom icon resources.
    /// </summary>
    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _customIcon?.Dispose();
    }
}