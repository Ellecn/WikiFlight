using System;
using WikiFlight.Common;
using WikiFlight.FlightSimulator;

namespace WikiFlight.MSFS2020
{
    /// <summary>
    /// Connector for Flight Simulators
    /// </summary>
    public abstract class FlightSimulatorConnector
    {
        protected FlightSimulatorConnection flightSimulatorConnection;

        /// <summary>
        /// Creates a new FlightSimulatorConnector.
        /// </summary>
        /// <param name="onConnected">Event handler that is called when a connection to a simulator is established.</param>
        /// <param name="onPositionReceived">Event handler that is called when new position data from a simualtor is received.</param>
        /// <param name="onSimExited">Event handler that is called when the connection to a simulator is lost.</param>
        public FlightSimulatorConnector(FlightSimulatorConnection flightSimulatorConnection)
        {
            this.flightSimulatorConnection = flightSimulatorConnection;
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
