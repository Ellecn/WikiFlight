using WikiFlight.Common;

namespace WikiFlight.FlightSimulator
{
    public interface IFlightSimulatorEventListener
    {
        public void OnConnected();

        public void OnPositionReceived(Position position);

        public void OnSimExited();
    }
}
