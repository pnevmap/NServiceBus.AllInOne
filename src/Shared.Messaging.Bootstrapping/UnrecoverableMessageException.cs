using System;
using System.Runtime.Serialization;

namespace Shared.Messaging
{
    [Serializable]
    public class UnrecoverableMessageException : Exception
    {
        public UnrecoverableMessageException()
        {
        }

        public UnrecoverableMessageException(string message) : base(message)
        {
        }

        public UnrecoverableMessageException(string message, Exception inner) : base(message, inner)
        {
        }

        protected UnrecoverableMessageException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
