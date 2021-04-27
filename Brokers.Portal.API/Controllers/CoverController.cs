using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Broker.Portal.Contracts.CoverMicroservice.V1;
using BrokersPortal.Client.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace Brokers.Portal.API.Controllers
{
    [Authorize(Roles = "broker")]
    [ApiController]
    [Route("/api/[controller]")]
    public class CoverController : ControllerBase
    {
        private readonly ILogger<CoverController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public CoverController(ILogger<CoverController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IEnumerable<CoverResponse>> Get()
        {
            var userClaims = User.Claims;
            var client = _httpClientFactory.CreateClient(ClientNames.BROKERS_COVER_API);

            var httpMessage = await client.GetAsync("/api/v1/covers");
            var response = await httpMessage.Content.ReadAsStringAsync();
            var newItems = JsonConvert.DeserializeObject<IEnumerable<CoverResponse>>(response).ToList();

            return newItems;
        }
    }
}
