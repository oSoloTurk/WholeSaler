using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WholeSaler.Data;
using WholeSaler.Models;

namespace WholeSaler.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly WholesalerContext _context;

        public HomeController(ILogger<HomeController> logger,
            UserManager<User> userManager,
            SignInManager<User> signInManager, 
            WholesalerContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if(User.IsInRole("Admin"))
            {
                return RedirectToAction("AdminBoard", "Dashboard");
            }
            if(User.IsInRole("Customer"))
            {
                return RedirectToAction("UserBoard", "Dashboard");
            }
            var countries = await _context.Countries.Select(model => new SelectListItem { Value = model.CountryID.ToString(), Text = model.CountryName }).ToListAsync();
            return View("Showcase", countries);
        }

        [AllowAnonymous]
        public async Task<IActionResult> FillCities(int countryId)
        {
            var cities = await _context.Cities.Where(model => model.CountryID == countryId && !model.OperationalState).Select(model => new SelectListItem { Value = model.CityID.ToString(), Text = model.CityName }).ToListAsync();
            return Json(cities);
        }

        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
    
}
