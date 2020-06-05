using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LinkedInPoc.Mvc.Models;
using Mmu.Mlh.RestExtensions.Areas.RestProxies;
using Mmu.Mlh.RestExtensions.Areas.RestCallBuilding;
using Mmu.Mlh.RestExtensions.Areas.Models.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LinkedInPoc.Mvc.Services;

namespace LinkedInPoc.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRestProxy _restProxy;
        private readonly IRestCallBuilderFactory _restCallBuilderFactory;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IRestProxy restProxy,
            IRestCallBuilderFactory restCallBuilderFactory,
            ILogger<HomeController> logger)
        {
            _restProxy = restProxy;
            _restCallBuilderFactory = restCallBuilderFactory;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("Tra")]
        public async Task<IActionResult> GetPersonalInfos()
        {
            var uri = new Uri("https://api.linkedin.com/v2/me");
            var restCall = _restCallBuilderFactory.StartBuilding(uri)
                .WithSecurity(RestSecurity.CreateTokenSecurity("Bearer " + LinkedInAccessTokenSingleton.Value))
                .Build();

            var tra = await _restProxy.PerformCallAsync<string>(restCall);

            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
