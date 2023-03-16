using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using WikiFlight.Common;

namespace WikiFlight.FlightSimulator
{
    /// <summary>
    /// Connection to the MS Flight Simulator 2020
    /// </summary>
    public class MSFS2020Connection : FlightSimulatorConnection
    {
        private const int WM_USER_SIMCONNECT = 0x402;
        private IntPtr windowHandle;
        private SimConnect? simConnect;

        public MSFS2020Connection(IFlightSimulatorEventListener flightSimulatorEventListener) : base(flightSimulatorEventListener)
        {
            windowHandle = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();

            HwndSource handleSource = HwndSource.FromHwnd(windowHandle);
            handleSource.AddHook(HandleSimConnectEvents);
        }

        #region Interface

        public override bool IsConnected()
        {
            return simConnect != null;
        }

        public override void Connect()
        {
            if (!IsConnected())
            {
                Trace.WriteLine("Connect to MS Flight Simualtor 2020...");

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

        public override void Disconnect()
        {
            if (IsConnected())
            {
                simConnect.Dispose();
                simConnect = null;
                Trace.WriteLine("Disconnected from sim");
            }
        }

        public override void RequestCurrentPosition()
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

        #endregion

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
            flightSimulatorEventListener.OnConnected();
        }

        private void OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (data.dwRequestID == 0)
            {
                RequestedData requestedData = (RequestedData)data.dwData[0];

                Trace.WriteLine(string.Format("New position data received: {0}|{1}", requestedData.latitude, requestedData.longitude));

                flightSimulatorEventListener.OnPositionReceived(new Position(requestedData.latitude, requestedData.longitude));
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
            flightSimulatorEventListener.OnSimExited();
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
