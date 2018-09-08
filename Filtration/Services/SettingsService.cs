using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Filtration.Properties;
using NLog;

namespace Filtration.Services
{
    internal interface ISettingsService
    {
        void BackupSettings();
        void RestoreSettings();
    }

    internal class SettingsService : ISettingsService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void BackupSettings()
        {
            var currentUserSettingsFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            var backupFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\..\\last.config";

            try
            {
                File.Copy(currentUserSettingsFile, backupFile, overwrite: true);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void RestoreSettings()
        {
            Settings.Default.Save();
            string currentUserSettingsFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            string backupFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\..\\last.config";

            // Check if we have settings that we need to restore
            if (!File.Exists(backupFile))
            {
                // Nothing we need to do
                return;
            }
            
            try
            {
                // Overwrite the current user settings file with the backed up user settings file
                File.Copy(backupFile, currentUserSettingsFile, overwrite: true);

                // Delete backup file
                File.Delete(backupFile);

                Settings.Default.Reload();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}
