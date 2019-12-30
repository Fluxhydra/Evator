using System;
using System.IO;
using System.Linq;
using System.Text;
using Evator.Models;
using Evator.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Evator.Controllers
{
    public class HomeController : Controller
    {
        private readonly EvatorContext db = new EvatorContext();
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHostingEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Accounts newData)
        {
            Accounts myAccount = db.Accounts.FirstOrDefault(c => c.Email == newData.Email && c.AtPassword == newData.AtPassword);

            if(myAccount != null)
            {
                StringBuilder token = new StringBuilder();
                Random random = new Random();
                char ch;
                for (int i = 0; i < 50; i++)
                {
                    ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                    token.Append(ch);
                }

                string timestamp = DateTime.Now.ToString().Replace(" ", "").Replace(":", "").Replace("/", "");

                AtToken myToken = new AtToken
                {
                    Token = token + timestamp,
                    AtId = myAccount.AtId,
                    TnStatus = myAccount.RoleType
                };

                AtToken previousToken = db.AtToken.FirstOrDefault(c => c.AtId == myAccount.AtId);
                if(previousToken != null)
                {
                    db.AtToken.Remove(previousToken);
                    db.SaveChanges();
                }

                db.AtToken.Add(myToken);
                db.SaveChanges();

                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }

                SetCookie("Token", myToken.Token, 30);
                SetCookie("Username", myAccount.AtName, 30);
                SetCookie("Role", myToken.TnStatus.ToString(), 30);
                SetCookie("Email", myAccount.Email, 30);

                return RedirectToAction("Authentication");
            }
            return RedirectToAction("Login");
        }

        public IActionResult Authentication()
        {
            string RoleType = GetCookie("Role");
            switch (RoleType)
            {
                case "1":
                    return RedirectToAction("Index", "Admin");
                case "2":
                    return RedirectToAction("Index", "User");
                case "3":
                    return RedirectToAction("Index", "Guest");
                default:
                    return RedirectToAction("Login", "Home");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel newData, IFormFile image)
        {
            Accounts thisAccount = db.Accounts.FirstOrDefault(c => c.Email == newData.Email);
            if (thisAccount != null)
            {
                return View("Register", newData);
            }

            if (ModelState.IsValid)
            {
                string imagepath = null;
                
                if (image != null)
                {
                    imagepath = DateTime.Now.ToString();
                    imagepath = imagepath.Replace(" ", "").Replace(":", "").Replace("/", "") + Path.GetExtension(image.FileName);

                    string myAccountPath = "/assets/accounts/";
                    string upload = _hostingEnvironment.WebRootPath + myAccountPath + imagepath;
                    image.CopyTo(new FileStream(upload, FileMode.Create));
                }

                Accounts myAccount = new Accounts
                {
                    AtName = newData.Name,
                    EmployeeId = 0,
                    Department = newData.Agency,
                    Email = newData.Email,
                    AtPassword = newData.Password,
                    AtProfile = imagepath,
                    RoleType = newData.Role
                };
                db.Accounts.Add(myAccount);
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            return View("Register", newData);
        }

        public IActionResult Event(string Code)
        {
            string RoleType = GetCookie("Role");
            if (RoleType == null)
            {
                return RedirectToAction("Login");
            }
            if (RoleType == "1")
            {
                return RedirectToAction("Index", "Admin");
            }
            if (Code == null)
            {
                switch(RoleType)
                {
                    case "2":
                        return RedirectToAction("Index", "User");
                    case "3":
                        return RedirectToAction("Index", "Guest");
                }
            }

            AtToken myToken = db.AtToken.FirstOrDefault(c => c.Token == GetCookie("Token"));
            bool ch = char.IsNumber(Code[0]);

            if(ch == true)
            {
                Events myEvent = db.Events.FirstOrDefault(c => c.QrInvite == Code);
                if(myEvent == null)
                {
                    return Content("Debug: The Invite QR that you've requested is not valid.");
                }

                Invite CheckIE = db.Invite.FirstOrDefault(c => c.EtId == myEvent.EtId && c.AtId == myToken.AtId);
                if(CheckIE != null)
                {
                    switch (RoleType)
                    {
                        case "2":
                            return RedirectToAction("Index", "User");
                        case "3":
                            return RedirectToAction("Index", "Guest");
                    }
                }

                Invite myInvite = new Invite();
                myInvite.AtId = myToken.AtId;
                myInvite.EtId = myEvent.EtId;
                db.Invite.Add(myInvite);
                db.SaveChanges();

                switch (RoleType)
                {
                    case "2":
                        return RedirectToAction("Index", "User");
                    case "3":
                        return RedirectToAction("Index", "Guest");
                }
            }

            Events thisEvent = db.Events.FirstOrDefault(c => c.QrAttend == Code);
            if (thisEvent == null)
            {
                return Content("Debug: The Attend QR that you've requested is not valid.");
            }

            Attend CheckAD = db.Attend.FirstOrDefault(c => c.AtId == myToken.AtId && c.EtId == thisEvent.EtId);
            if(CheckAD != null)
            {
                switch (RoleType)
                {
                    case "2":
                        return RedirectToAction("Index", "User");
                    case "3":
                        return RedirectToAction("Index", "Guest");
                }
            }

            Attend myAttend = new Attend();
            myAttend.AtId = myToken.AtId;
            myAttend.EtId = thisEvent.EtId;
            myAttend.AdRecord = DateTime.Now;
            db.Attend.Add(myAttend);
            db.SaveChanges();

            switch (RoleType)
            {
                case "2":
                    return RedirectToAction("Index", "User");
                case "3":
                    return RedirectToAction("Index", "Guest");
            }
            return View();
        }

        public string GetCookie(string key)
        {
            return Request.Cookies[key];
        }

        public void SetCookie(string key, string value, double? expireTime)
        {
            CookieOptions option = new CookieOptions
            {
                IsEssential = true
            };

            if (expireTime.HasValue)
            {
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            }
            else option.Expires = DateTime.Now.AddMilliseconds(1);

            Response.Cookies.Append(key, value, option);
        }

        public void RemoveCookie(string key)
        {
            Response.Cookies.Delete(key);
        }
    }
}
