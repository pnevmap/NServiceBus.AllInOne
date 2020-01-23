using System.Collections.Generic;

namespace Shared.Web.Bootstrapping
{
    public class BadRequestError
    {
        public const string DefaultTitle = "Bad Request";
        
        public Dictionary<string, string[]> Errors { get; }
        public string Title { get; set; }
        public int Status { get; set; } = 400;
        public string TraceId { get; set; }

        public BadRequestError()
        {
            Title = DefaultTitle;
            Errors = new Dictionary<string, string[]>();
        }

        public BadRequestError(Dictionary<string, string[]> errors)
        {
            Title = DefaultTitle;
            Errors = errors;
        }

        public BadRequestError(string title, Dictionary<string, string[]> errors)
        {
            Title = title;
            Errors = errors;
        }
    }
}