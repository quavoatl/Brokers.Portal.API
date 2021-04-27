using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Brokers.Portal.API.Extensions
{
    public class TokenHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenHelper(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }

        public async Task WriteIdentityInformationToConsole()
        {
            var idToken = await _httpContextAccessor.HttpContext
                .GetTokenAsync(OpenIdConnectParameterNames.IdToken);

            Debug.WriteLine($"Identity token: {idToken}");

            var user = _httpContextAccessor.HttpContext.User.Identities;

            foreach (var claim in _httpContextAccessor.HttpContext.User.Claims)
            {
                
                Debug.WriteLine($"Claim type: {claim.Type} / Claim value: {claim.Value}");
            }
        }

        public async Task<UserInfoResponse> CallUserInfoEndpoint()
        {
            var authorizationServerClient = _httpClientFactory.CreateClient("AuthorizationServer.IS4");

            var discoveryDoc = await authorizationServerClient.GetDiscoveryDocumentAsync();

            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var userClaims = await authorizationServerClient.GetUserInfoAsync(new UserInfoRequest
            {
                Address = discoveryDoc.UserInfoEndpoint,
                Token = accessToken
            });
            return userClaims;
        }

        public async Task<string> GetAccessTokenAsync()
        { 
            var expiresAt = await _httpContextAccessor.HttpContext.GetTokenAsync("expires_at");
            var expiresAtDateTimeOffset = DateTimeOffset.Parse(expiresAt, CultureInfo.InvariantCulture);

            if (expiresAtDateTimeOffset.AddSeconds(-60).ToUniversalTime() > DateTime.UtcNow)
            {
                return await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            }

            var authorizationServerClient = _httpClientFactory.CreateClient("AuthorizationServer.IS4");

            var discoveryDoc = await authorizationServerClient.GetDiscoveryDocumentAsync();

            var refreshToken =
                await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            var refreshResponse = await authorizationServerClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = discoveryDoc.TokenEndpoint,
                ClientId = "brokers-portal-api2",
                ClientSecret = "secret",
                RefreshToken = refreshToken
            });

            var updatedTokens = new List<AuthenticationToken>();

            updatedTokens.Add(new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.IdToken,
                Value = refreshResponse.IdentityToken
            });
            updatedTokens.Add(new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.AccessToken,
                Value = refreshResponse.AccessToken
            });
            updatedTokens.Add(new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.RefreshToken,
                Value = refreshResponse.RefreshToken
            });
            updatedTokens.Add(new AuthenticationToken
            {
                Name = "expires_at",
                Value = (DateTime.UtcNow + TimeSpan.FromSeconds(refreshResponse.ExpiresIn))
                            .ToString("o",CultureInfo.InvariantCulture)
            });

            var currentAuthenticationResult = await _httpContextAccessor.HttpContext
                .AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            currentAuthenticationResult.Properties.StoreTokens(updatedTokens);

            await _httpContextAccessor.HttpContext.SignInAsync
            (
                CookieAuthenticationDefaults.AuthenticationScheme,
                currentAuthenticationResult.Principal,
                currentAuthenticationResult.Properties
            );

            return refreshResponse.AccessToken;
        }
    }
}
