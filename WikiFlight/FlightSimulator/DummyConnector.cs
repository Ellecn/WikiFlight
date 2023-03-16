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

        private Position currentPosition;

        public DummyConnector(FlightSimulatorConnection flightSimulatorConnection) : base(flightSimulatorConnection)
        {
            currentPosition = new Position(54.153131, 13.778811);
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

                currentPosition = new Position(currentPosition.Latitude - 0.001, currentPosition.Longitude - 0.001);

                Trace.WriteLine(string.Format("New position data received: {0}|{1}", currentPosition.Latitude, currentPosition.Longitude));
                flightSimulatorConnection.OnPositionReceived(currentPosition);
            }
        }
    }
}
