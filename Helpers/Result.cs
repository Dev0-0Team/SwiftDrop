namespace SwiftDrop.Helpers
{
    public enum ResultStatus
    {
        Success,
        NotFound,
        DatabaseError,
        ValidationError,
        Unauthorized,
        EmailNotConfirmed,
        ExternalNewUser,
        RequestCanceled
    }

    public class Result
    {

        public ResultStatus StatusType { get; set; }
        public bool IsSuccess => StatusType == ResultStatus.Success;
        public string? Message { get; set; }
        public List<string>? ValidationErrors { get; set; }

        public Result(ResultStatus statusType, string? message = null, List<string>? validationErrors = null)
        {
            StatusType = statusType;
            Message = message;
            ValidationErrors = validationErrors;
        }


    }

    public class Result<T>
    {
        public ResultStatus StatusType { get; set; }

        public bool IsSuccess => StatusType == ResultStatus.Success;

        public string? Message { get; set; }
        public T? Data { get; set; }

        public List<string>? ValidationErrors { get; set; }

        public Result(ResultStatus statusType, T data, string? message = null)
        {


            StatusType = statusType;
            Message = message;
            Data = data;
        }
        public Result(ResultStatus statusType, T data, string? message = null, List<string>? validationErrors = null)
        {

            StatusType = statusType;
            Message = message;
            Data = data;
            ValidationErrors = validationErrors;
        }



    }
}