using System;
using System.Timers;

namespace VocabularyTrainer.Services;

/// <summary>
/// Production implementation of <see cref="ITimer"/> backed by <see cref="System.Timers.Timer"/>.
/// </summary>
internal sealed class SystemTimer : ITimer
{
    private readonly System.Timers.Timer _inner;
    private EventHandler? _elapsed;

    public SystemTimer()
    {
        _inner = new System.Timers.Timer();
        _inner.Elapsed += OnInnerElapsed;
    }

    public double Interval
    {
        get => _inner.Interval;
        set => _inner.Interval = value;
    }

    public bool Enabled => _inner.Enabled;

    public bool AutoReset
    {
        get => _inner.AutoReset;
        set => _inner.AutoReset = value;
    }

    public event EventHandler? Elapsed
    {
        add => _elapsed += value;
        remove => _elapsed -= value;
    }

    public void Start() => _inner.Start();
    public void Stop() => _inner.Stop();

    public void Dispose()
    {
        _inner.Elapsed -= OnInnerElapsed;
        _inner.Dispose();
    }

    private void OnInnerElapsed(object? sender, ElapsedEventArgs e) =>
        _elapsed?.Invoke(this, EventArgs.Empty);
}
