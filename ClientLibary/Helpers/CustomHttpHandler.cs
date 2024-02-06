
using BaseLibrary.DTOs;
using ClientLibrary.Services.Contracts;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ClientLibrary.Helpers
{
    public class CustomHttpHandler(
        GetHttpClient getHttpClient, LocalStorageService localStorageService, IUserAccountService userAccountService) : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool loginUrl = request.RequestUri!.AbsolutePath.Contains("login");
            bool registerUrl = request.RequestUri!.AbsolutePath.Contains("register");
            bool refreshTokenUrl = request.RequestUri!.AbsolutePath.Contains("refresh-token");

            if (loginUrl || registerUrl || refreshTokenUrl)
                return await base.SendAsync(request, cancellationToken);

            var result = await base.SendAsync(request, cancellationToken);
            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                var localStorageToken = await localStorageService.GetToken();
                if (string.IsNullOrEmpty(localStorageToken))
                    return result;

                var headerToken = string.Empty;
                try
                { headerToken = request.Headers.Authorization?.Parameter!; }
                catch (Exception) { }

                var deserializedToken = Serializations.DeserializeJsonString<UserSession>(localStorageToken);
                if (deserializedToken is null)
                    return result;

                if (string.IsNullOrEmpty(headerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deserializedToken.Token);
                    return await base.SendAsync(request, cancellationToken);
                }
                else if (headerToken != deserializedToken.Token)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deserializedToken.Token);
                    return await base.SendAsync(request, cancellationToken);
                }

                var newJwtToken = await GetRefreshToken(deserializedToken.RefreshToken!);
                if (string.IsNullOrEmpty(newJwtToken))
                    return result;

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newJwtToken);
                return await base.SendAsync(request, cancellationToken);
            }
            return result;
        }

        private async Task<string> GetRefreshToken(string refreshToken)
        {
            var result = await userAccountService.RefreshTokenAsync(new RefreshToken { Token = refreshToken });
            string serializedToken = Serializations.SerializeObject(new UserSession() { Token = result.Token, RefreshToken = result.RefreshToken });
            await localStorageService.SetToken(serializedToken);
            return result.Token;

        }

    }
}
