using System;
using System.Windows.Forms;

namespace VocabularyTrainer.Infrastructure;

public class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly System.Drawing.Icon? _customIcon;
    private readonly ToolStripMenuItem _pauseItem;
    private readonly ToolStripMenuItem _resumeItem;

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
    
    public void SetPaused(bool paused)
    {
        _pauseItem.Enabled = !paused;
        _resumeItem.Enabled = paused;
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _customIcon?.Dispose();
    }
}