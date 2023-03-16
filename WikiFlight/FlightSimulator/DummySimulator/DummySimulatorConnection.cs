using System.Diagnostics;
using WikiFlight.Common;

namespace WikiFlight.FlightSimulator
{
    /// <summary>
    /// Connection for a dummy simulator
    /// </summary>
    public class DummySimulatorConnection : FlightSimulatorConnection
    {
        private bool connected = false;

        private Position currentPosition;

        public DummySimulatorConnection(IFlightSimulatorEventListener flightSimulatorEventListener) : base(flightSimulatorEventListener)
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
                flightSimulatorEventListener.OnConnected();
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
                flightSimulatorEventListener.OnPositionReceived(currentPosition);
            }
        }
    }
}
