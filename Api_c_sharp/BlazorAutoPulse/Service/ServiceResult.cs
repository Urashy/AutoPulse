namespace BlazorAutoPulse.Service;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<string, string[]> ValidationErrors { get; set; }

    public static ServiceResult<T> SuccessResult(T data)
    {
        return new ServiceResult<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ServiceResult<T> ErrorResult(string message, Dictionary<string, string[]> validationErrors = null)
    {
        return new ServiceResult<T>
        {
            Success = false,
            ErrorMessage = message,
            ValidationErrors = validationErrors
        };
    }
}