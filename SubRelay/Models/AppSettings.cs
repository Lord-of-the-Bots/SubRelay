using System;
using System.Collections.Generic;

namespace SubRelay.Models;

public sealed class AppSettings
{
    public bool CloseToTray { get; set; } = true;
    public bool StartWithWindows { get; set; } = true;
    public bool IsPaused { get; set; }
    public string Language { get; set; } = "system";
    public List<SubwooferSettings> Subwoofers { get; set; } = [new()];
}

public sealed class SubwooferSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public double LowCutHz { get; set; } = 0;
    public double HighCutHz { get; set; } = 80;
    public double Gain { get; set; } = 0.5;
    public bool Enabled { get; set; } = true;
    public bool CustomChannelMapping { get; set; }
    public List<ChannelMappingSettings> ChannelMappings { get; set; } = [];

    public SubwooferSettings Copy()
    {
        var copy = (SubwooferSettings)MemberwiseClone();
        copy.ChannelMappings = new List<ChannelMappingSettings>();
        if (ChannelMappings is not null)
            foreach (ChannelMappingSettings mapping in ChannelMappings)
                if (mapping is not null) copy.ChannelMappings.Add(mapping.Copy());
        return copy;
    }
}

public sealed class ChannelMappingSettings
{
    // SpeakerMask is a single WAVEFORMATEXTENSIBLE speaker bit, or zero when the
    // endpoint exposes numbered channels without their speaker positions.
    public int ChannelIndex { get; set; }
    public int SpeakerMask { get; set; }
    public bool Enabled { get; set; }
    public double LowCutHz { get; set; } = 0;
    public double HighCutHz { get; set; } = 80;

    public ChannelMappingSettings Copy() => (ChannelMappingSettings)MemberwiseClone();
}
