using Common;
using System.Collections.Generic;
using System.Linq;

namespace WikiFlight
{
    class WikipediaPageCache
    {
        private List<WikipediaPage> pages = new List<WikipediaPage>();

        public void Add(List<WikipediaPage> pages)
        {
            var newPages = pages.Where(p => !this.pages.Any(pp => p.PageId == pp.PageId));
            this.pages.AddRange(newPages);
        }

        public List<WikipediaPage> Get(Position currentPosition, int radius = 5000)
        {
            return pages.Where(p => currentPosition.GetDistance(p.Position) <= radius).OrderBy(p => p.Distance).ToList();
        }

        public List<WikipediaPage> GetPagesWithoutSummary()
        {
            return pages.Where(p => string.IsNullOrEmpty(p.Summary)).ToList();
        }

        public void Clear()
        {
            pages.Clear();
        }
    }
}
