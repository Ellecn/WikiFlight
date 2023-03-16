using WikiFlight.Common;

namespace WikiFlight.MSFS2020
{
    /// <summary>
    /// Connector for Flight Simulators
    /// </summary>
    public abstract class FlightSimulatorConnector
    {
        public delegate void ConnectedHandler();
        protected readonly ConnectedHandler OnConnected;

        public delegate void PositionReceivedHandler(Position currentPosition);
        protected readonly PositionReceivedHandler OnPositionReceived;

        public delegate void SimExitedHandler();
        protected readonly SimExitedHandler OnSimExited;

        /// <summary>
        /// Creates a new FlightSimulatorConnector.
        /// </summary>
        /// <param name="onConnected">Event handler that is called when a connection to a simulator is established.</param>
        /// <param name="onPositionReceived">Event handler that is called when new position data from a simualtor is received.</param>
        /// <param name="onSimExited">Event handler that is called when the connection to a simulator is lost.</param>
        public FlightSimulatorConnector(ConnectedHandler onConnected, PositionReceivedHandler onPositionReceived, SimExitedHandler onSimExited)
        {
            OnConnected = onConnected;
            OnPositionReceived = onPositionReceived;
            OnSimExited = onSimExited;
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
