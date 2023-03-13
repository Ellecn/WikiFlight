using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WikiFlight.Common;

namespace WikiFlight.Wikipedia
{
    public class WikipediaService
    {
        private readonly WikipediaClient wikipediaClient = new WikipediaClient();
        private readonly WikipediaPageCache wikipediaPageCache = new WikipediaPageCache();

        private Position? positionOfLastPageRequest;
        private DateTime? timeStampOfLastPageRequest;

        public async Task<List<WikipediaPage>> GetPagesNearby(string languageCode, Position position, int radius)
        {
            if (ShouldLoadNewWikipediaPages(position))
            {
                var pages = await wikipediaClient.GetPages(languageCode, position, 10000);
                positionOfLastPageRequest = position;
                timeStampOfLastPageRequest = DateTime.Now;

                wikipediaPageCache.AddNewPagesOnly(pages);
            }

            return wikipediaPageCache.Get(languageCode, position, radius);
        }

        public void Reset()
        {
            positionOfLastPageRequest = null;
            timeStampOfLastPageRequest = null;
        }

        public async Task<string?> GetSummary(WikipediaPage wikipediaPage)
        {
            return await wikipediaClient.GetSummary(wikipediaPage);
        }

        private bool ShouldLoadNewWikipediaPages(Position currentPosition)
        {
            return positionOfLastPageRequest == null
                || (currentPosition.GetDistance(positionOfLastPageRequest) > 1000 && DateTime.Now.Subtract(timeStampOfLastPageRequest.Value) > TimeSpan.FromSeconds(3));
        }
    }
}
