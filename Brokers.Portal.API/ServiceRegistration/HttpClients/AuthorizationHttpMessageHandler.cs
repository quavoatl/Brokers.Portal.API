using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Brokers.Portal.API.Extensions;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Brokers.Portal.API.ServiceRegistration.HttpClients
{
    public class AuthorizationHttpMessageHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TokenHelper _tokenHelper;

        public AuthorizationHttpMessageHandler(IHttpContextAccessor httpContextAccessor, TokenHelper tokenHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken =
                await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.SetBearerToken(accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
