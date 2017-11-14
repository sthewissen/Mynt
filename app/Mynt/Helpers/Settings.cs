// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Mynt.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string CurrentUniqueIdKey = "CurrentUniqueId_Key";
        private static readonly string CurrentUniqueIdDefault = string.Empty;

        private const string DeviceTokenKey = "DeviceToken_Key";
        private static readonly string DeviceTokenDefault = string.Empty;

        private const string IsDeviceRemoteRegisteredKey = "IsDeviceRemoteRegistered_Key";
        private static readonly bool IsDeviceRemoteRegisteredDefault = false;

        private const string FunctionRootKey = "FunctionRoot_Key";
        private static readonly string FunctionRootDefault = string.Empty;

        #endregion


        public static string CurrentUniqueId
        {
            get
            {
                return AppSettings.GetValueOrDefault(CurrentUniqueIdKey, CurrentUniqueIdDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(CurrentUniqueIdKey, value);
            }
        }

        public static string DeviceToken
        {
            get
            {
                return AppSettings.GetValueOrDefault(DeviceTokenKey, DeviceTokenDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(DeviceTokenKey, value);
            }
        }

        public static string FunctionRoot
        {
            get
            {
                return AppSettings.GetValueOrDefault(FunctionRootKey, FunctionRootDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(FunctionRootKey, value);
            }
        }

        public static bool IsDeviceRemoteRegistered
        {
            get
            {
                return AppSettings.GetValueOrDefault(IsDeviceRemoteRegisteredKey, IsDeviceRemoteRegisteredDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IsDeviceRemoteRegisteredKey, value);
            }
        }

    }
}