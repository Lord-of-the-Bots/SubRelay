using System;
using System.Collections.Generic;
using SubRelay.Models;

namespace SubRelay.Audio;

public sealed record AudioDeviceInfo(string Id, string Name, int ChannelCount = 0, int ChannelMask = 0);
public sealed record SubwooferStatus(Guid Id, string Message, bool IsBlocked, bool IsRunning);
public sealed record AudioEngineSnapshot(
    string? SourceDeviceId,
    string SourceDeviceName,
    IReadOnlyList<AudioDeviceInfo> Devices,
    IReadOnlyList<SubwooferStatus> Subwoofers,
    string? Error = null);

public interface IAudioEngine : IDisposable
{
    event Action<AudioEngineSnapshot>? SnapshotChanged;
    AudioEngineSnapshot Snapshot { get; }
    void Configure(IReadOnlyList<SubwooferSettings> subwoofers, bool isPaused = false);
    void Refresh();
}
