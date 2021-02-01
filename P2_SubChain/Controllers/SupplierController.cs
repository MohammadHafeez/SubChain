using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2_SubChain.DAL;
using P2_SubChain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace P2_SubChain.Controllers
{
    public class SupplierController : Controller
    {
        ProductDAL productContext = new ProductDAL();
        UserDAL userContext = new UserDAL();
        CommunicationDAL communicationContext = new CommunicationDAL();

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
            string country = formdata["country"];
            string cityOrState = formdata["city"];
            string cat1 = formdata["cat1"];
            string cat2 = formdata["cat2"];
            string cat3 = formdata["cat3"];
            string cat4 = formdata["cat4"];
            string cat5 = formdata["cat5"];
            string cat6 = formdata["cat6"];
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

            List<string> catList = new List<string> { cat1, cat2, cat3, cat4, cat5, cat6 };
            foreach (string cat in catList.ToList())
            {
                if (cat == null)
                {
                    catList.Remove(cat);
                }
            }

            // signing user up
            Users user = new Users { CompanyName = companyName, CompanyType = "Supplier", Email = email, Password = pass, Country = country, CityorState = cityOrState, ProductCategory = catList };
            int id = userContext.AddUser(user);

            HttpContext.Session.SetInt32("UserId", id);
            return RedirectToAction("Index", "Supplier");
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
                    return RedirectToAction("Index", "Supplier");
                }
            }

            TempData["auth_error"] = "Email or password is invalid";
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            product.UserId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));

            // Find the filename extension of the file to be uploaded.
            string fileExt = Path.GetExtension(product.Image.FileName);

            int productId = productContext.GetAllProducts().Count+1;
            string uploadedFile = "Product" + productId + fileExt;
            string savePath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot\\images", uploadedFile);
            // Upload the file to server
            using (var fileSteam = new FileStream(savePath, FileMode.Create))
            {
                await product.Image.CopyToAsync(fileSteam);
            }
            product.ImageUrl = uploadedFile;
            productContext.AddProduct(product);

            TempData["product_success"] = "Succussfully Created Product";
            return View();
        }

        public IActionResult Products(int? id)
        {
            List<Product> productList = new List<Product>();
            foreach (Product p in productContext.GetAllProducts())
            {
                if (p.UserId == id)
                {
                    productList.Add(p);
                }
            }
            return View(productList);
        }

        public IActionResult Communication()
        {
            List<Chat> chatList = new List<Chat>();
            foreach (Chat c in communicationContext.GetAllChats())
            {
                if (HttpContext.Session.GetInt32("UserId") == c.UserId1 || HttpContext.Session.GetInt32("UserId") == c.UserId2)
                {
                    chatList.Add(c);
                }
            }

            List<Users> userList = new List<Users>();
            foreach (Chat c in chatList)
            {
                foreach (Users u in userContext.GetAllUser())
                {
                    if (c.UserId1 == u.UserId && u.UserId != HttpContext.Session.GetInt32("UserId"))
                    {
                        userList.Add(u);
                    }
                    else if (c.UserId2 == u.UserId && u.UserId != HttpContext.Session.GetInt32("UserId"))
                    {
                        userList.Add(u);
                    }
                }
            }

            return View(userList);
        }

        public IActionResult Chat(int id)
        {
            ChatViewModel viewModel = new ChatViewModel();
            int count = 0;
            // check if chat exists
            foreach (Chat c in communicationContext.GetAllChats())
            {
                if (c.UserId1 == HttpContext.Session.GetInt32("UserId") && c.UserId2 == id)
                {
                    count += 1;
                }
                
                if (c.UserId2 == HttpContext.Session.GetInt32("UserId") && c.UserId1 == id)
                {
                    count += 1;
                }

                if (count > 0)
                {
                    foreach (Users u in userContext.GetAllUser())
                    {
                        if (c.UserId1 == u.UserId)
                        {
                            viewModel.User1 = u;
                        }

                        if (c.UserId2 == u.UserId)
                        {
                            viewModel.User2 = u;
                        }
                    }

                    foreach (Messages m in communicationContext.GetAllMessages())
                    {
                        if (m.ChatId == c.ChatId)
                        {
                            viewModel.Messages.Add(m);
                        }
                    }

                    viewModel.ChatId = c.ChatId;
                    return View(viewModel);
                }
            }


            // create a new chat
            int userId1 = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
            Chat chat = communicationContext.CreateChat(new Chat { UserId1 = userId1, UserId2 = id });

            foreach (Users u in userContext.GetAllUser())
            {
                if (userId1 == u.UserId)
                {
                    viewModel.User1 = u;
                }

                if (id == u.UserId)
                {
                    viewModel.User2 = u;
                }
            }

            viewModel.ChatId = chat.ChatId;
            viewModel.Messages = null;
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Chat(IFormCollection formdata)
        {
            int chatId = Convert.ToInt32(formdata["chatId"]);
            int senderId = Convert.ToInt32(formdata["chatId"]);
            string message = formdata["message"];

            Messages newMessage = new Messages { ChatId = chatId, SenderId = senderId, Message = message, Timestamp = DateTime.Now };
            communicationContext.CreateMessage(newMessage);

            ChatViewModel viewModel = new ChatViewModel();
            foreach (Chat c in communicationContext.GetAllChats())
            {
                if (c.ChatId == chatId)
                {
                    foreach (Users u in userContext.GetAllUser())
                    {
                        if (c.UserId1 == u.UserId)
                        {
                            viewModel.User1 = u;
                        }

                        if (c.UserId2 == u.UserId)
                        {
                            viewModel.User2 = u;
                        }
                    }

                    foreach (Messages m in communicationContext.GetAllMessages())
                    {
                        if (m.ChatId == c.ChatId)
                        {
                            viewModel.Messages.Add(m);
                        }
                    }
                }
            }

            return View(viewModel);
        }
    }
}
