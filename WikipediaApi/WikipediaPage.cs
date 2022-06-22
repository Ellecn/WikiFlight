using Common;

namespace WikipediaApi
{
    public class WikipediaPage
    {
        public WikipediaPage(long pageId, string languageCode, string title, Position position)
        {
            PageId = pageId;
            LanguageCode = languageCode;
            Title = title;
            Summary = "";
            Position = position;
        }

        public long PageId { get; }

        public string LanguageCode { get; }

        public string Title { get; }

        public string Summary { get; set; }

        public Position Position { get; }

        public double Distance { get; set; }

        public string URL
        {
            get
            {
                return string.Format("https://{0}.wikipedia.org/wiki/{1}", LanguageCode, Title.Replace(' ', '_'));
            }
        }
    }
}