using System.Collections.Generic;

namespace WikiFlight
{
    internal class Settings
    {
        public string WikipediaLanguageCode { get; set; } = "en";
        public List<WikipediaLanguage> WikipediaLanguageOptions { get; set; } = new List<WikipediaLanguage>()
        {
            new WikipediaLanguage("English (en)", "en"),
            new WikipediaLanguage("French (fr)", "fr"),
            new WikipediaLanguage("German (de)", "de"),
            new WikipediaLanguage("Japanese (ja)", "ja"),
            new WikipediaLanguage("Spanish (es)", "es"),
            new WikipediaLanguage("Russian (ru)", "ru"),
            new WikipediaLanguage("Chinese (zh)", "zh"),
            new WikipediaLanguage("Italian (it)", "it"),
            new WikipediaLanguage("Portuguese (pt)", "pt"),
            new WikipediaLanguage("Persian (fa)", "fa"),
            new WikipediaLanguage("Arabic (ar)", "ar"),
            new WikipediaLanguage("Polish (pl)", "pl"),
            new WikipediaLanguage("Dutch (nl)", "nl"),
            new WikipediaLanguage("Ukrainian (uk)", "uk"),
            new WikipediaLanguage("Hebrew (he)", "he"),
            new WikipediaLanguage("Turkish (tr)", "tr"),
            new WikipediaLanguage("Indonesian (id)", "id"),
            new WikipediaLanguage("Czech (cs)", "cs"),
            new WikipediaLanguage("Vietnamese (vi)", "vi"),
            new WikipediaLanguage("Swedish (sv)", "sv")
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
