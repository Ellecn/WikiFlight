using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
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

        private readonly int MIN_PAGE_DISPLAY_TIME_IN_SECONDS = 20;
        private DateTime timeAtLastDisplayChange = DateTime.MinValue;
        private Position positionAtLastDisplayChange = new Position(0, 0);

        private readonly LogWindow logWindow = new LogWindow();

        private readonly FlightSimulatorConnection flightSimulatorConnection;

        private readonly WikipediaService wikipediaService = new WikipediaService();

        private readonly DispatcherTimer PositionRefreshTimer = new DispatcherTimer();

        private readonly Settings settings = new Settings();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = settings;

            Trace.Listeners.Add(new LogTraceListener(logWindow.txtLog));

            flightSimulatorConnection = new MSFS2020Connection(this);
            //flightSimulatorConnection = new DummySimulatorConnection(this);

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

        private void btnOpenLog_Click(object sender, RoutedEventArgs e)
        {
            logWindow.Show();
        }

        private void PositionRequestTimerTick(object? sender, EventArgs e)
        {
            flightSimulatorConnection.RequestCurrentPosition();
        }

        private void Connect()
        {
            try
            {
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
            SetUi();

            timeAtLastDisplayChange = DateTime.MinValue;
            positionAtLastDisplayChange = new Position(0, 0);
        }

        private void SetUi()
        {
            bool connected = flightSimulatorConnection.IsConnected();

            btnDisconnect.IsEnabled = connected;
            btnConnect.IsEnabled = !connected;
            cmbLanguageCode.IsEnabled = !connected;
            if (!connected)
            {
                txtPosition.Text = "n/a";
                wikipediaPageView.Page = null;
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
            txtPosition.Text = string.Format("{0}, {1}", Math.Round(position.Latitude, 6), Math.Round(position.Longitude, 6));

            if (wikipediaPageView.Page != null)
            {
                wikipediaPageView.Page.Distance = wikipediaPageView.Page.Position.GetDistance(position);
                wikipediaPageView.RefreshUi();
            }

            DateTime now = DateTime.Now;

            if ((now - timeAtLastDisplayChange).TotalSeconds >= MIN_PAGE_DISPLAY_TIME_IN_SECONDS && positionAtLastDisplayChange.GetDistance(position) > 10.0)
            {
                WikipediaPage? nextPage = await wikipediaService.GetNextPage(settings.WikipediaLanguageCode, position, wikipediaPageView.Page);

                if (nextPage == null)
                {
                    wikipediaPageView.Page = null;
                }
                else if (wikipediaPageView.Page == null || nextPage != wikipediaPageView.Page)
                {
                    wikipediaPageView.Page = nextPage;
                    timeAtLastDisplayChange = now;
                    positionAtLastDisplayChange = position;
                }
            }
        }

        public void OnSimExited()
        {
            Disconnect();
        }

        #endregion
    }
}
