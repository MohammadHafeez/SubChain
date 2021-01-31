using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2_SubChain.Models;
using P2_SubChain.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace P2_SubChain.Controllers
{
    public class DistributorController : Controller
    {
        UserDAL userContext = new UserDAL();
        InvoiceDAL invoiceContext = new InvoiceDAL();
        ProductDAL productContext = new ProductDAL();

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
                    return RedirectToAction("Index", "Distributor");
                }
            }

            TempData["auth_error"] = "Email or password is invalid";
            return View();
        }

        public IActionResult Invoice()
        {

            List<Invoice> invoiceList = invoiceContext.GetAllInvoice();
            return View(invoiceList);
        }


        public IActionResult CreateInv()
        {
            return View();
        }

        public ActionResult Create(Invoice invoice)
        {

            if (ModelState.IsValid)
            {
                //Add staff record to database
                invoice.SupplierID = invoiceContext.Add(invoice);
                //Redirect user to Staff/Index view
                return RedirectToAction("Invoice");
            }
            else
            {
                //Input validation fails, return to the Create view
                //to display error message
                return View(invoice);
            }
        }

        [HttpPost]
        public IActionResult Search(IFormCollection formdata)
        {
            string companyName = formdata["companyName"];
            string cat = formdata["select"];

            List<Users> userList = new List<Users>();
            foreach (Users u in userContext.GetAllUser())
            {
                if (u.CompanyType == "Supplier" || u.CompanyType == "Logistics")
                {
                    if (u.CompanyName.Contains(companyName) || u.ProductCategory.Contains(cat))
                    {
                        userList.Add(u);
                    }
                }
            }

            return View(userList);
        }

    }
    
}
