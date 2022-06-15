using Common;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WikipediaApi
{
    public class WikipediaClient
    {
        public Position? PositionOfLastRequest { get; set; }

        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<List<WikipediaPage>> GetPagesNearby(string languageCode, Position position, int radius = 5000, int limit = 50)
        {
            var url = string.Format(
                "https://{0}.wikipedia.org/w/api.php?action=query&format=json&list=geosearch&gscoord={1}|{2}&gsradius={3}&gslimit={4}",
                languageCode,
                position.Latitude.ToString("00.0000", CultureInfo.CreateSpecificCulture("en-GB")),
                position.Longitude.ToString("00.0000", CultureInfo.CreateSpecificCulture("en-GB")),
                radius,
                limit);
            Trace.WriteLine("GET " + url);
            var t = httpClient.GetStreamAsync(url);
            var geoSearchResult = await JsonSerializer.DeserializeAsync<GeoSearchResult>(await t);

            PositionOfLastRequest = position;

            var pageInfoList = geoSearchResult.GeoSearchQuery.PageInfoList;
            return pageInfoList.Select(pi => new WikipediaPage(pi.PageId, languageCode, pi.Title, new Position(pi.Latitude, pi.Longitude))).ToList();
        }

        public async Task<List<WikipediaPage>> AddSummary(List<WikipediaPage> pages, string languageCode)
        {
            if (pages.Count == 0)
            {
                return pages;
            }

            PageResult? pageResult = null;
            do
            {
                var url = string.Format(
                    "https://{0}.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro&explaintext&pageids={1}{2}",
                    languageCode,
                    string.Join("|", pages.Take(50).Select(pi => pi.PageId).ToList()),
                    pageResult == null ? "" : (pageResult.Continue == null ? "" : string.Format("&excontinue={0}&continue={1}", pageResult.Continue.ExcontinueParam, pageResult.Continue.ContinueParam)));
                Trace.WriteLine("GET " + url);
                var t = httpClient.GetStreamAsync(url);
                pageResult = await JsonSerializer.DeserializeAsync<PageResult>(await t);

                pages.Take(50).ToList().ForEach(p =>
                {
                    if (pageResult.PageQuery.Pages[p.PageId].Summary != null)
                    {
                        p.Summary = pageResult.PageQuery.Pages[p.PageId].Summary;
                    }
                });
            } while (pageResult.Continue != null);

            return pages;
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

        [JsonPropertyName("continue")]
        public Continue Continue { get; set; }

        [JsonPropertyName("query")]
        public PageQuery PageQuery { get; set; }
    }

    internal class Continue
    {
        [JsonPropertyName("excontinue")]
        public int ExcontinueParam { get; set; }

        [JsonPropertyName("continue")]
        public string ContinueParam { get; set; }
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
