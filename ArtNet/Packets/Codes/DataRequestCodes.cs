namespace ArtNet.Packets.Codes
{
    /// <summary>
    /// The following enum contains the DataRequest Codes.
    /// <para>These codes are used by ArtDataRequest and ArtDataReply.</para>
    /// </summary>
    public enum DataRequestCodes : ushort
    {
        /// <summary>
        /// Controller is polling to establish whether ArtDataRequest is supported.
        /// </summary>
        DrPoll = 0x0000,
        /// <summary>
        /// URL to manufacturer product page.
        /// </summary>
        DrUrlProduct = 0x0001,
        /// <summary>
        /// URL to manufacturer user guide.
        /// </summary>
        DrUrlUserGuide = 0x0002,
        /// <summary>
        /// URL to manufacturer support page.
        /// </summary>
        DrUrlSupport = 0x0003,
        /// <summary>
        /// URL to manufacturer UDR personality.
        /// </summary>
        DrUrlPersUdr = 0x0004,
        /// <summary>
        /// URL to manufacturer GDTF personality.
        /// </summary>
        DrUrlPersGdtf = 0x0005,
        /// <summary>
        /// Manufacturer specific use.
        /// </summary>
        /// <remark>Can be 0x8000-0xffff</remark>
        DrManSpec = 0x8000
    }
}
