using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ClientLibrary.Helpers
{
    public class CustomAuthenticationStateProvider(LocalStorageService localStorageService) : AuthenticationStateProvider
    {
        private readonly ClaimsPrincipal anonymous = new(new ClaimsIdentity());
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var stringToken = await localStorageService.GetToken();
            if (string.IsNullOrEmpty(stringToken))
                return new AuthenticationState(anonymous);

            var deserializedToken = Serializations.DeserializeJsonString<UserSession>(stringToken);
            if (deserializedToken == null)
                return await Task.FromResult(new AuthenticationState(anonymous));

            var getUserClaims = DecryptToken(deserializedToken.Token!);
            if (getUserClaims == null)
                return await Task.FromResult(new AuthenticationState(anonymous));

            var claimsPrincipal = SetClaimPrincipal(getUserClaims);
            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }

        public async Task UpdateAuthenticationState(UserSession userSession)
        {
            var claimsPrincipal = new ClaimsPrincipal();
            if (userSession.Token is not null || userSession.RefreshToken is not null)
            {
                var serializeSession = Serializations.SerializeObject(userSession);
                await localStorageService.SetToken(serializeSession);
                var getUserClaims = DecryptToken(userSession.Token!);
                claimsPrincipal = SetClaimPrincipal(getUserClaims);
            }
            else
            {
                await localStorageService.RemoveToken();
            }
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
        private static CustomUserClaims DecryptToken(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken))
                return new CustomUserClaims();

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            var userId = token.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))?.Value!;
            var email = token.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Email))?.Value!;
            var fullName = token.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name))?.Value!;
            var role = token.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Role))?.Value!;
            return new CustomUserClaims(userId, fullName, email, role);
        }

        private static ClaimsPrincipal SetClaimPrincipal(CustomUserClaims userClaims)
        {
            if (userClaims is null)
                return new ClaimsPrincipal();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userClaims.Id),
                new(ClaimTypes.Name, userClaims.Name),
                new(ClaimTypes.Email, userClaims.Email),
                new(ClaimTypes.Role, userClaims.Role)
            };
            var identity = new ClaimsIdentity(claims, "JwtAuth");
            return new ClaimsPrincipal(identity);
        }
    }
}
