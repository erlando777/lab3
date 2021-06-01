using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication7.Models;
using Scrypt;
using System.Data.SqlClient;
using System.Web.Security;
using System.Net;

namespace WebApplication7.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private Entities db = new Entities();
        // GET: Admin
        public bool Check()
        {
            if (System.Web.HttpContext.Current.Session["Role"] == null)
            {
                return false;
            }
            if (Session["Role"].ToString() == "1")
            {
                return true;
            }
            return false;
        }
        public ActionResult AddRole()
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            ViewBag.UserID = new SelectList(db.Users.Where(x => x.Role == null), "UserID", "Surname");
            var i = 0;
            foreach (var person in ViewBag.UserID)
            {
                i++;
            }
            if (i == 0)
            {
                ViewBag.Error = "Visi vartotojai jau turi roles.";
            }
            //ViewBag.Rolele = new SelectList<int>() { 1, 2, 3 };
            //Assigning generic list to ViewBag
            return View();
        }

        // POST: Roles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRole([Bind(Include = "RoleConnectionID,UserID,Role1")] Role role)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            if (ModelState.IsValid)
            {
                db.Roles.Add(role);
                db.SaveChanges();
                var user = db.Users.Where(x => x.UserID == role.UserID).First();
                db.Users.Where(x => x.UserID == user.UserID).First().Role = role.Role1;
                db.SaveChanges();
                return RedirectToAction("Index", "Home") ;
            }
            ViewBag.UserID = new SelectList(db.Users.Where(x => x.Role == null), "UserID", "Surname", role.UserID);
            return View(role);
        }
        public ActionResult AddModule()
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            if (Session["Role"] == null || Session["Role"].ToString() == "3" || Session["Role"].ToString() == "2")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        // POST: Modules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddModule([Bind(Include = "ModuleID,ModuleCode,ModuleTitle,UserID")] Module module)
        {
            if (ModelState.IsValid)
            {
                if (db.Modules.Any(s => s.ModuleCode.Equals(module.ModuleCode)))
                {
                    ViewBag.Error = "Modulis su tokiu kodu jau yra pridėtas.";
                    return View(module);
                }
                module.UserID = ((int)Session["UserID"]);
                db.Modules.Add(module);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(module);
        }
        [Authorize]
        public ActionResult Users()
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            var id = ((int)Session["UserID"]);
            var users = db.Users.Where(x => x.UserID != id).Where(x=>x.Role != 1);
            return View(users.ToList());
        }
        public ActionResult DeleteUser(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var userC = db.Users.Where(x => x.UserID == id).FirstOrDefault();
            if(userC.Role == 3)
            {
                while (true)
                {
                    StudentGrade studentGrade = db.StudentGrades.Where(x => x.StudentID == id).FirstOrDefault();
                    if (studentGrade != null)
                    {
                        db.StudentGrades.Remove(studentGrade);
                        db.SaveChanges();
                    }
                    else
                    {
                        break;
                    }
                }
                while (true)
                {
                    StudentModule studentModule = db.StudentModules.Where(s => s.StudentID == id).FirstOrDefault();
                    if (studentModule != null)
                    {
                        db.StudentModules.Remove(studentModule);
                        db.SaveChanges();
                    }
                    else
                    {
                        break;
                    }
                }
                while (true)
                {
                    Role role = db.Roles.Where(x => x.UserID == id).FirstOrDefault();
                    if(role != null)
                    {
                        db.Roles.Remove(role);
                        db.SaveChanges();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (userC.Role == 2)
            {
                while (true)
                {
                    Module module = db.Modules.Where(s => s.UserID == id).FirstOrDefault();
                    if (module != null)
                    {
                        while (true)
                        {
                            StudentGrade studentGrade = db.StudentGrades.Where(s => s.ModuleID == module.ModuleID).FirstOrDefault();
                            if (studentGrade != null)
                            {
                                db.StudentGrades.Remove(studentGrade);
                                db.SaveChanges();
                            }
                            else
                            {
                                break;
                            }

                        }
                        while (true)
                        {
                            GradeTitle titles = db.GradeTitles.Where(s => s.ModuleID == module.ModuleID).FirstOrDefault();
                            if (titles != null)
                            {
                                db.GradeTitles.Remove(titles);
                                db.SaveChanges();
                            }
                            else
                            {
                                break;
                            }

                        }
                        while (true)
                        {
                            Lecture lecture = db.Lectures.Where(s => s.ModuleID == module.ModuleID).FirstOrDefault();
                            if(lecture != null)
                            {
                                db.Lectures.Remove(lecture);
                                db.SaveChanges();
                            }
                            else
                            {
                                break;
                            }
 
                        }
                        while (true)
                        {
                            StudentModule studentModule = db.StudentModules.Where(x => x.LecturerID == id).FirstOrDefault();
                            if(studentModule != null)
                            {
                                db.StudentModules.Remove(studentModule);
                                db.SaveChanges();
                            }
                            else
                            {
                                break;
                            }
                        }
                        db.Modules.Remove(module);
                        db.SaveChanges();
                    }
                    else
                    {
                        break;
                    }
                }
                while (true)
                {
                    Role role = db.Roles.Where(x => x.UserID == id).FirstOrDefault();
                    if (role != null)
                    {
                        db.Roles.Remove(role);
                        db.SaveChanges();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Users");
        }
        public ActionResult Modules()
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            var id = ((int)Session["UserID"]);
            return View(db.Modules.Where(x => x.UserID == id).ToList());
        }
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}