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

        public DummyConnector(FlightSimulatorConnection flightSimulatorConnection) : base(flightSimulatorConnection)
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
                Trace.WriteLine("Connect to dummy simulator...");
                connected = true;
                flightSimulatorConnection.OnConnected();
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
                Trace.WriteLine(string.Format("New position data received: {0}|{1}", 54.153131, 13.778811));
                flightSimulatorConnection.OnPositionReceived(new Position(54.153131, 13.778811));
            }
        }
    }
}
