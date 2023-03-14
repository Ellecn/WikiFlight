using System.Collections.Generic;
using System.Linq;
using WikiFlight.Common;

namespace WikiFlight.Wikipedia
{
    public class WikipediaPageCache
    {
        private readonly List<WikipediaPage> pages = new List<WikipediaPage>();

        private IEnumerable<WikipediaPage> GetNewPagesOnly(List<WikipediaPage> pages)
        {
            return pages.Where(p => !this.pages.Any(pp => p.PageId == pp.PageId && p.LanguageCode.Equals(pp.LanguageCode)));
        }

        public void AddNewPagesOnly(List<WikipediaPage> pages)
        {
            this.pages.AddRange(GetNewPagesOnly(pages));
        }

        public List<WikipediaPage> Get(string languageCode, Position currentPosition, int radius)
        {
            var pagesNearby = pages.Where(p => p.LanguageCode.Equals(languageCode) && currentPosition.GetDistance(p.Position) <= radius).ToList();
            pagesNearby.ForEach(p => p.Distance = currentPosition.GetDistance(p.Position));
            return pagesNearby.OrderBy(p => p.Distance).ToList();
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
