using System;

namespace VocabularyTrainer.Services;

/// <summary>
/// Abstraction over System.Timers.Timer to allow testable timer behaviour.
/// </summary>
public interface ITimer : IDisposable
{
    double Interval { get; set; }
    bool Enabled { get; }
    bool AutoReset { get; set; }
    event EventHandler? Elapsed;
    void Start();
    void Stop();
}
