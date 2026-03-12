namespace Wrath.Domain;

public sealed record VaultPolicy(bool RequireWindowsVault, bool AllowPasswordStorage, bool RequireHardwareBackedKeys)
{
    public static VaultPolicy Default => new(requireWindowsVault: true, allowPasswordStorage: false, requireHardwareBackedKeys: false);
}
