using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WikiFlight.Common;
using WikiFlight.MSFS2020;
using WikiFlight.Wikipedia;

namespace WikiFlight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int POSITION_REQUEST_INTERVAL_IN_SECONDS = 1;

        private readonly LogWindow logWindow = new LogWindow();

        private readonly FlightSimulatorService flightSimulatorService;
        private readonly WikipediaService wikipediaService = new WikipediaService();

        private readonly WikipediaPageCache wikipediaPageCache = new WikipediaPageCache();

        private readonly DispatcherTimer PositionRefreshTimer = new DispatcherTimer();

        private readonly Settings settings = new Settings();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = settings;

            Trace.Listeners.Add(new LogTraceListener(logWindow.txtLog));

            flightSimulatorService = new FlightSimulatorService(OnConnected, OnPositionReceived, OnSimExited);

            SetUi();

            PositionRefreshTimer.Interval = TimeSpan.FromSeconds(POSITION_REQUEST_INTERVAL_IN_SECONDS);
            PositionRefreshTimer.Tick += PositionRequestTimerTick;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            flightSimulatorService.Init(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Disconnect();
            Application.Current.Shutdown();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void lstPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstPages.SelectedItem is WikipediaPage selectedPage)
            {
                lstPages.SelectedIndex = -1;
                Process.Start("explorer", string.Format("\"{0}\"", selectedPage.URL));
            }
        }

        private void btnOpenLog_Click(object sender, RoutedEventArgs e)
        {
            logWindow.Show();
        }

        private async void ListViewExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander expander = (Expander)sender;
            WikipediaPage wikipediaPage = (WikipediaPage)expander.DataContext;
            if (string.IsNullOrEmpty(wikipediaPage.Summary))
            {
                string? summary = await wikipediaService.GetSummary(wikipediaPage);
                wikipediaPage.Summary = summary ?? "";
            }
        }

        private void PositionRequestTimerTick(object? sender, EventArgs e)
        {
            flightSimulatorService.RequestCurrentPosition();
        }

        private void Connect()
        {
            try
            {
                flightSimulatorService.Connect();
                flightSimulatorService.RequestCurrentPosition();
            }
            catch (Exception exception)
            {
                Trace.WriteLine(string.Format("Could not connect to simulator ({0})", exception.Message));
            }
        }

        private void Disconnect()
        {
            PositionRefreshTimer.Stop();
            flightSimulatorService.Disconnect();
            wikipediaService.Reset();
            lstPages.Items.Clear();
            SetUi();
        }

        private void SetUi()
        {
            bool connected = flightSimulatorService.IsConnected();

            btnDisconnect.IsEnabled = connected;
            btnConnect.IsEnabled = !connected;
            cmbLanguageCode.IsEnabled = !connected;
            cmbRadius.IsEnabled = !connected;
            if (!connected)
            {
                txtLatitude.Text = "n/a";
                txtLongitude.Text = "n/a";
            }
        }

        #region FlightSimulatorClient event handlers

        private void OnConnected()
        {
            PositionRefreshTimer.Start();
            SetUi();
        }

        private async void OnPositionReceived(Position currentPosition)
        {
            txtLatitude.Text = currentPosition.Latitude.ToString();
            txtLongitude.Text = currentPosition.Longitude.ToString();

            if (ShouldLoadNewWikipediaPages(currentPosition))
            {
                var pages = await wikipediaService.GetPages(settings.WikipediaLanguageCode, currentPosition, 10000);
                wikipediaPageCache.AddNewPagesOnly(pages);
            }

            var pagesNearby = wikipediaPageCache.Get(settings.WikipediaLanguageCode, currentPosition, settings.SearchRadiusInMeter);
            if (!AreTheSame(pagesNearby, lstPages.Items))
            {
                lstPages.Items.Clear();
                pagesNearby.ForEach(p => lstPages.Items.Add(p));
            }
        }

        private bool ShouldLoadNewWikipediaPages(Position currentPosition)
        {
            return wikipediaService.GetPositionOfLastPageRequest() == null
                || (currentPosition.GetDistance(wikipediaService.GetPositionOfLastPageRequest()) > 1000 && DateTime.Now.Subtract(wikipediaService.GetTimeStampOfLastPageRequest().Value) > TimeSpan.FromSeconds(3));
        }

        private static bool AreTheSame(List<WikipediaPage> list, ItemCollection itemCollection)
        {
            return list.Count == itemCollection.Count && list.TrueForAll(p => itemCollection.Contains(p));
        }

        private void OnSimExited()
        {
            Disconnect();
        }

        #endregion
    }
}
