namespace API.Helpers.Errors;
public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }

    public ApiResponse(int statusCode, string message = null)
    {
        StatusCode = statusCode;
        Message = message ?? GetDefaultMessage(statusCode);
    }

    private string GetDefaultMessage(int statusCode)
    {
        return statusCode switch
        {
            400 => "Wron petition.",
            401 => "User not authorized.",
            404 => "Resource not found.",
            405 => "HTTP method isn't allowed.",
            500 => "Server error"
        };
    }
}
