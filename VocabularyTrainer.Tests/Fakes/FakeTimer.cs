using System;
using VocabularyTrainer.Services;

namespace VocabularyTrainer.Tests.Fakes;

internal sealed class FakeTimer : VocabularyTrainer.Services.ITimer
{
    public double Interval { get; set; } = 60_000;
    public bool Enabled { get; private set; }
    public bool AutoReset { get; set; }
    public event EventHandler? Elapsed;

    public void Start() => Enabled = true;
    public void Stop() => Enabled = false;
    public void Fire() => Elapsed?.Invoke(this, EventArgs.Empty);
    public void Dispose() { }
}
