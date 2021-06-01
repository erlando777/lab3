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
using System.Data.Entity;


namespace WebApplication7.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        // GET: Student
        private Entities db = new Entities();
        public bool Check()
        {
            if (System.Web.HttpContext.Current.Session["Role"] == null)
            {
                return false;
            }
            if (Session["Role"].ToString() == "3")
            {
                return true;
            }
            return false;
        }
        [Authorize]
        public ActionResult Modules()
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            var id = ((int)Session["UserID"]);
            var studentModules = db.StudentModules.Where(s => s.StudentID == id);
            return View(studentModules.ToList());
        }
        [Authorize]
        public ActionResult ModuleLectures(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            var moduleTitle = db.Modules.Find(id).ModuleTitle;
            var lectures = db.Lectures.Where(s => s.Module.ModuleTitle == moduleTitle);
            ViewBag.Message = moduleTitle;
            ViewBag.lecture = lectures;
            return View(lectures.ToList());
        }
        public ActionResult StudentGrades(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            var studentas = ((int)Session["UserID"]);
            var studentGrades = db.StudentGrades.Include(s => s.GradeTitle).Include(s => s.Module).Where(x => x.ModuleID == id).Include(s => s.User).Where(x => x.StudentID == studentas);
            ViewBag.Module = (db.Modules.Where(x => x.ModuleID == id).FirstOrDefault()).ModuleTitle;
            return View(studentGrades.ToList());
        }
    }
}