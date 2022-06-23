using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WikiFlight.Common;

namespace WikiFlight.Wikipedia
{
    public class WikipediaService
    {
        private readonly HttpClient httpClient = new HttpClient();

        public WikipediaService()
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "WikiFlight/1.0-dev (https://github.com/Ellecn/WikiFlight)");
        }

        /// <summary>
        /// Gets Wikipedia pages in specified search area
        /// </summary>
        /// <param name="languageCode">Wikipedia language code</param>
        /// <param name="position">Center of search area</param>
        /// <param name="radius">Search radius in meter (between 0 and 10000)</param>
        /// <param name="limit"></param>
        /// <returns>List of Wikipedia pages</returns>
        public async Task<List<WikipediaPage>> GetPages(string languageCode, Position position, int radius, int limit = 50) // TODO: mit limit spielen. Wie viel geht?
        {
            var url = string.Format(
                "https://{0}.wikipedia.org/w/api.php?action=query&format=json&list=geosearch&gscoord={1}|{2}&gsradius={3}&gslimit={4}",
                languageCode,
                position.Latitude.ToString("00.0000", CultureInfo.CreateSpecificCulture("en-GB")),
                position.Longitude.ToString("00.0000", CultureInfo.CreateSpecificCulture("en-GB")),
                radius <= 10000 ? radius : 10000,
                limit);
            Trace.WriteLine("GET " + url);
            var t = httpClient.GetStreamAsync(url);
            var geoSearchResult = await JsonSerializer.DeserializeAsync<GeoSearchResult>(await t);

            var pageInfoList = geoSearchResult.GeoSearchQuery.PageInfoList;
            return pageInfoList.Select(pi => new WikipediaPage(pi.PageId, languageCode, pi.Title, new Position(pi.Latitude, pi.Longitude))).ToList();
        }

        public async Task<string?> GetSummary(WikipediaPage wikipediaPage)
        {
            var url = string.Format(
                    "https://{0}.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro&explaintext&pageids={1}",
                    wikipediaPage.LanguageCode,
                    wikipediaPage.PageId
                    );
            Trace.WriteLine("GET " + url);
            var t = httpClient.GetStreamAsync(url);
            PageResult? pageResult = await JsonSerializer.DeserializeAsync<PageResult>(await t);

            if (pageResult == null || pageResult.PageQuery.Pages[wikipediaPage.PageId].Summary == null)
            {
                return null;
            }

            return pageResult.PageQuery.Pages[wikipediaPage.PageId].Summary.Trim();
        }
    }

    #region GeoSearch

    internal class GeoSearchResult
    {
        //[JsonPropertyName("batchcomplete")]
        //public string BatchComplete { get; set; }

        [JsonPropertyName("query")]
        public GeoSearchQuery GeoSearchQuery { get; set; }
    }

    internal class GeoSearchQuery
    {
        [JsonPropertyName("geosearch")]
        public List<PageInfo> PageInfoList { get; set; }
    }

    internal class PageInfo
    {
        [JsonPropertyName("pageid")]
        public long PageId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }
    }

    #endregion

    #region PageSearch

    internal class PageResult
    {
        //[JsonPropertyName("batchcomplete")]
        //public string BatchComplete { get; set; }

        [JsonPropertyName("query")]
        public PageQuery PageQuery { get; set; }
    }

    internal class PageQuery
    {
        [JsonPropertyName("pages")]
        public Dictionary<long, Page> Pages { get; set; }
    }

    internal class Page
    {
        [JsonPropertyName("pageid")]
        public long PageId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("extract")]
        public string Summary { get; set; }
    }

    #endregion
}
