using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            var userId = _userManager.GetUserId(User);
            var model = await _context.BasketItems.Include(basketItems => basketItems.Basket).Where(basketItems => basketItems.Basket.UserID == userId).Select(basketItems => new BasketItemModel()
            {
                ItemAmount = basketItems.Amount.Value,
                ItemID = basketItems.ItemID
            }).ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> SubmitOrders()
        {
            //convert basket items to operation and clear previous basket informations
            return View("CustomerBoard", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task AddBasket([Bind("ItemID,ItemAmount")] BasketItemModel value)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var basket = await _context.Baskets.FirstOrDefaultAsync(basket => basket.UserID == userId);
                if(basket == null)
                {
                    basket = new Basket() { UserID= userId , Date= DateTime.Now};
                    _context.Baskets.Add(basket);
                    await _context.SaveChangesAsync();
                    basket = await _context.Baskets.FirstOrDefaultAsync(basket => basket.UserID == userId);
                }
                var item = await _context.Items.Where(item => item.ItemID == value.ItemID).FirstOrDefaultAsync();
                _context.BasketItems.Add(new BasketItem() { BasketID = basket.BasketID, Amount = value.ItemAmount, ItemID = value.ItemID, BasketPrice = item.ItemPrice.Value * value.ItemAmount });
                await _context.SaveChangesAsync();
            }
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
        public int ItemAmount { get; set; }
    }

}
