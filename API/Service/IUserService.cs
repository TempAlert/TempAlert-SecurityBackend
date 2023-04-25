using API.Dtos;

namespace API.Service;
public interface IUserService
{
    Task<string> AddUserAsync(AddUserDto model);
    Task<UserDataDto> LoginUserAsync(LoginUserDto model);

    Task<string> AddRolAsync(AddRolDto model);
    Task<UserDataDto> RefreshTokenAsync(string refreshToken);
}
