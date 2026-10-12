namespace GIBS.Module.IPGuard
{
    public static class IPGuardSettings
    {
        public const string SettingPrefix = "GIBS.Module.IPGuard.";
        public const string EnabledSettingName = SettingPrefix + "Enabled";
        public const string ThresholdSettingName = SettingPrefix + "Threshold";
        public const string AllowedIpAddressSettingName = SettingPrefix + "AllowedIpAddress";
        public const string DiagnosticModeSettingName = SettingPrefix + "DiagnosticMode";
        public const string ExcludeReservedRoutesSettingName = SettingPrefix + "ExcludeReservedRoutes";
        public const string BlockWindowHoursSettingName = SettingPrefix + "BlockWindowHours";
        public const string LegacyEnabledSettingName = "Enabled";
        public const string LegacyThresholdSettingName = "Threshold";
        public const string LegacyAllowedIpAddressSettingName = "AllowedIpAddress";
        public const bool DefaultEnabled = false;
        public const bool DefaultDiagnosticMode = false;
        public const bool DefaultExcludeReservedRoutes = true;
        public const int DefaultThreshold = 6;
        public const int DefaultBlockWindowHours = 1;
    }
}
