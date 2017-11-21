using System;
using System.Runtime.Serialization;

namespace Stellar
{
    [Serializable]
    internal class CosmosQueryException : Exception
    {
        public CosmosQueryException()
        {
        }

        public CosmosQueryException(string message) : base(message)
        {
        }

        public CosmosQueryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CosmosQueryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}