using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using UnityEngine;

namespace BrandonUtils.Exceptions
{
    public class BrandonException : SystemException
    {
        public BrandonException()
        {
        }

        protected BrandonException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BrandonException(string message) : base(message)
        {
        }

        public BrandonException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}