using System.Collections.Generic;

namespace WikiFlight
{
    internal class Settings
    {
        public int SearchRadiusInMeter { get; set; } = 3000;
        public List<int> SearchRadiusInMeterOptions { get; set; } = new List<int>() { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000 };

        public string WikipediaLanguageCode { get; set; } = "en";
        public List<WikipediaLanguage> WikipediaLanguageOptions { get; set; } = new List<WikipediaLanguage>()
        {
            new WikipediaLanguage("English", "en"),
            new WikipediaLanguage("French", "fr"),
            new WikipediaLanguage("German", "de"),
            new WikipediaLanguage("Japanese", "ja"),
            new WikipediaLanguage("Spanish", "es"),
            new WikipediaLanguage("Russian", "ru"),
            new WikipediaLanguage("Chinese", "zh"),
            new WikipediaLanguage("Italian", "it"),
            new WikipediaLanguage("Portuguese", "pt"),
            new WikipediaLanguage("Persian", "fa"),
            new WikipediaLanguage("Arabic", "ar"),
            new WikipediaLanguage("Polish", "pl"),
            new WikipediaLanguage("Dutch", "nl"),
            new WikipediaLanguage("Ukrainian", "uk"),
            new WikipediaLanguage("Hebrew", "he"),
            new WikipediaLanguage("Turkish", "tr"),
            new WikipediaLanguage("Indonesian", "id"),
            new WikipediaLanguage("Czech", "cs"),
            new WikipediaLanguage("Vietnamese", "vi"),
            new WikipediaLanguage("Swedish", "sv")
            // TODO: mehr
        };

        internal class WikipediaLanguage
        {
            public WikipediaLanguage(string name, string code)
            {
                Name = name;
                Code = code;
            }

            public string Name { get; }
            public string Code { get; }
        }
    }
}
