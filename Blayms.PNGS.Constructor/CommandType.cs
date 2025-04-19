namespace Blayms.PNGS.Constructor
{
    public enum CommandType
    {
        /// <summary>
        /// Runs once and exits immediately
        /// </summary>
        Once,
        /// <summary>
        /// Enters a command block that cannot be existed, unless if user provides a command that finishes execution of that block
        /// </summary>
        ModeEnter,
    }
}
