using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;
using Filtration.Models;
using Filtration.Properties;
using Filtration.Views;
using NLog;

namespace Filtration.Services
{
    internal interface IUpdateCheckService
    {
        UpdateData CheckForUpdates();
    }

    internal class UpdateCheckService : IUpdateCheckService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IHTTPService _httpService;
        private const string _updateDataUrl = "http://ben-wallis.github.io/Filtration/filtration_version_dev.xml";

        public UpdateCheckService(IHTTPService httpService)
        {
            _httpService = httpService;
        }

        public UpdateData CheckForUpdates()
        {
            var assemblyVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            try
            {
                var updateData = GetUpdateData();
                updateData.CurrentVersionMajorPart = assemblyVersion.FileMajorPart;
                updateData.CurrentVersionMinorPart = assemblyVersion.FileMinorPart;

                if (updateData.LatestVersionMajorPart >= updateData.CurrentVersionMajorPart &&
                    updateData.LatestVersionMinorPart > updateData.CurrentVersionMinorPart)
                {
                    if (Settings.Default.SuppressUpdates == false || LatestVersionIsNewerThanSuppressedVersion(updateData))
                    {
                        Settings.Default.SuppressUpdates = false;
                        Settings.Default.Save();

                        updateData.UpdateAvailable = true;
                        return updateData;
                    }
                }

                if (updateData.StaticDataUpdatedDate > Settings.Default.StaticDataLastUpdated)
                {
                    var result = MessageBox.Show("New static data files are available (Item Base Types and Item Classes). Do you wish to download them?",
                        "Static Data Update Available", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            UpdateStaticDataFiles();
                            Settings.Default.StaticDataLastUpdated = DateTime.Now;
                            Settings.Default.Save();

                            MessageBox.Show("Static Data successfully updated!", "Update Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            MessageBox.Show($"An error occurred while updating the static data files {Environment.NewLine}{e.Message}", "Update Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }

                updateData.UpdateAvailable = false;
                return updateData;
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                // We don't care if the update check fails, because it could fail for multiple reasons 
                // including the user blocking Filtration in their firewall.
            }

            return null;
        }
        
        private static bool LatestVersionIsNewerThanSuppressedVersion(UpdateData updateData)
        {
            return Settings.Default.SuppressUpdatesUpToVersionMajorPart < updateData.LatestVersionMajorPart ||
                   (Settings.Default.SuppressUpdatesUpToVersionMajorPart <= updateData.LatestVersionMajorPart &&
                    Settings.Default.SuppressUpdatesUpToVersionMinorPart < updateData.LatestVersionMinorPart);
        }

        private UpdateData GetUpdateData()
        {
            var updateXml = _httpService.GetContent(_updateDataUrl);
            return DeserializeUpdateData(updateXml);
        }

        private void UpdateStaticDataFiles()
        {
            var itemClassesStaticData = _httpService.GetContent(Settings.Default.ItemClassesStaticDataUrl);
            var itemBaseTypesStaticData = _httpService.GetContent(Settings.Default.ItemBaseTypesStaticDataUrl);

            var appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Filtration\";
            Directory.CreateDirectory(appdatapath);

            var itemClassesPath = appdatapath + "ItemClasses.txt";
            var itemBaseTypesPath = appdatapath + "ItemBaseTypes.txt";

            File.WriteAllText(itemClassesPath, itemClassesStaticData);
            File.WriteAllText(itemBaseTypesPath, itemBaseTypesStaticData);
        }

        private UpdateData DeserializeUpdateData(string updateDataString)
        {
            var serializer = new XmlSerializer(typeof(UpdateData));
            object result;

            using (TextReader reader = new StringReader(updateDataString))
            {
                result = serializer.Deserialize(reader);
            }

            return result as UpdateData;
        }
    }
}
