using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WholeSaler.Data;
using WholeSaler.Models;
using WholeSaler.Services;

namespace WholeSaler.Controllers
{
    [Authorize(Roles = "SuperAdmin, Customer")]
    public class BasketController : Controller
    {
        private readonly WholesalerContext _context;
        private readonly UserManager<User> _userManager;
        private readonly AlertService _alertService;
        private readonly IStringLocalizer<BasketController> _localizer;

        public BasketController(WholesalerContext context, UserManager<User> userManager, IStringLocalizer<BasketController> localizer)
        {
            _context = context;
            _userManager = userManager;
            _alertService = new AlertService(_context);
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            ViewData["BasketSize"] = _context.BasketItems.Include(item => item.Basket).Include(item => item.Item).Where(item => item.Basket.UserID == userId).Count();
            var model = await _context.Categories.Select(category => new CategoryViewModel() { Category = category, ElementCount = _context.Items.Where(item => item.CategoryID == category.CategoryID).Count() }).ToListAsync(); ;
            return View(model);
        }

        public async Task<IActionResult> ViewCategory(int categoryId)
        {
            var userId = _userManager.GetUserId(User);
            ViewData["BasketSize"] = _context.BasketItems.Include(item => item.Basket).Include(item => item.Item).Where(item => item.Basket.UserID == userId).Count();
            var model = await _context.Items.Where(item => item.CategoryID == categoryId).ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> Check()
        {
            var model = new CheckModel();
            var userId = _userManager.GetUserId(User);
            model.SelectLocations = await _context.Locations.Where(loc => loc.LocationOwnerID == userId).Include(loc => loc.City).Select(loc => new SelectListItem()
            {
                Value = loc.LocationID.ToString(),
                Text = loc.Adress + " " + loc.City.CityName + "/" + loc.City.Country.CountryName
            }).ToListAsync();
            model.BasketItems = await _context.BasketItems.Include(item => item.Basket).Include(item => item.Item).Where(item => item.Basket.UserID == userId).Select(basketItems => new BasketItemModel
            {
                ItemName = basketItems.Item.ItemName,
                ItemDesc = basketItems.Item.ItemDesc,
                ItemAmount = basketItems.Amount.Value,
                ItemID = basketItems.Item.ItemID,
                ItemBasketID = basketItems.BasketItemID
            }).ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOrders([Bind("LocationID")] int locationId)
        {
            var userId = _userManager.GetUserId(User);
            var basket = await _context.Baskets.FirstOrDefaultAsync(basket => basket.UserID == userId);
            var basketItems = await _context.BasketItems.Where(basketItems => basketItems.BasketID == basket.BasketID).ToListAsync();
            if (basketItems != null)
            {
                var value = 0.0;
                foreach (var basketItem in basketItems)
                {
                    value += basketItem.BasketPrice;
                }
                var operation = new Operation() {
                    BasketID = basketItems.FirstOrDefault().BasketID,
                    Date = DateTime.Now,
                    LocationID = locationId,
                    OperationValue = value,
                    OwnerID = userId };
                _context.Operations.Add(operation);                
                await _alertService.SendAlert(userId, _localizer["We got your orders we will send notification to you when sending you."], Url.Action("UserBoard", "Dashboard"));
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("UserBoard", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task AddBasket([Bind("ItemID,ItemAmount")] BasketItemModel value)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var basket = await _context.Baskets.FirstOrDefaultAsync(basket => basket.UserID == userId);
                if (basket == null)
                {
                    basket = new Basket() { UserID = userId, Date = DateTime.Now };
                    _context.Baskets.Add(basket);
                    await _context.SaveChangesAsync();
                    basket = await _context.Baskets.FirstOrDefaultAsync(basket => basket.UserID == userId);
                }
                var item = await _context.Items.Where(item => item.ItemID == value.ItemID).FirstOrDefaultAsync();
                _context.BasketItems.Add(new BasketItem() { BasketID = basket.BasketID, Amount = value.ItemAmount, ItemID = value.ItemID, BasketPrice = item.ItemPrice.Value * value.ItemAmount });
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IActionResult> RemoveBasket(int basketItemId)
            {
            var basketItem = await _context.BasketItems.FindAsync(basketItemId);
            if (basketItem != null)
            {
                _context.BasketItems.Remove(basketItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Check");
        }

    }
    public class CategoryViewModel
    {
        public Category Category { get; set; }
        public int ElementCount { get; set; }
    }

    public class BasketItemModel
    {
        public int ItemID { get; set; }
        public int ItemBasketID { get; set; }
        public string ItemName { get; set; }
        public string ItemDesc { get; set; }
        public int ItemAmount { get; set; }
    }

    public class CheckModel
    {
        public List<SelectListItem> SelectLocations { get; set; }
        public List<BasketItemModel> BasketItems { get; set; }
    }
}
