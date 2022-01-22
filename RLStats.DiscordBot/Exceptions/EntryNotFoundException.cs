﻿using System;
using System.Runtime.Serialization;

namespace Discord_Bot.Exceptions
{
    public class EntryNotFoundException : Exception
    {
        public EntryNotFoundException()
        {
        }

        public EntryNotFoundException(string message) : base(message)
        {
        }

        public EntryNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntryNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
