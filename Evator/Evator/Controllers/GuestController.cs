using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Evator.Models;
using Evator.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Evator.Controllers
{
    public class GuestController : Controller
    {
        private readonly EvatorContext db = new EvatorContext();
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GuestController(IHostingEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            string RoleType = GetCookie("Role");
            switch (RoleType)
            {
                case "1":
                    return RedirectToAction("Index", "Admin");
                case "2":
                    return RedirectToAction("Index", "User");
                case null:
                    return RedirectToAction("Login", "Home");
            }

            ViewBag.Username = GetCookie("Username");
            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            string RoleType = GetCookie("Role");
            switch (RoleType)
            {
                case "1":
                    return RedirectToAction("Index", "Admin");
                case "2":
                    return RedirectToAction("Index", "User");
                case null:
                    return RedirectToAction("Login", "Home");
            }
            ViewBag.Username = GetCookie("Username");

            Accounts myAccount = db.Accounts.FirstOrDefault(c => c.Email == GetCookie("Email"));
            RegisterViewModel myProfile = new RegisterViewModel
            {
                Name = myAccount.AtName,
                Agency = myAccount.Department,
                Email = myAccount.Email,
                Password = myAccount.AtPassword
            };
            if (myAccount.AtProfile == null)
            {
                ViewBag.ProfilePath = "defaultprofile.png";
            }
            else ViewBag.ProfilePath = myAccount.AtProfile;
            return View("Profile", myProfile);
        }

        [HttpPost]
        public IActionResult Profile(RegisterViewModel newData, IFormFile image)
        {
            Accounts thisAccount = db.Accounts.FirstOrDefault(c => c.Email == GetCookie("Email"));
            if (ModelState.IsValid)
            {
                SetCookie("Email", newData.Email, 30);
                SetCookie("Username", newData.Name, 30);
                string imagepath = thisAccount.AtProfile;

                if (image != null)
                {
                    string myAccountPath = _hostingEnvironment.WebRootPath + "/assets/accounts/";

                    if (imagepath != null)
                    {
                        string filename = myAccountPath + imagepath;
                        FileInfo file = new FileInfo(filename);
                        file.Delete();
                    }

                    imagepath = DateTime.Now.ToString();
                    imagepath = imagepath.Replace(" ", "").Replace(":", "").Replace("/", "") + Path.GetExtension(image.FileName);
                    string upload = myAccountPath + imagepath;
                    image.CopyTo(new FileStream(upload, FileMode.Create));
                }

                thisAccount.AtName = newData.Name;
                thisAccount.Department = newData.Agency;
                thisAccount.Email = newData.Email;
                thisAccount.AtPassword = newData.Password;
                thisAccount.AtProfile = imagepath;
                db.Accounts.Update(thisAccount);
                db.SaveChanges();
                return RedirectToAction("Index", "Guest");
            }

            ViewBag.Username = GetCookie("Username");
            if (thisAccount.AtProfile == null)
            {
                ViewBag.ProfilePath = "defaultprofile.png";
            }
            else ViewBag.ProfilePath = thisAccount.AtProfile;
            return View("Profile", newData);
        }

        public IActionResult EventList(int Initial, string Query)
        {
            string RoleType = GetCookie("Role");
            switch (RoleType)
            {
                case "1":
                    return RedirectToAction("Index", "Admin");
                case "2":
                    return RedirectToAction("Index", "User");
                case null:
                    return RedirectToAction("Login", "Home");
            }

            ViewBag.Username = GetCookie("Username");

            AtToken myToken = db.AtToken.FirstOrDefault(c => c.Token == GetCookie("Token"));
            var q1 = db.Invite.Where(c => c.AtId == myToken.AtId).Select(s => s.EtId);
            var q2 = db.Attend.Where(c => c.AtId == myToken.AtId).Select(s => s.EtId);
            var q3 = q1.Concat(q2).Distinct().ToList();
            List<Events> events = new List<Events>();
            foreach(var i in q3)
            {
                Events myEvent = db.Events.FirstOrDefault(c => c.EtId == i);
                events.Add(myEvent);
            }

            if (Initial < 0)
            {
                Initial = 0;
                ViewBag.L2 = Initial + 10;
            }

            if (Query != null)
            {
                events = events.Where(c => c.EtName.Contains(Query, StringComparison.OrdinalIgnoreCase)).ToList();
                ViewBag.Query = Query;
            }

            int total = events.Count();
            ViewBag.Limit = total;
            int pages = total / 10;
            int remainder = total - pages * 10;
            var builder = events.Take(10 + Initial);
            var display = builder.TakeLast(10);

            ViewBag.L2 = Initial + 10;

            if (Initial == pages * 10)
            {
                display = builder.TakeLast(remainder);
                ViewBag.L2 = Initial + remainder;
            }

            ViewBag.Initial = Initial;

            if (total == 0) ViewBag.L1 = 0;
            else ViewBag.L1 = Initial + 1;
            ViewBag.L3 = total;

            string link = Request.Scheme + "://" + Request.Host + Url.Action("Event", "Home") + "?Code=";
            ViewBag.Link = link;
            return View(display);
        }

        public IActionResult AttendancePDF()
        {
            return View();
        }

        public IActionResult Logout()
        {
            AtToken RemovedToken = db.AtToken.FirstOrDefault(c => c.Token == GetCookie("Token"));
            db.AtToken.Remove(RemovedToken);
            db.SaveChanges();
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            return RedirectToAction("Login", "Home");
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
    }
}