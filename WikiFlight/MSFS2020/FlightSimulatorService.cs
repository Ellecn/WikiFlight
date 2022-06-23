using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using WikiFlight.Common;

namespace WikiFlight.MSFS2020
{
    /// <summary>
    /// Service that connects to and reads values from Microsoft Flight Simulator 2020.
    /// </summary>
    public class FlightSimulatorService
    {
        private const int WM_USER_SIMCONNECT = 0x402;
        private IntPtr windowHandle;
        private SimConnect? simConnect;

        public delegate void ConnectedHandler();
        private readonly ConnectedHandler OnConnected;

        public delegate void PositionReceivedHandler(Position currentPosition);
        private readonly PositionReceivedHandler OnPositionReceived;

        public delegate void SimExitedHandler();
        private readonly SimExitedHandler OnSimExited;

        /// <summary>
        /// Creates a new FlightSimulatorService.
        /// </summary>
        /// <param name="onConnected">Event handler that is called when a connection to the simulator is established.</param>
        /// <param name="onPositionReceived">Event handler that is called when new position data from the simualtor is received.</param>
        /// <param name="onSimExited">Event handler that is called when the connection to the simulator is lost.</param>
        public FlightSimulatorService(ConnectedHandler onConnected, PositionReceivedHandler onPositionReceived, SimExitedHandler onSimExited)
        {
            OnConnected = onConnected;
            OnPositionReceived = onPositionReceived;
            OnSimExited = onSimExited;
        }

        /// <summary>
        /// Initializes the FlightSimulatorService. Important for receiving simulator events.
        /// </summary>
        /// <param name="window">The WPF main window.</param>
        public void Init(Window window)
        {
            windowHandle = new WindowInteropHelper(window).Handle;

            HwndSource handleSource = HwndSource.FromHwnd(windowHandle);
            handleSource.AddHook(HandleSimConnectEvents);
        }

        /// <summary>
        /// Indicates wether a connection to the simulator is established.
        /// </summary>
        /// <returns>true if connection is established, otherwise false.</returns>
        public bool IsConnected()
        {
            return simConnect != null;
        }

        /// <summary>
        /// Initiates a connection to the simulator. On success <see cref="OnConnected"/> will be called.
        /// </summary>
        public void Connect()
        {
            if (!IsConnected())
            {
                Trace.WriteLine("Connect to sim...");

                simConnect = new SimConnect("Managed Data Request", windowHandle, WM_USER_SIMCONNECT, null, 0);

                simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(OnRecvOpen);
                simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(OnRecvQuit);
                simConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(OnRecvException);
                simConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(OnRecvSimobjectDataBytype);

                simConnect.AddToDataDefinition(DEFINITIONS.RequestedData, "Title", null, SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DEFINITIONS.RequestedData, "Plane Latitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.AddToDataDefinition(DEFINITIONS.RequestedData, "Plane Longitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simConnect.RegisterDataDefineStruct<RequestedData>(DEFINITIONS.RequestedData);
            }
            else
            {
                Trace.WriteLine("Already connected to sim");
            }
        }

        /// <summary>
        /// Disconnects from the simulator.
        /// </summary>
        public void Disconnect()
        {
            if (IsConnected())
            {
                simConnect.Dispose();
                simConnect = null;
            }
            Trace.WriteLine("Disconnected from sim");
        }

        /// <summary>
        /// Requests current position of the plane. On receiving <see cref="OnPositionReceived"/> is called.
        /// </summary>
        public void RequestCurrentPosition()
        {
            if (IsConnected())
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

        private IntPtr HandleSimConnectEvents(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool isHandled)
        {
            isHandled = false;

            if (message == WM_USER_SIMCONNECT && IsConnected())
            {
                Trace.WriteLine("Event from sim received");
                simConnect.ReceiveMessage(); // that invokes the corresponding event handlers
                isHandled = true;
            }

            return IntPtr.Zero;
        }

        private void OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Trace.WriteLine("Connected to sim");
            OnConnected();
        }

        private void OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (data.dwRequestID == 0)
            {
                RequestedData requestedData = (RequestedData)data.dwData[0];

                Trace.WriteLine(string.Format("New position data received: {0}|{1}", requestedData.latitude, requestedData.longitude));

                OnPositionReceived(new Position(requestedData.latitude, requestedData.longitude));
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

        private void OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Trace.WriteLine("Sim has exited");
            OnSimExited();
        }

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
