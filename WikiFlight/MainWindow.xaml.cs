using Common;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using WikipediaApi;

namespace WikiFlight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr handle;
        private const int WM_USER_SIMCONNECT = 0x402;

        private SimConnect? simConnect;

        private readonly DispatcherTimer PositionRefreshTimer = new DispatcherTimer();

        private readonly WikipediaClient wikipediaClient = new WikipediaClient();
        private readonly WikipediaPageCache wikipediaPageCache = new WikipediaPageCache();

        private const int POSITION_REFRESH_INTERVAL_IN_SECONDS = 10;

        private readonly LogWindow logWindow = new LogWindow();

        public MainWindow()
        {
            InitializeComponent();

            Trace.Listeners.Add(new LogTraceListener(logWindow.txtLog));

            SetUi(false);

            PositionRefreshTimer.Interval = TimeSpan.FromSeconds(POSITION_REFRESH_INTERVAL_IN_SECONDS);
            PositionRefreshTimer.Tick += PositionRefreshTimerTick;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            handle = new WindowInteropHelper(this).Handle;

            HwndSource handleSource = HwndSource.FromHwnd(handle);
            handleSource.AddHook(HandleSimConnectEvents);
        }

        private void Connect()
        {
            if (simConnect == null)
            {
                try
                {
                    simConnect = new SimConnect("Managed Data Request", handle, WM_USER_SIMCONNECT, null, 0);

                    simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(OnRecvOpen);
                    simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(OnRecvQuit);
                    simConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(OnRecvException);
                    simConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(OnRecvSimobjectDataBytype);

                    simConnect.AddToDataDefinition(DEFINITIONS.RequestedData, "Title", null, SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simConnect.AddToDataDefinition(DEFINITIONS.RequestedData, "Plane Latitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simConnect.AddToDataDefinition(DEFINITIONS.RequestedData, "Plane Longitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simConnect.RegisterDataDefineStruct<RequestedData>(DEFINITIONS.RequestedData);

                    RequestNewPosition();
                }
                catch (Exception exception)
                {
                    Trace.WriteLine(string.Format("Could not connect to simulator ({0})", exception.Message));
                }
            }
        }

        private void Disconnect()
        {
            PositionRefreshTimer.Stop();
            if (simConnect != null)
            {
                simConnect.Dispose();
                simConnect = null;
            }
            Trace.WriteLine("Disconneted from sim");
            SetUi(false);
        }

        private void RequestNewPosition()
        {
            if (simConnect != null)
            {
                try
                {
                    simConnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_1, DEFINITIONS.RequestedData, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
                    Trace.WriteLine("New position data requested");
                }
                catch (Exception exception)
                {
                    Trace.WriteLine(exception.Message);
                    Disconnect();
                }
            }
        }

        private void PositionRefreshTimerTick(object sender, EventArgs e)
        {
            RequestNewPosition();
        }

        private void SetUi(bool connected)
        {
            btnDisconnect.IsEnabled = connected;
            btnConnect.IsEnabled = !connected;
            if (!connected)
            {
                txtLatitude.Text = "n/a";
                txtLongitude.Text = "n/a";
            }
        }

        private async Task MakeMagic(Position currentPosition)
        {
            if (wikipediaClient.PositionOfLastRequest == null || currentPosition.GetDistance(wikipediaClient.PositionOfLastRequest) > 2000)
            {
                var pagesNearby = await wikipediaClient.GetPagesNearby(currentPosition);
                wikipediaPageCache.Add(pagesNearby);

                var pagesWithoutSummary = wikipediaPageCache.GetPagesWithoutSummary();
                if (pagesWithoutSummary.Count > 0)
                {
                    await wikipediaClient.AddSummary(pagesWithoutSummary);
                }

                DisplayPages(currentPosition);
            }
        }

        private void DisplayPages(Position currentPosition)
        {
            lstPages.Items.Clear();
            var pagesForDisplay = wikipediaPageCache.Get(currentPosition);
            pagesForDisplay.ForEach(p => p.Distance = p.Position.GetDistance(currentPosition));
            pagesForDisplay.ForEach(p => lstPages.Items.Add(p));
        }

        private void OpenSelectedPageInBrowser()
        {
            if (lstPages.SelectedItem is WikipediaPage selectedPage)
            {
                lstPages.SelectedIndex = -1;
                Process.Start("explorer", selectedPage.URL);
            }
        }

        #region GUI event handlers

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Disconnect();
            App.Current.Shutdown();
        }

        private void lstPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OpenSelectedPageInBrowser();
        }

        private void btnOpenLog_Click(object sender, RoutedEventArgs e)
        {
            logWindow.Show();
        }

        #endregion

        #region SimConnect event handlers

        private IntPtr HandleSimConnectEvents(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool isHandled)
        {
            isHandled = false;

            if (message == WM_USER_SIMCONNECT)
            {
                if (simConnect != null)
                {
                    simConnect.ReceiveMessage(); // that invokes the corresponding event handlers
                    isHandled = true;
                }
            }

            return IntPtr.Zero;
        }

        private async void OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (data.dwRequestID == 0)
            {
                RequestedData requestedData = (RequestedData)data.dwData[0];

                Trace.WriteLine(string.Format("New position data received: {0}|{1}", requestedData.latitude, requestedData.longitude));

                Position currentPosition = new Position(requestedData.latitude, requestedData.longitude);

                txtLatitude.Text = currentPosition.Latitude.ToString();
                txtLongitude.Text = currentPosition.Longitude.ToString();

                await MakeMagic(currentPosition);
            }
            //else
            //{
            //    Log("Unknown request ID: " + data.dwRequestID);
            //}
        }

        private void OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            Trace.WriteLine("Exception received: " + data.dwException);
        }

        private void OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Trace.WriteLine("Connected to sim");
            PositionRefreshTimer.Start();
            SetUi(true);
        }

        private void OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Trace.WriteLine("Sim has exited");
            Disconnect();
        }

        #endregion

        #region structs and enums

        private enum DEFINITIONS
        {
            RequestedData
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct RequestedData
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
            public string title;
            public double latitude;
            public double longitude;
        }

        private enum DATA_REQUESTS
        {
            REQUEST_1
        }

        private enum NOTIFICATION_GROUPS
        {
            GROUP0,
        }

        #endregion
    }
}
