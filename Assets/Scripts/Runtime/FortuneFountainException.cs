using System;

using BrandonUtils.Standalone.Exceptions;

namespace Runtime {
    /// <summary>
    /// An exception that means something specific to Fortune Fountain's game logic went wrong.
    /// </summary>
    public class FortuneFountainException : BrandonException {
        public FortuneFountainException() { }

        //protected FortuneFountainException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
        public FortuneFountainException(string message) : base(message) { }
        public FortuneFountainException(string message, Exception innerException) : base(message, innerException) { }
    }
}