using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication7.Models;
using Scrypt;
using System.Data.SqlClient;
using System.Web.Security;

namespace WebApplication7.Controllers
{
    public class HomeController : Controller
    {
        private Entities db = new Entities();
        public ActionResult Register()
        {
            if (Session["Username"] != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Register(User user)
        {
            ScryptEncoder encoder = new ScryptEncoder();
            /* var registeredUser = from c in db.Users
                                  where c.Username.Equals(user.Username)
                                  select c;
             if(registeredUser != null)
             {
                 ViewBag.Error = "This username already registered";
                 return View();
             }*/
            using (Entities db = new Entities())
            {
                if(user.Age < 18 || user.Age > 100)
                {
                    ViewBag.Error = "Invalid age";
                    return View();
                }
                var registeredEmail = db.Users.Where(x => x.Email == user.Email).FirstOrDefault();
                if (registeredEmail != null)
                {
                    ViewBag.Error = "This email already registered";
                    return View();
                }
                if (user.Password == user.ConfirmPassword)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                else
                {
                    ViewBag.Error = "Slaptazodziai nesutampa.";
                    return View();
                }

            }
            ViewBag.Error = "Sėkminga registracija. Prisijunkite.";

            return View();
        }
        public ActionResult Login()
        {
            if (Session["Username"] != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Login(User user)
        {
            var validUser = db.Users.Where(x => x.Email == user.Email).FirstOrDefault();
            if (validUser == null) {
                ViewBag.Error = "Email or password is invalid.";
                return View();
            }
            bool isvalidUser = BCrypt.Net.BCrypt.Verify(user.Password, validUser.Password);
            if (isvalidUser)
            {
                Session["Username"] = validUser.Name;
                Session["Role"] = validUser.Role;
                Session["UserID"] = validUser.UserID;
                FormsAuthentication.SetAuthCookie(validUser.Name, false);
                return View("Index");
            }
            else
            {
                ViewBag.Error = "Email or password is invalid";
            }
            FormsAuthentication.SetAuthCookie(validUser.Name, false);
            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "ModuleCode");
            ViewBag.StudentID = new SelectList(db.Users.Where(x => x.Role == 3), "UserID", "Username");
            return View();
        }

        // POST: StudentModules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ConnectionID,StudentID,ModuleID,LecturerID")] StudentModule studentModule)
        {
            if (ModelState.IsValid)
            {
                studentModule.LecturerID = ((int)Session["UserID"]);
                db.StudentModules.Add(studentModule);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "ModuleCode", studentModule.ModuleID);
            ViewBag.StudentID = new SelectList(db.Users.Where(x => x.Role == 3), "UserID", "Username", studentModule.StudentID);
            return View(studentModule);
        }
        public ActionResult Index()
        {
            return View();
        }

    }
}