namespace Common
{
    public class WikipediaPage
    {
        public WikipediaPage(long pageId, string title, Position position)
        {
            PageId = pageId;
            Title = title;
            Summary = "";
            Position = position;
        }

        public long PageId { get; }

        public string Title { get; }

        public string Summary { get; set; }

        public Position Position { get; }

        public double Distance { get; set; }

        public string URL
        {
            get
            {
                return string.Format("https://de.wikipedia.org/wiki/{0}", Title.Replace(' ', '_'));
            }
        }
    }
}