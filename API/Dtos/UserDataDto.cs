using Newtonsoft.Json;

namespace API.Dtos;

public class UserDataDto
{
    public string Message { get; set; }
    public bool IsAuthenticate { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public List<string> Rols { get; set; }
    public string Token { get; set; }
    [JsonIgnore]
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
}
