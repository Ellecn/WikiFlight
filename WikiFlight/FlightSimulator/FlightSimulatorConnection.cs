using System;
using System.Diagnostics;
using WikiFlight.Common;
using WikiFlight.MSFS2020;

namespace WikiFlight.FlightSimulator
{
    public class FlightSimulatorConnection
    {
        public delegate void ConnectedHandler();
        public readonly ConnectedHandler OnConnected;

        public delegate void PositionReceivedHandler(Position currentPosition);
        public readonly PositionReceivedHandler OnPositionReceived;

        public delegate void SimExitedHandler();
        public readonly SimExitedHandler OnSimExited;

        private FlightSimulatorConnector? flightSimulatorConnector;

        public FlightSimulatorConnection(ConnectedHandler onConnected, PositionReceivedHandler onPositionReceived, SimExitedHandler onSimExited)
        {
            OnConnected = onConnected;
            OnPositionReceived = onPositionReceived;
            OnSimExited = onSimExited;
        }

        /// <summary>
        /// Indicates wether a connection to a simulator is established.
        /// </summary>
        /// <returns>true if connection is established, otherwise false.</returns>
        public bool IsConnected()
        {
            if (flightSimulatorConnector == null)
            {
                return false;
            }
            return flightSimulatorConnector.IsConnected();
        }

        /// <summary>
        /// Initiates a connection to a simulator. On success <see cref="OnConnected"/> will be called.
        /// </summary>
        public void Connect(Type connectorType)
        {
            try
            {
                flightSimulatorConnector = Activator.CreateInstance(connectorType, this) as FlightSimulatorConnector;
                if (flightSimulatorConnector != null)
                {
                    flightSimulatorConnector.Connect();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Connector '" + connectorType + "' could not be created. (" + ex.Message + ")");
            }
        }

        /// <summary>
        /// Disconnects from a simulator.
        /// </summary>
        public void Disconnect()
        {
            if (flightSimulatorConnector != null)
            {
                flightSimulatorConnector.Disconnect();
                flightSimulatorConnector = null;
            }
        }

        /// <summary>
        /// Requests current position of the plane. On receiving <see cref="OnPositionReceived"/> is called.
        /// </summary>
        public void RequestCurrentPosition()
        {
            if (flightSimulatorConnector != null)
            {
                flightSimulatorConnector.RequestCurrentPosition();
            }
        }
    }
}
