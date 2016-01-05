namespace LightBuzz.Vitruvius.FingerTracking
{
    /// <summary>
    /// A collection of hands for a specific Body.
    /// </summary>
    public class HandCollection
    {
        /// <summary>
        /// The tracking ID of the current body.
        /// </summary>
        public ulong TrackingId { get; set; }

        /// <summary>
        /// The left hand data of the current body.
        /// </summary>
        public Hand HandLeft { get; set; }

        /// <summary>
        /// The right hand data of the current body.
        /// </summary>
        public Hand HandRight { get; set; }
    }
}
