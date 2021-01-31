using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2_SubChain.Models;
using P2_SubChain.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace P2_SubChain.Controllers
{
    public class DistributorController : Controller
    {
        UserDAL userContext = new UserDAL();

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(IFormCollection formdata)
        {
            string companyName = formdata["company"];
            string email = formdata["email"];
            string pass = formdata["pass"];
            string pass_confirm = formdata["pass-confirm"];

            bool email_fail = false;
            bool pass_fail = false;
            Match m = Regex.Match(email, @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");

            // validating email input
            if (!m.Success)
            {
                TempData["email_error"] = "Please enter a valid email";
                email_fail = true;
            }

            // validating password input
            if (pass != pass_confirm)
            {
                TempData["pass_error"] = "Passwords do not match";
                pass_fail = true;
            }
            else if (pass.Length < 8)
            {
                TempData["pass_error"] = "Password should be 8 characters long";
                pass_fail = true;
            }

            if (email_fail || pass_fail)
            {
                return View();
            }

            //checking for existing suppliers
            foreach (Users u in userContext.GetAllUser())
            {
                if (u.Email == email)
                {
                    TempData["email_error"] = "User already exists";
                    return View();
                }
            }

            // signing user up
            Users user = new Users { CompanyName = companyName, CompanyType = "Distributor", Email = email, Password = pass };
            int id = userContext.AddUser(user);

            HttpContext.Session.SetInt32("UserId", id);
            return RedirectToAction("Index", "Distributor");
        }
    }
}
