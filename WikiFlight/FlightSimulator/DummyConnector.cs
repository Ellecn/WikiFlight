using System.Diagnostics;
using WikiFlight.Common;
using WikiFlight.MSFS2020;

namespace WikiFlight.FlightSimulator
{
    /// <summary>
    /// Connector for a dummy simulator
    /// </summary>
    public class DummyConnector : FlightSimulatorConnector
    {
        private bool connected = false;

        public DummyConnector(ConnectedHandler onConnected, PositionReceivedHandler onPositionReceived, SimExitedHandler onSimExited) : base(onConnected, onPositionReceived, onSimExited)
        {
        }

        public override bool IsConnected()
        {
            return connected;
        }

        public override void Connect()
        {
            if (!IsConnected())
            {
                Trace.WriteLine("Connect to sim...");
                connected = true;
                OnConnected();
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
                connected = false;
            }
            Trace.WriteLine("Disconnected from sim");
        }

        public override void RequestCurrentPosition()
        {
            if (IsConnected())
            {
                Trace.WriteLine("New position data requested");
                OnPositionReceived(new Position(54.153131, 13.778811));
            }
        }
    }
}
