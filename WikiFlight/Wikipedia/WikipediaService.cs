using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WikiFlight.Common;

namespace WikiFlight.Wikipedia
{
    public class WikipediaService
    {
        private readonly WikipediaClient wikipediaClient = new WikipediaClient();

        /// <summary>
        /// Searches for wikipedia pages within a radius of 5000m and returns the nearest page.
        /// </summary>
        /// <param name="languageCode"></param>
        /// <param name="position"></param>
        /// <param name="currentPage"></param>
        /// <returns></returns>
        public async Task<WikipediaPage?> GetNextPage(string languageCode, Position position, WikipediaPage? currentPage)
        {
            List<WikipediaPage> pages = await wikipediaClient.GetPages(languageCode, position, 5000);
            if(pages.Count == 0)
            {
                return null;
            }
            pages.ForEach(p => p.Distance = position.GetDistance(p.Position));
            WikipediaPage nearestPage = pages.OrderBy(p => p.Distance).First();

            string? summary = await GetSummary(nearestPage);
            if (summary != null)
            {
                nearestPage.Summary = summary;
            }

            return nearestPage;
        }

        public async Task<string?> GetSummary(WikipediaPage wikipediaPage)
        {
            return await wikipediaClient.GetSummary(wikipediaPage);
        }
    }
}
