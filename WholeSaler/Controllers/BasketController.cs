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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async void AddBasket([Bind("ItemID,ItemAmount")] BasketAddModel value)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var basket = await _context.Baskets.Where(basket => basket.UserID == userId).FirstOrDefaultAsync();
                if(basket == null)
                {
                    basket = new Basket() { UserID= userId , };
                }
                var basketItem = new BasketItem() { Amount = value.ItemAmount, BasketItemID = value.ItemID };
                var item = await _context.Items.Where(item => item.ItemID == value.ItemID).FirstOrDefaultAsync();
                basketItem.BasketPrice = item.ItemPrice.Value * value.ItemAmount;
                //basketItem.BasketID = null;
            }
        }
    }

    public class CategoryViewModel
    {
        public Category Category { get; set; }
        public int ElementCount { get; set; }
    }

    public class BasketAddModel
    {
        public int ItemID { get; set; }
        public int ItemAmount { get; set; }
    }

}
