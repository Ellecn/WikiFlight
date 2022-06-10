namespace WikipediaApi
{
    public class Page
    {
        public string Title { get; set; }

        public string Summary { get; set; }

        public string URL
        {
            get
            {
                return String.Format("https://de.wikipedia.org/wiki/{0}", Title.Replace(' ', '_'));
            }
        }
    }
}