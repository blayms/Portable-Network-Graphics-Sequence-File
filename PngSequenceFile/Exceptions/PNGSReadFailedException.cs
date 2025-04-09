using System;

namespace Blayms.PNGS.Exceptions
{
    internal class PNGSReadFailedException : Exception
    {
        public PNGSReadFailedException(string reason) : base($"Failed reading PNG Sequence File because {reason}") { }
    }
}
