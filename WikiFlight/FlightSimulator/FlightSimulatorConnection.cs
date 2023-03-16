namespace WikiFlight.FlightSimulator
{
    /// <summary>
    /// Connection to flight simulators
    /// </summary>
    public abstract class FlightSimulatorConnection
    {
        protected IFlightSimulatorEventListener flightSimulatorEventListener;

        /// <summary>
        /// Creates a new FlightSimulatorConnection.
        /// </summary>
        /// <param name="onConnected">Event handler that is called when a connection to a simulator is established.</param>
        /// <param name="onPositionReceived">Event handler that is called when new position data from a simualtor is received.</param>
        /// <param name="onSimExited">Event handler that is called when the connection to a simulator is lost.</param>
        public FlightSimulatorConnection(IFlightSimulatorEventListener flightSimulatorEventListener)
        {
            this.flightSimulatorEventListener = flightSimulatorEventListener;
        }

        /// <summary>
        /// Indicates wether a connection to a simulator is established.
        /// </summary>
        /// <returns>true if connection is established, otherwise false.</returns>
        public abstract bool IsConnected();

        /// <summary>
        /// Initiates a connection to a simulator. On success <see cref="OnConnected"/> will be called.
        /// </summary>
        public abstract void Connect();

        /// <summary>
        /// Disconnects from a simulator.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Requests current position of the plane. On receiving <see cref="OnPositionReceived"/> is called.
        /// </summary>
        public abstract void RequestCurrentPosition();
    }
}
