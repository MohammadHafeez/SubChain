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
using System.IO;

namespace P2_SubChain.Controllers
{
    public class DistributorController : Controller
    {
        UserDAL userContext = new UserDAL();
        InvoiceDAL invoiceContext = new InvoiceDAL();
        ProductDAL productContext = new ProductDAL();
        CommunicationDAL communicationContext = new CommunicationDAL();
        ChainDAL chainContext = new ChainDAL();

        public IActionResult Index()
        {
            // creating chain viewmodel;
            ChainViewModel chainViewModel = new ChainViewModel();
            foreach (Chain c in chainContext.GetAllChains())
            {
                if (c.ChainOwner == HttpContext.Session.GetInt32("UserId"))
                {
                    chainViewModel.Status = c.Status;

                    if (c.EfficientChain != null)
                    {
                        foreach (int userId in c.EfficientChain.Split(",").Select(Int32.Parse).ToList())
                        {
                            foreach (Users u in userContext.GetAllUser())
                            {
                                if (userId == u.UserId)
                                {
                                    chainViewModel.EfficientChain.Add(u);
                                }
                            }
                        }
                    }

                    if (c.ResponsiveChain != null)
                    {
                        foreach (int userId in c.ResponsiveChain.Split(",").Select(Int32.Parse).ToList())
                        {
                            foreach (Users u in userContext.GetAllUser())
                            {
                                if (userId == u.UserId)
                                {
                                    chainViewModel.ResponsiveChain.Add(u);
                                }
                            }
                        }
                    }
                }
            }
            return View(chainViewModel);
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

            // creating a new chain
            Chain chain = new Chain { ChainOwner = id };
            chain.ChainId = chainContext.CreateChain(chain);

            HttpContext.Session.SetInt32("ChainId", chain.ChainId);
            HttpContext.Session.SetString("ChainStatus", "Efficient");

            return RedirectToAction("Index");
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

                    foreach (Chain chain in chainContext.GetAllChains())
                    {
                        if (chain.ChainOwner == user.UserId)
                        {
                            HttpContext.Session.SetInt32("ChainId", chain.ChainId);
                            HttpContext.Session.SetString("ChainStatus", chain.Status);
                        }
                    }
                    return RedirectToAction("Index");
                }
            }

            TempData["auth_error"] = "Email or password is invalid";
            return View();
        }

        public IActionResult Invoice()
        {
            List<Invoice> invoiceList = new List<Invoice>();
            foreach (Invoice i in invoiceContext.GetAllInvoice())
            {
                if (i.ChainId == HttpContext.Session.GetInt32("ChainId") && i.ChainStatus == HttpContext.Session.GetString("ChainStatus"))
                {
                    invoiceList.Add(i);
                }
            }
            return View(invoiceList);
        }


        public IActionResult CreateInv()
        {
            string userIdString = null;
            foreach (Chain c in chainContext.GetAllChains())
            {
                if (c.ChainId == HttpContext.Session.GetInt32("ChainId"))
                {
                    if (HttpContext.Session.GetString("ChainStatus") == "Efficient")
                    {
                        userIdString = c.EfficientChain;
                    }
                    else if (HttpContext.Session.GetString("ChainStatus") == "Responsive")
                    {
                        userIdString = c.ResponsiveChain;
                    }
                }
            }

            if (userIdString == null)
            {
                TempData["NoChain"] = "Add Intermediaries to your Chain";
                return RedirectToAction("Invoice", "Distributor");
            }

            List<Users> userList = new List<Users>();
            List<int> userIds = userIdString.Split(",").Select(Int32.Parse).ToList();
            foreach (Users u in userContext.GetAllUser())
            {
                foreach (int userId in userIds)
                {
                    if (userId == u.UserId)
                    {
                        userList.Add(u);
                    }
                }
            }
            return View(userList);
        }

        [HttpPost]
        public async Task<ActionResult> CreateInv(int receiverId, IFormFile file)
        {
            Invoice invoice = new Invoice { ChainId = Convert.ToInt32(HttpContext.Session.GetInt32("ChainId")), 
                                            SenderId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId")),
                                            ChainStatus = HttpContext.Session.GetString("ChainStatus"),
                                            ReceiverId = receiverId,
                                            UploadDate = DateTime.Now};

            // Find the filename extension of the file to be uploaded.
            string fileExt = Path.GetExtension(file.FileName);

            List<Invoice> invoiceList = invoiceContext.GetAllInvoice();
            foreach (Invoice i in invoiceContext.GetAllInvoice())
            {
                if (i.ChainId == HttpContext.Session.GetInt32("ChainId") && i.ChainStatus == HttpContext.Session.GetString("ChainStatus"))
                {
                    invoiceList.Add(i);
                }
            }

            string uploadedFile = "Invoice" + invoiceList.Count+1 + fileExt;
            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\invoice", uploadedFile);
            // Upload the file to server
            using (var fileSteam = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(fileSteam);
            }
            invoice.Filename = uploadedFile;
            invoiceContext.AddInvoice(invoice);

            return RedirectToAction("Invoice");
        }

        public IActionResult ViewInv(int id)
        {
            Invoice invoice = new Invoice();
            foreach (Invoice i in invoiceContext.GetAllInvoice())
            {
                if (i.InvoiceId == id)
                {
                    invoice = i;
                }
            }
            return View(invoice);
        }

        public ActionResult EditInv(int id)
        {
            Invoice invoice = new Invoice();
            foreach (Invoice i in invoiceContext.GetAllInvoice())
            {
                if (i.InvoiceId == id)
                {
                    invoice = i;
                }
            }

            return View(invoice);
        }

        [HttpPost]
        public async Task<ActionResult> EditInv(int id, IFormFile file)
        {
            Invoice invoice = new Invoice();
            foreach (Invoice i in invoiceContext.GetAllInvoice())
            {
                if (i.InvoiceId == id)
                {
                    invoice = i;
                    invoiceContext.UpdateInvoice(invoice.InvoiceId);
                }
            }

            // Find the filename extension of the file to be uploaded.
            string fileExt = Path.GetExtension(file.FileName);

            string uploadedFile = "Updated_Invoice" + invoice.InvoiceId + fileExt;
            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\invoice", uploadedFile);
            // Upload the file to server
            using (var fileSteam = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(fileSteam);
            }
            invoice.Filename = uploadedFile;
            invoiceContext.AddInvoice(invoice);

            return RedirectToAction("Invoice");
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
                    if (u.CompanyName.Contains(companyName) && u.ProductCategory.Contains(cat))
                    {
                        userList.Add(u);
                    }
                }
            }

            return View(userList);
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
            int senderId = Convert.ToInt32(formdata["senderId"]);
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

            viewModel.ChatId = chatId;
            return View(viewModel);
        }

        public IActionResult AddIntermediary(string chain)
        {
            AddIntermediaryViewModel viewModel = new AddIntermediaryViewModel { ChainType = chain };

            foreach (Users u in userContext.GetAllUser())
            {
                if (u.CompanyType != "Distributor")
                {
                    viewModel.UserList.Add(u);
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddIntermediary(IFormCollection formdata)
        {
            string status = formdata["chain"];
            int userId = Convert.ToInt32(formdata["userId"]);

            string chain = null;
            foreach (Chain c in chainContext.GetAllChains())
            {
                if (c.ChainId == HttpContext.Session.GetInt32("ChainId"))
                {
                    if (status == "Efficient")
                    {
                        chain = c.EfficientChain;
                    }
                    else if (status == "Responsive")
                    {
                        chain = c.ResponsiveChain;
                    }
                }
            }

            string newChain = null;
            List<int> supplierIds = new List<int>();

            if (chain != null)
            {
                supplierIds = chain.Split(',').Select(Int32.Parse).ToList();
                supplierIds.Add(userId);
                newChain = string.Join(",", supplierIds);
            }
            else
            {
                newChain = Convert.ToString(userId);
            }

            if (status == "Efficient")
            {
                chainContext.SetEfficientChain(newChain, Convert.ToInt32(HttpContext.Session.GetInt32("ChainId")));
            }
            else if (status == "Responsive")
            {
                chainContext.SetResponsiveChain(newChain, Convert.ToInt32(HttpContext.Session.GetInt32("ChainId")));
            }

            return RedirectToAction("Index");
        }

        public IActionResult DelEfficientIntermediary(int id)
        {
            List<int> userIds = new List<int>();
            string userIdString = null;
            foreach (Chain c in chainContext.GetAllChains())
            {
                if (c.ChainId == HttpContext.Session.GetInt32("ChainId"))
                {
                    userIdString = c.EfficientChain;
                }
            }

            userIds = userIdString.Split(',').Select(Int32.Parse).ToList();
            userIds.Remove(id);
            string newUserIdString = string.Join(",", userIds);
            chainContext.SetEfficientChain(newUserIdString, Convert.ToInt32(HttpContext.Session.GetInt32("ChainId")));
            return RedirectToAction("Index");
        }

        public IActionResult DelResponsiveIntermediary(int id)
        {
            List<int> userIds = new List<int>();
            string userIdString = null;
            foreach (Chain c in chainContext.GetAllChains())
            {
                if (c.ChainId == HttpContext.Session.GetInt32("ChainId"))
                {
                    userIdString = c.ResponsiveChain;
                }
            }

            userIds = userIdString.Split(',').Select(Int32.Parse).ToList();
            userIds.Remove(id);
            string newUserIdString = string.Join(",", userIds);
            chainContext.SetResponsiveChain(newUserIdString, Convert.ToInt32(HttpContext.Session.GetInt32("ChainId")));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SwitchType(IFormCollection formdata)
        {
            string status = formdata["status"];
            if (status == null)
            {
                status = "Efficient";
            }
            chainContext.SetStatus(status, Convert.ToInt32(HttpContext.Session.GetInt32("ChainId")));
            HttpContext.Session.SetString("ChainStatus", status);
            return RedirectToAction("Index");
        }
    }
    
}
