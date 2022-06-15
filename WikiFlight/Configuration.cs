using System.IO;
using System.Linq;

namespace WikiFlight
{
    public class Configuration
    {
        #region Preferences

        public const string WIKIPEDIA_LANGUAGE_CODE_DEFAULT = "en";
        public string WikipediaLanguageCode { get; set; } = WIKIPEDIA_LANGUAGE_CODE_DEFAULT;

        #endregion

        private static readonly string CONFIG_FILE = "config.txt";

        private Configuration()
        {
            if (File.Exists(CONFIG_FILE))
            {
                ReadConfig();
            }
            else
            {
                WriteConfig();
            }
        }

        private void ReadConfig()
        {
            File.ReadAllLines(CONFIG_FILE)
                .Select(l => l.Trim())
                .Where(l => !l.StartsWith("#")).ToList()
                .ForEach(l =>
                {
                    string key = l.Split('=')[0];
                    string value = l.Split('=')[1];
                    if (key == "wikipedia-language-code")
                    {
                        WikipediaLanguageCode = value;
                    }
                });
        }

        private void WriteConfig()
        {
            string[] configs = {
                string.Format("wikipedia-language-code={0}", WikipediaLanguageCode)
            };
            File.WriteAllLines(CONFIG_FILE, configs);
        }

        #region Singelton

        private static Configuration INSTANCE;

        public static Configuration GetInstance()
        {
            if (INSTANCE == null)
            {
                INSTANCE = new Configuration();
            }
            return INSTANCE;
        }

        #endregion
    }
}
