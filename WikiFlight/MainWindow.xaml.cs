using Microsoft.FlightSimulator.SimConnect;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
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

        private DispatcherTimer PositionRefreshTimer = new DispatcherTimer();
        private const int POSITION_REFRESH_INTERVAL_IN_SECONDS = 1;

        public MainWindow()
        {
            InitializeComponent();

            SetUi(false);

            PositionRefreshTimer.Interval = TimeSpan.FromSeconds(POSITION_REFRESH_INTERVAL_IN_SECONDS);
            PositionRefreshTimer.Tick += PositionRefreshTimerTick;

            for (int i = 0; i < 10; i++)
            {
                lstPages.Items.Add(new Page
                {
                    Title = "Title " + i,
                    Summary = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet."
                });
            }
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
                }
                catch (Exception exception)
                {
                    Log(string.Format("Could not connect to simulator ({0})", exception.Message));
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
            Log("Disconneted from sim");
            SetUi(false);
        }

        void PositionRefreshTimerTick(object sender, EventArgs e)
        {
            if (simConnect != null)
            {
                try
                {
                    simConnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_1, DEFINITIONS.RequestedData, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
                    Log("New position data requested");
                }
                catch (Exception exception)
                {
                    Log(exception.Message);
                    Disconnect();
                }
            }
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

        private void Log(string message)
        {
            Trace.WriteLine(message);
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

        void Window_Closing(object sender, CancelEventArgs e)
        {
            Disconnect();
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

        private void OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (data.dwRequestID == 0)
            {
                RequestedData requestedData = (RequestedData)data.dwData[0];

                Log(string.Format("New position data received: {0}|{1}", requestedData.latitude, requestedData.longitude));

                txtLatitude.Text = requestedData.latitude.ToString();
                txtLongitude.Text = requestedData.longitude.ToString();
            }
            //else
            //{
            //    Log("Unknown request ID: " + data.dwRequestID);
            //}
        }

        private void OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            Log("Exception received: " + data.dwException);
        }

        private void OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Log("Connected to sim");
            PositionRefreshTimer.Start();
            SetUi(true);
        }

        private void OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Log("Sim has exited");
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
