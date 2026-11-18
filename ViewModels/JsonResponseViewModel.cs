namespace SwiftDrop.ViewModels
{
    public class JsonResponseViewModel
    {
        public string? Message { get; set; }

        public string? ReturnedURL { get; set; }

        public IEnumerable<string>? Errors { get; set; }

        public JsonResponseViewModel(string? msg, string? returnURL, IEnumerable<string>? errors)
        {
            Message = msg;
            ReturnedURL = returnURL;
            Errors = errors;

        }
    }
}
