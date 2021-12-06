using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WholeSaler.Data;
using WholeSaler.Models;

namespace WholeSaler.Controllers
{
    [Authorize(Roles = "Customer")]
    public class BasketController : Controller
    {
        private readonly WholesalerContext _context;
        private readonly UserManager<User> _userManager;

        public BasketController(WholesalerContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _context.Categories.Select(category => new CategoryViewModel() { Category = category, ElementCount = _context.Items.Where(item => item.CategoryID == category.CategoryID).Count() }).ToListAsync(); ;
            return View(model);
        }

        public async Task<IActionResult> ViewCategory(int categoryId)
        {
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
            var basket = await _context.BasketItems.Include(basketItems => basketItems.Basket).Where(basketItems => basketItems.Basket.UserID == userId).ToListAsync();
            if (basket != null)
            {
                var value = 0.0;
                foreach (var basketItem in basket)
                {
                    value += basketItem.BasketPrice;
                }
                var operation = new Operation() {
                    BasketID = basket.FirstOrDefault().BasketID,
                    Date = DateTime.Now,
                    LocationID = locationId,
                    OperationValue = value,
                    OwnerID = userId };
                _context.Operations.Add(operation);
                await _context.SaveChangesAsync();
            }
            return View("UserBoard", "Dashboard");
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
