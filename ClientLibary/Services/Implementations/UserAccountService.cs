using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using ClientLibrary.Helpers;
using ClientLibrary.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary.Services.Implementations
{
    public class UserAccountService(GetHttpClient getHttpClient) : IUserAccountService
    {
        public const string AuthUrl = "api/authentication";
        public async Task<GeneralResponse> CreateAsync(Register user)
        {
            var httpClient = getHttpClient.GetPublicHttpClient();
            var response = await httpClient.PostAsJsonAsync($"{AuthUrl}/register", user);
            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, "Failed to create user");

            return await response.Content.ReadFromJsonAsync<GeneralResponse>()!;

        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshToken refreshToken)
        {
            var httpClient = getHttpClient.GetPublicHttpClient();
            var response = await httpClient.PostAsJsonAsync($"{AuthUrl}/refresh-token", refreshToken);
            if (!response.IsSuccessStatusCode)
                return new LoginResponse(false, "Failed to refresh token");

            return await response.Content.ReadFromJsonAsync<LoginResponse>()!;
        }

        public async Task<LoginResponse> SignInAsync(Login user)
        {
            var httpClient = getHttpClient.GetPublicHttpClient();
            var response = await httpClient.PostAsJsonAsync($"{AuthUrl}/login", user);
            if (!response.IsSuccessStatusCode)
                return new LoginResponse(false, "Failed to login");

            return await response.Content.ReadFromJsonAsync<LoginResponse>()!;
        }
    }
}
