using System;

namespace WikiFlight.Common
{
    /// <summary>
    /// Position on earth defined by coordinates (latitude and longitude).
    /// </summary>
    public class Position
    {
        public Position(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; }

        public double Longitude { get; }

        /// <summary>
        /// Calculates distance to other position
        /// </summary>
        /// <param name="other">Other position</param>
        /// <returns>Distance in meter</returns>
        public double GetDistance(Position other)
        {
            var d1 = Latitude * (Math.PI / 180.0);
            var num1 = Longitude * (Math.PI / 180.0);
            var d2 = other.Latitude * (Math.PI / 180.0);
            var num2 = other.Longitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return Math.Round(6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3))), 1);
        }
    }
}
