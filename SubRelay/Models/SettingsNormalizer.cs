using System;
using System.Collections.Generic;

namespace SubRelay.Models;

public static class SettingsNormalizer
{
    public static void Normalize(AppSettings settings)
    {
        if (settings is null) throw new ArgumentNullException(nameof(settings));
        settings.Subwoofers ??= new List<SubwooferSettings>();
        settings.Subwoofers.RemoveAll(s => s is null);
        var ids = new HashSet<Guid>();
        foreach (var subwoofer in settings.Subwoofers)
        {
            if (subwoofer.Id == Guid.Empty || !ids.Add(subwoofer.Id))
            {
                subwoofer.Id = Guid.NewGuid();
                ids.Add(subwoofer.Id);
            }
            if (!FrequencyRange.IsValidBand(subwoofer.LowCutHz, subwoofer.HighCutHz))
            {
                subwoofer.LowCutHz = 0;
                subwoofer.HighCutHz = 80;
            }
            if (!subwoofer.Enabled) { subwoofer.Gain = 0; subwoofer.Enabled = true; }
            subwoofer.Gain = double.IsNaN(subwoofer.Gain) || double.IsInfinity(subwoofer.Gain)
                ? 0.5 : Math.Max(0, Math.Min(1, subwoofer.Gain));
            if (string.IsNullOrWhiteSpace(subwoofer.DeviceId)) subwoofer.DeviceId = null;
            subwoofer.ChannelMappings ??= new List<ChannelMappingSettings>();
            var speakers = new HashSet<int>();
            var numberedChannels = new HashSet<int>();
            subwoofer.ChannelMappings.RemoveAll(mapping => mapping is null ||
                mapping.ChannelIndex is < 0 or >= 32 ||
                (mapping.SpeakerMask != 0 && (mapping.SpeakerMask < 0 ||
                    (mapping.SpeakerMask & ~0x3ffff) != 0 ||
                    (mapping.SpeakerMask & (mapping.SpeakerMask - 1)) != 0)) ||
                !(mapping.SpeakerMask == 0 ? numberedChannels.Add(mapping.ChannelIndex) : speakers.Add(mapping.SpeakerMask)));
            if (subwoofer.ChannelMappings.Count > 32)
                subwoofer.ChannelMappings.RemoveRange(32, subwoofer.ChannelMappings.Count - 32);
            foreach (ChannelMappingSettings mapping in subwoofer.ChannelMappings)
            {
                if (!FrequencyRange.IsValidBand(mapping.LowCutHz, mapping.HighCutHz))
                {
                    mapping.LowCutHz = subwoofer.LowCutHz;
                    mapping.HighCutHz = subwoofer.HighCutHz;
                }
            }
        }
    }
}
