using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WikiFlight.Common;
using WikiFlight.FlightSimulator;
using WikiFlight.Wikipedia;

namespace WikiFlight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IFlightSimulatorEventListener
    {
        private readonly int POSITION_REQUEST_INTERVAL_IN_SECONDS = 1;

        private readonly LogWindow logWindow = new LogWindow();

        private FlightSimulatorConnection flightSimulatorConnection;

        private readonly WikipediaService wikipediaService = new WikipediaService();

        private readonly DispatcherTimer PositionRefreshTimer = new DispatcherTimer();

        private readonly Settings settings = new Settings();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = settings;

            Trace.Listeners.Add(new LogTraceListener(logWindow.txtLog));

            flightSimulatorConnection = new MSFS2020Connection(this);

            SetUi();

            PositionRefreshTimer.Interval = TimeSpan.FromSeconds(POSITION_REQUEST_INTERVAL_IN_SECONDS);
            PositionRefreshTimer.Tick += PositionRequestTimerTick;
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
            flightSimulatorConnection.RequestCurrentPosition();
        }

        private void Connect()
        {
            try
            {
                if (flightSimulatorConnection.GetType() != settings.SimulatorConnectionType)
                {
                    flightSimulatorConnection.Disconnect();
                    flightSimulatorConnection = Activator.CreateInstance(settings.SimulatorConnectionType, this) as FlightSimulatorConnection;
                }
                flightSimulatorConnection.Connect();
            }
            catch (Exception exception)
            {
                Trace.WriteLine(string.Format("Could not connect to simulator ({0})", exception.Message));
            }
        }

        private void Disconnect()
        {
            PositionRefreshTimer.Stop();
            flightSimulatorConnection.Disconnect();
            wikipediaService.Reset();
            lstPages.Items.Clear();
            flightMap.clearMarkersAndCircle();
            SetUi();
        }

        private void SetUi()
        {
            bool connected = flightSimulatorConnection.IsConnected();

            btnDisconnect.IsEnabled = connected;
            btnConnect.IsEnabled = !connected;
            cmbSimulator.IsEnabled = !connected;
            cmbLanguageCode.IsEnabled = !connected;
            cmbRadius.IsEnabled = !connected;
            if (!connected)
            {
                txtLatitude.Text = "n/a";
                txtLongitude.Text = "n/a";
            }
        }

        #region IFlightSimulatorEventListener implementation

        public void OnConnected()
        {
            PositionRefreshTimer.Start();
            SetUi();
        }

        public async void OnPositionReceived(Position position)
        {
            txtLatitude.Text = position.Latitude.ToString();
            txtLongitude.Text = position.Longitude.ToString();

            var pagesNearby = await wikipediaService.GetPagesNearby(settings.WikipediaLanguageCode, position, settings.SearchRadiusInMeter);

            if (tabList.IsSelected)
            {
                if (!AreTheSame(pagesNearby, lstPages.Items))
                {
                    lstPages.Items.Clear();
                    pagesNearby.ForEach(p => lstPages.Items.Add(p));
                }
            }

            if (tabMap.IsSelected)
            {
                flightMap.setCircle(position, settings.SearchRadiusInMeter);
                flightMap.addNewMarkers(pagesNearby);
            }
        }

        public void OnSimExited()
        {
            Disconnect();
        }

        #endregion

        private static bool AreTheSame(List<WikipediaPage> list, ItemCollection itemCollection)
        {
            return list.Count == itemCollection.Count && list.TrueForAll(p => itemCollection.Contains(p));
        }
    }
}
