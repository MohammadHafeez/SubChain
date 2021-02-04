using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2_SubChain.DAL;
using P2_SubChain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace P2_SubChain.Controllers
{
    public class LogisticsController : Controller
    {
        UserDAL userContext = new UserDAL();
        ChainDAL chainContext = new ChainDAL();

        public IActionResult Index()
        {
            List<Users> userList = new List<Users>();
            List<int> userIds = new List<int>();
            string userIdString = null;
            foreach (Chain c in chainContext.GetAllChains())
            {
                if (c.Status == "Efficient")
                {
                    userIdString = c.EfficientChain;
                    userIds = userIdString.Split(",").Select(Int32.Parse).ToList();
                    if (userIds.Contains(Convert.ToInt32(HttpContext.Session.GetInt32("UserId"))))
                    {
                        foreach (Users user in userContext.GetAllUser())
                        {
                            foreach (int userId in userIds)
                            {
                                if (user.UserId == userId)
                                {
                                    userList.Add(user);
                                }
                            }
                        }
                        TempData["ChainStatus"] = "Efficient Supply Chain";
                        HttpContext.Session.SetString("ChainStatus", "Efficient");
                        HttpContext.Session.SetInt32("ChainId", c.ChainId);
                        return View(userList);
                    }
                }
                else if (c.Status == "Responsive")
                {
                    userIdString = c.ResponsiveChain;
                    userIds = userIdString.Split(",").Select(Int32.Parse).ToList();
                    if (userIds.Contains(Convert.ToInt32(HttpContext.Session.GetInt32("UserId"))))
                    {
                        foreach (Users user in userContext.GetAllUser())
                        {
                            foreach (int userId in userIds)
                            {
                                if (user.UserId == userId)
                                {
                                    userList.Add(user);
                                }
                            }
                        }
                        TempData["ChainStatus"] = "Responsive Supply Chain";
                        HttpContext.Session.SetString("ChainStatus", "Responsive");
                        HttpContext.Session.SetInt32("ChainId", c.ChainId);
                        return View(userList);
                    }
                }
            }

            TempData["ChainStatus"] = "Not Part of a supply chain";
            return View(userList);
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(IFormCollection formdata)
        {
            string companyName = formdata["company"];
            string country = formdata["country"];
            string cityOrState = formdata["city"];
            string cat1 = formdata["cat1"];
            string cat2 = formdata["cat2"];
            string cat3 = formdata["cat3"];
            string cat4 = formdata["cat4"];
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

            List<string> catList = new List<string> { cat1, cat2, cat3, cat4 };
            foreach (string cat in catList.ToList())
            {
                if (cat == null)
                {
                    catList.Remove(cat);
                }
            }

            // signing user up
            Users user = new Users { CompanyName = companyName, CompanyType = "Logistics", Email = email, Password = pass, Country = country, CityorState = cityOrState, ProductCategory = catList };
            int id = userContext.AddUser(user);

            HttpContext.Session.SetInt32("UserId", id);
            return RedirectToAction("Index", "Logistics");
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignIn(IFormCollection formdata)
        {
            string email = formdata["email"];
            string pass = formdata["pass"];

            foreach (Users user in userContext.GetAllUser())
            {
                if (user.Email == email && user.Password == pass)
                {
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    return RedirectToAction("Index", "Logistics");
                }
            }

            TempData["auth_error"] = "Email or password is invalid";
            return View();
        }
    }
}
