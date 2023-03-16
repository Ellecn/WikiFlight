using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using WikiFlight.Common;
using WikiFlight.Wikipedia;

namespace WikiFlight.Controls.FlightMap
{
    public class FlightMap : WebView2
    {
        private readonly List<WikipediaPage> markers = new List<WikipediaPage>();

        public FlightMap()
        {
            CoreWebView2InitializationCompleted += CoreWebView2_InitializationCompleted;

            Source = new Uri(Path.Combine(Environment.CurrentDirectory, "Controls/FlightMap/map.html"));
        }

        private void CoreWebView2_InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            //CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        }

        private void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            Process.Start("explorer", string.Format("\"{0}\"", e.Uri));
        }

        /*public async void flyTo(Position position)
        {
            string functionCall = string.Format("flyTo({0}, {1});",
                position.Latitude.ToString("00.0000", CultureInfo.CreateSpecificCulture("en-GB")),
                position.Longitude.ToString("00.0000", CultureInfo.CreateSpecificCulture("en-GB")));
            await webView.ExecuteScriptAsync(functionCall);
        }*/

        private void addMarker(WikipediaPage page)
        {
            string functionCall = string.Format("addMarker('{0}', '{1}', {2}, {3});",
                page.Title,
                page.URL,
                page.Position.Latitude.ToString("00.0000", CultureInfo.CreateSpecificCulture("en-GB")),
                page.Position.Longitude.ToString("00.0000", CultureInfo.CreateSpecificCulture("en-GB")));
            ExecuteScriptAsync(functionCall);
        }

        public void addNewMarkers(List<WikipediaPage> pages)
        {
            foreach (WikipediaPage p in pages)
            {
                if (!markers.Contains(p))
                {
                    addMarker(p);
                    markers.Add(p);
                }
            }
        }

        public void clearMarkersAndCircle()
        {
            markers.Clear();
            ExecuteScriptAsync("clearMarkers();");
            ExecuteScriptAsync("clearCircle();");
        }

        public void setCircle(Position position, int radius)
        {
            string functionCall = string.Format("setCircle({0}, {1}, {2});",
                position.Latitude.ToString("00.0000", CultureInfo.CreateSpecificCulture("en-GB")),
                position.Longitude.ToString("00.0000", CultureInfo.CreateSpecificCulture("en-GB")),
                radius);
            ExecuteScriptAsync(functionCall);
        }
    }
}
