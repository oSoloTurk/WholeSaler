using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace WholeSaler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TranslateController : ControllerBase
    {
        private readonly IStringLocalizer<TranslateController> _localizer;

        public TranslateController(IStringLocalizer<TranslateController> localizer)
        {
            _localizer = localizer;
        }

        [HttpGet]
        public string Get(string word)
        {
            return _localizer[word??""];
        }
    }
}
