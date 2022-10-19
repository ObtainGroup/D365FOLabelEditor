using System.Configuration;

namespace LabelEditor
{
    /// <summary>
    /// Handles app settings. 
    /// Should probably be set up as a singleton and only load and save once.
    /// </summary>
    public static class Settings
    {
        private const string lastOpenedFilename = "LastOpenedFilename";
        private const string windowSize = "WindowSize";
        private const string windowPosition = "WindowPosition";
        private const string translationEndpoint = "TranslationEndpoint";
        private const string translationKey = "TranslationKey";
        private const string translationRegion = "TranslationRegion";

        public static string TranslationEndpoint => ConfigurationManager.AppSettings[translationEndpoint];
        public static string TranslationKey => ConfigurationManager.AppSettings[translationKey];
        public static string TranslationRegion => ConfigurationManager.AppSettings[translationRegion];

        public static string Filename
        {
            get => ConfigurationManager.AppSettings[lastOpenedFilename];
            set => SaveKey(lastOpenedFilename, value);
        }

        public static string WindowSize
        {
            get => ConfigurationManager.AppSettings[windowSize];
            set => SaveKey(windowSize, value);
        }

        public static string WindowPosition
        {
            get => ConfigurationManager.AppSettings[windowPosition];
            set => SaveKey(windowPosition, value);
        }

        private static void SaveKey(string key, string value)
        {
            try
            {
                Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                KeyValueConfigurationCollection settings = configFile.AppSettings.Settings;
                
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;    
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {

            }
        }
    }
}
