using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Shared.Web.Bootstrapping
{
    [Serializable]
    public class BadRequestException : Exception
    {
        public BadRequestError Error { get; }
        
        public BadRequestException()
            : this(BadRequestError.DefaultTitle)
        {
            Error = new BadRequestError();
        }

        public BadRequestException(string message) 
            : base(message)
        {
            Error = new BadRequestError
            {
                Title = message
            };
        }

        public BadRequestException(string message, string field, params string[] errors) 
            : base(message)
        {
            Error = new BadRequestError
            {
                Title = message,
                Errors = {
                {
                    field, errors
                }}
            };
        }

        public BadRequestException(string message, Dictionary<string, string[]> errors) 
            : base(message)
        {
            Error = new BadRequestError(message, errors);
        }

        public BadRequestException(BadRequestError error) 
            : base(error?.Title ?? BadRequestError.DefaultTitle)
        {
            Error = error ?? new BadRequestError();
        }

        public BadRequestException(string message, Exception inner) 
            : base(message, inner)
        {
            Error = new BadRequestError
            {
                Title = message
            };
        }

        public BadRequestException(BadRequestError error, Exception inner)
            : base(error?.Title ?? BadRequestError.DefaultTitle, inner)
        {
            Error = error ?? new BadRequestError();
        }   
        
        protected BadRequestException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}