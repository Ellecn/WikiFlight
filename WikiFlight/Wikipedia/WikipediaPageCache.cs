using System.Collections.Generic;
using System.Linq;
using WikiFlight.Common;

namespace WikiFlight.Wikipedia
{
    public class WikipediaPageCache
    {
        private List<WikipediaPage> pages = new List<WikipediaPage>();

        public void AddNewPagesOnly(List<WikipediaPage> pages)
        {
            var newPages = pages.Where(p => !this.pages.Any(pp => p.PageId == pp.PageId));
            this.pages.AddRange(newPages);
        }

        public List<WikipediaPage> Get(Position currentPosition, int radius)
        {
            var pagesNearby = pages.Where(p => currentPosition.GetDistance(p.Position) <= radius).ToList();
            pagesNearby.ForEach(p => p.Distance = currentPosition.GetDistance(p.Position));
            return pagesNearby.OrderBy(p => p.Distance).ToList();
        }

        public List<WikipediaPage> GetPagesWithoutSummary(Position currentPosition, int radius, int limit = 50)
        {
            return Get(currentPosition, radius).Where(p => string.IsNullOrEmpty(p.Summary)).Take(limit).ToList();
        }

        /// <summary>
        /// Removes all pages that are within radius (meter) of the current position.
        /// </summary>
        public void CleanUp(Position currentPosition, int radius)
        {
            pages.RemoveAll(p => currentPosition.GetDistance(p.Position) <= radius);
        }

        /// <summary>
        /// Removes all pages.
        /// </summary>
        public void Clear()
        {
            pages.Clear();
        }
    }
}
