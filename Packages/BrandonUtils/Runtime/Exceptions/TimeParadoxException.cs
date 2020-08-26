using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Packages.BrandonUtils.Runtime.Exceptions {
    public class TimeParadoxException : BrandonException {
        public TimeParadoxException() { }
        protected TimeParadoxException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
        public TimeParadoxException(string message) : base(message) { }
        public TimeParadoxException(string message, Exception innerException) : base(message, innerException) { }
    }
}