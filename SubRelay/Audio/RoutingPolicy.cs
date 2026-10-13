using System;
using System.Collections.Generic;
using SubRelay.Models;

namespace SubRelay.Audio;

public static class RoutingPolicy
{
    public const string DefaultDeviceMessage = "DefaultDeviceBlocked";

    /// <summary>Claims enabled destinations in display order and returns a host-localized message key.</summary>
    public static string? GetBlockReason(SubwooferSettings subwoofer, string? sourceDeviceId, ISet<string> claimedTargets)
    {
        if (subwoofer is null) throw new ArgumentNullException(nameof(subwoofer));
        if (claimedTargets is null) throw new ArgumentNullException(nameof(claimedTargets));
        if (string.IsNullOrWhiteSpace(subwoofer.DeviceId)) return null;
        if (string.Equals(subwoofer.DeviceId, sourceDeviceId, StringComparison.OrdinalIgnoreCase)) return DefaultDeviceMessage;
        if (subwoofer.Enabled && !claimedTargets.Add(subwoofer.DeviceId!))
            return "DuplicateDeviceBlocked";
        return null;
    }
}
