namespace TradingJournal.Contract.Common
{
    public class BaseResponse<T>
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static BaseResponse<T> Ok(T data, string message = "Success") => new()
        {
            StatusCode = 200,
            Success = true,
            Message = message,
            Data = data
        };

        public static BaseResponse<T> Created(T data, string message = "Created") => new()
        {
            StatusCode = 201,
            Success = true,
            Message = message,
            Data = data
        };

        public static BaseResponse<T> NotFound(string message = "Not found") => new()
        {
            StatusCode = 404,
            Success = false,
            Message = message,
            Data = default
        };

        public static BaseResponse<T> BadRequest(string message = "Bad request") => new()
        {
            StatusCode = 400,
            Success = false,
            Message = message,
            Data = default
        };

        public static BaseResponse<T> Unauthorized(string message = "Unauthorized") => new()
        {
            StatusCode = 401,
            Success = false,
            Message = message,
            Data = default
        };
    }
}
