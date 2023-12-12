using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;

namespace Operation.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 入口
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }
    }
}
