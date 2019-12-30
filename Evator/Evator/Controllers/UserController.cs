using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Evator.Models;
using Evator.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace Evator.Controllers
{
    public class UserController : Controller
    {
        private readonly EvatorContext db = new EvatorContext();
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(IHostingEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
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
                case "3":
                    return RedirectToAction("Index", "Guest");
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
                case "3":
                    return RedirectToAction("Index", "Guest");
                case null:
                    return RedirectToAction("Login", "Home");
            }
            ViewBag.Username = GetCookie("Username");

            Accounts myAccount = db.Accounts.FirstOrDefault(c => c.Email == GetCookie("Email"));
            ProfileViewModel myProfile = new ProfileViewModel
            {
                Name = myAccount.AtName,
                EmployeeID = (int)myAccount.EmployeeId,
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
        public IActionResult Profile(ProfileViewModel newData, IFormFile image)
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
                thisAccount.EmployeeId = newData.EmployeeID;
                thisAccount.Department = newData.Agency;
                thisAccount.Email = newData.Email;
                thisAccount.AtPassword = newData.Password;
                thisAccount.AtProfile = imagepath;
                db.Accounts.Update(thisAccount);
                db.SaveChanges();
                return RedirectToAction("Index", "User");
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
                case "3":
                    return RedirectToAction("Index", "Guest");
                case null:
                    return RedirectToAction("Login", "Home");
            }

            ViewBag.Username = GetCookie("Username");

            AtToken myToken = db.AtToken.FirstOrDefault(c => c.Token == GetCookie("Token"));
            var q1 = db.Invite.Where(c => c.AtId == myToken.AtId).Select(s => s.EtId);
            var q2 = db.Attend.Where(c => c.AtId == myToken.AtId).Select(s => s.EtId);
            var q3 = q1.Concat(q2).Distinct().ToList();
            List<Events> events = new List<Events>();
            foreach (var i in q3)
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

        public IActionResult ManageEvents(int Initial, string Query)
        {
            string RoleType = GetCookie("Role");
            switch (RoleType)
            {
                case "1":
                    return RedirectToAction("Index", "Admin");
                case "3":
                    return RedirectToAction("Index", "Guest");
                case null:
                    return RedirectToAction("Login", "Home");
            }
            ViewBag.Username = GetCookie("Username");

            if (Initial < 0)
            {
                Initial = 0;
                ViewBag.L2 = Initial + 10;
            }

            var identity = db.AtToken.FirstOrDefault(c => c.Token == GetCookie("Token"));
            var events = db.Events.Where(c => c.OwnerId == identity.AtId).ToList();

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

        public IActionResult Generated(string myText)
        {
            var bitmapBytes = GenerateQR(myText);
            return File(bitmapBytes, "image/jpeg");
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            string RoleType = GetCookie("Role");
            switch (RoleType)
            {
                case "1":
                    return RedirectToAction("Index", "Admin");
                case "3":
                    return RedirectToAction("Index", "Guest");
                case null:
                    return RedirectToAction("Login", "Home");
            }
            ViewBag.Username = GetCookie("Username");
            AtToken myToken = db.AtToken.FirstOrDefault(c => c.Token == GetCookie("Token"));
            ViewBag.MyId = myToken.AtId;
            return View();
        }

        [HttpPost]
        public IActionResult CreateEvent(Events newData, IFormFile image)
        {
            if(ModelState.IsValid)
            {
                string imagepath = null;
                if(image != null)
                {
                    imagepath = DateTime.Now.ToString();
                    imagepath = imagepath.Replace(" ", "").Replace(":", "").Replace("/", "") + Path.GetExtension(image.FileName);

                    string myEventPath = "/assets/events/";
                    string upload = _hostingEnvironment.WebRootPath + myEventPath + imagepath;
                    image.CopyTo(new FileStream(upload, FileMode.Create));
                }

                StringBuilder token = new StringBuilder();
                Random random = new Random();
                char ch;
                for (int i = 0; i < 50; i++)
                {
                    ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                    token.Append(ch);
                }

                string qrinvite = DateTime.Now.ToString().Replace(" ", "").Replace(":", "").Replace("/", "");
                string qrattend = token.ToString();

                Events myEvent = new Events
                {
                    OwnerId = newData.OwnerId,
                    EtName = newData.EtName,
                    Speaker = newData.Speaker,
                    EtLocation = newData.EtLocation,
                    DateStart = newData.DateStart,
                    DateEnd = newData.DateEnd,
                    TimeStart = newData.TimeStart,
                    TimeEnd = newData.TimeEnd,
                    EtDescription = newData.EtDescription,
                    QrInvite = qrinvite,
                    QrAttend = qrattend,
                    Banner = imagepath,
                    EtStatus = 0
                };

                db.Events.Add(myEvent);
                db.SaveChanges();
                return RedirectToAction("ManageEvents");
            }
            return View("CreateEvent", newData);
        }

        [HttpGet]
        public IActionResult EditEvent(int id)
        {
            string RoleType = GetCookie("Role");
            switch (RoleType)
            {
                case "1":
                    return RedirectToAction("Index", "Admin");
                case "3":
                    return RedirectToAction("Index", "Guest");
                case null:
                    return RedirectToAction("Login", "Home");
            }
            ViewBag.Username = GetCookie("Username");

            Events myEvent = db.Events.FirstOrDefault(c => c.EtId == id);
            SetCookie("TempCode", myEvent.QrInvite, 20);
            if (myEvent.Banner == null)   
            {
                ViewBag.Banner = "defaultevent.png";
            }
            else ViewBag.Banner = myEvent.Banner;
            ViewBag.DateStart = myEvent.DateStart.ToShortDateString();
            ViewBag.DateEnd = myEvent.DateEnd.ToShortDateString();
            return View(myEvent);
        }

        [HttpPost]
        public IActionResult EditEvent(Events newData, IFormFile image)
        {
            Events thisEvent = db.Events.FirstOrDefault(c => c.QrInvite == GetCookie("TempCode"));

            if (ModelState.IsValid)
            {
                string imagepath = thisEvent.Banner;

                if (image != null)
                {
                    string myEventPath = _hostingEnvironment.WebRootPath + "/assets/events/";

                    if (imagepath != null)
                    {
                        string filename = myEventPath + imagepath;
                        FileInfo file = new FileInfo(filename);
                        file.Delete();
                    }

                    imagepath = DateTime.Now.ToString();
                    imagepath = imagepath.Replace(" ", "").Replace(":", "").Replace("/", "") + Path.GetExtension(image.FileName);
                    string upload = myEventPath + imagepath;
                    image.CopyTo(new FileStream(upload, FileMode.Create));
                }

                thisEvent.OwnerId = newData.OwnerId;
                thisEvent.EtName = newData.EtName;
                thisEvent.Speaker = newData.Speaker;
                thisEvent.EtLocation = newData.EtLocation;
                thisEvent.DateStart = newData.DateStart;
                thisEvent.DateEnd = newData.DateEnd;
                thisEvent.TimeStart = newData.TimeStart;
                thisEvent.TimeEnd = newData.TimeEnd;
                thisEvent.EtDescription = newData.EtDescription;
                thisEvent.Banner = imagepath;

                db.Events.Update(thisEvent);
                db.SaveChanges();
                return RedirectToAction("ManageEvents");
            }

            ViewBag.Username = GetCookie("Username");
            ViewBag.DateStart = newData.DateStart.ToShortDateString();
            ViewBag.DateEnd = newData.DateEnd.ToShortDateString();
            return RedirectToAction("EditEvent", newData);
        }

        public IActionResult EventStatus(int id)
        {
            Events myEvent = db.Events.FirstOrDefault(c => c.EtId == id);
            if (myEvent.EtStatus == 0)
            {
                myEvent.EtStatus = 1;
            }
            else myEvent.EtStatus = 0;
            db.Events.Update(myEvent);
            db.SaveChanges();

            return RedirectToAction("ManageEvents");
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

        public byte[] GenerateQR(string myText)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(myText, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            var bitmapBytes = BitmapToBytes(qrCodeImage);
            return bitmapBytes;
        }

        private static byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
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
