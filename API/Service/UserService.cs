using API.Dtos;
using API.Helpers;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace API.Service;

public class UserService : IUserService
{
    private readonly JWT _jwt;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(IUnitOfWork unitOfWork, IOptions<JWT> jwt,
        IPasswordHasher<User> passwordHasher)
    {
        _jwt = jwt.Value;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<string> AddUserAsync(AddUserDto addUserDto)
    {
        var user = new User
        {
            Names = addUserDto.Names,
            MotherLastName = addUserDto.MotherLastName,
            PaternLastName = addUserDto.PaternLastName,
            Email = addUserDto.Email,
            UserName = addUserDto.UserName,
            PhoneNumber = addUserDto.PhoneNumber
        };

        user.Password = _passwordHasher.HashPassword(user, addUserDto.Password);

        var userExistWithUsername = _unitOfWork.Users
                                    .Find(u => u.UserName.ToLower() == addUserDto.UserName.ToLower())
                                    .FirstOrDefault();

        if (userExistWithUsername != null)
            return $"The user with username {userExistWithUsername.UserName} already exists";


        var userExistWithEmail = _unitOfWork.Users
                                    .Find(u => u.Email == addUserDto.Email)
                                    .FirstOrDefault();

        if (userExistWithEmail != null)
            return $"The user with email {userExistWithEmail.Email} already exists";


        var userExistWithPhoneNumber = _unitOfWork.Users
                            .Find(u => u.PhoneNumber == addUserDto.PhoneNumber)
                            .FirstOrDefault();

        if (userExistWithPhoneNumber != null)
            return $"The user with phone number {userExistWithPhoneNumber.PhoneNumber} already exists";

        var defaultRol = _unitOfWork.Rols
                                .Find(u => u.Name == Authorization.default_rol.ToString())
                                .First();
        try
        {
            user.Rols.Add(defaultRol);
            _unitOfWork.Users.Add(user);
            await _unitOfWork.SaveAsync();

            return $"Sucessfully user {addUserDto.UserName} register";
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            return $"Error: {message}";
        }

    }


    public async Task<UserDataDto> LoginUserAsync(LoginUserDto model)
    {
        UserDataDto userDataDto = new UserDataDto();
        var user = await _unitOfWork.Users
                    .GetByUsernameAsync(model.UserName);

        if (user == null)
        {
            userDataDto.IsAuthenticate = false;
            userDataDto.Message = $"There is no user with the username {model.UserName}.";
            return userDataDto;
        }

        var resultado = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

        if (resultado == PasswordVerificationResult.Success)
        {
            userDataDto.IsAuthenticate = true;
            JwtSecurityToken jwtSecurityToken = CreateJwtToken(user);
            userDataDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            userDataDto.Email = user.Email;
            userDataDto.UserName = user.UserName;
            userDataDto.Rols = user.Rols
                                            .Select(u => u.Name)
                                            .ToList();

            if (user.RefreshTokens.Any(a => a.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.Where(a => a.IsActive == true).FirstOrDefault();
                userDataDto.RefreshToken = activeRefreshToken.Token;
                userDataDto.RefreshTokenExpiration = activeRefreshToken.Expires;
            }
            else
            {
                var refreshToken = CreateRefreshToken();
                userDataDto.RefreshToken = refreshToken.Token;
                userDataDto.RefreshTokenExpiration = refreshToken.Expires;
                user.RefreshTokens.Add(refreshToken);
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveAsync();
            }

            return userDataDto;
        }
        userDataDto.IsAuthenticate = false;
        userDataDto.Message = $"Incorrect password for user {user.UserName}.";
        return userDataDto;
    }

    public async Task<string> AddRolAsync(AddRolDto model)
    {

        var usuario = await _unitOfWork.Users
                    .GetByUsernameAsync(model.UserName);

        if (usuario == null)
        {
            return $"No existe algún usuario registrado con la cuenta {model.UserName}.";
        }


        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.Password, model.Password);

        if (resultado == PasswordVerificationResult.Success)
        {


            var rolExiste = _unitOfWork.Rols
                                        .Find(u => u.Name.ToLower() == model.Rol.ToLower())
                                        .FirstOrDefault();

            if (rolExiste != null)
            {
                var usuarioTieneRol = usuario.Rols
                                            .Any(u => u.Id == rolExiste.Id);

                if (usuarioTieneRol == false)
                {
                    usuario.Rols.Add(rolExiste);
                    _unitOfWork.Users.Update(usuario);
                    await _unitOfWork.SaveAsync();
                }

                return $"Rol {model.Rol} agregado a la cuenta {model.UserName} de forma exitosa.";
            }

            return $"Rol {model.Rol} no encontrado.";
        }
        return $"Credenciales incorrectas para el usuario {usuario.UserName}.";
    }

    public async Task<UserDataDto> RefreshTokenAsync(string refreshToken)
    {
        var datosUsuarioDto = new UserDataDto();

        var usuario = await _unitOfWork.Users
                        .GetByRefreshTokenAsync(refreshToken);

        if (usuario == null)
        {
            datosUsuarioDto.IsAuthenticate = false;
            datosUsuarioDto.Message = $"El token no pertenece a ningún usuario.";
            return datosUsuarioDto;
        }

        var refreshTokenBd = usuario.RefreshTokens.Single(x => x.Token == refreshToken);

        if (!refreshTokenBd.IsActive)
        {
            datosUsuarioDto.IsAuthenticate = false;
            datosUsuarioDto.Message = $"El token no está activo.";
            return datosUsuarioDto;
        }
        //Revocamos el Refresh Token actual y
        refreshTokenBd.Revoked = DateTime.UtcNow;
        //generamos un nuevo Refresh Token y lo guardamos en la Base de Datos
        var newRefreshToken = CreateRefreshToken();
        usuario.RefreshTokens.Add(newRefreshToken);
        _unitOfWork.Users.Update(usuario);
        await _unitOfWork.SaveAsync();
        //Generamos un nuevo Json Web Token 😊
        datosUsuarioDto.IsAuthenticate = true;
        JwtSecurityToken jwtSecurityToken = CreateJwtToken(usuario);
        datosUsuarioDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        datosUsuarioDto.Email = usuario.Email;
        datosUsuarioDto.UserName = usuario.UserName;
        datosUsuarioDto.Rols = usuario.Rols
                                        .Select(u => u.Name)
                                        .ToList();
        datosUsuarioDto.RefreshToken = newRefreshToken.Token;
        datosUsuarioDto.RefreshTokenExpiration = newRefreshToken.Expires;
        return datosUsuarioDto;
    }



    private RefreshToken CreateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expires = DateTime.UtcNow.AddDays(10),
                Created = DateTime.UtcNow
            };
        }
    }

    private JwtSecurityToken CreateJwtToken(User user)
    {
        var rols = user.Rols;
        var rolClaims = new List<Claim>();
        foreach (var rol in rols)
        {
            rolClaims.Add(new Claim("rols", rol.Name));
        }
        var claims = new[]
        {
                                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                                new Claim("uid", user.Id.ToString())
                        }
        .Union(rolClaims);
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
            signingCredentials: signingCredentials);
        return jwtSecurityToken;
    }
}
