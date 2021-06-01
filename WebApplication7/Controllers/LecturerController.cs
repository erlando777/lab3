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
    public class LecturerController : Controller
    {

        private Entities db = new Entities();
        [Authorize]
        public bool Check()
        {
            if(System.Web.HttpContext.Current.Session["Role"] == null)
            {
                return false; 
            }
            if (Session["Role"].ToString() == "2")
            {
                return true;
            }
            return false;
        }

        public ActionResult AddModule()
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            return View();
        }
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
        public ActionResult Modules()
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            var id = ((int)Session["UserID"]);
            return View(db.Modules.Where(x => x.UserID == id).ToList());
        }
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
        public ActionResult EditModule(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Module module = db.Modules.Find(id);
            if (module == null)
            {
                return HttpNotFound();
            }
            return View(module);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditModule([Bind(Include = "ModuleID,ModuleCode,ModuleTitle,UserID")] Module module)
        {
            if (ModelState.IsValid)
            {
                db.Entry(module).State = EntityState.Modified;
                module.UserID = ((int)Session["UserID"]);
                db.SaveChanges();
                return RedirectToAction("Modules");
            }
            return View(module);
        }
        public ActionResult Students(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            //Session.Remove("GradeModule");
            Session["GradeModule"] = id;
            var students = new List<User>();
           // var students = db.Users.Where(x => x.Role == 2).ToList();

            foreach(var student in db.StudentModules)
            {
                 if(student.ModuleID == id)
                {
                    var studentID = student.StudentID;
                    students.Add(db.Users.Where(x => x.UserID == studentID).FirstOrDefault());
                }
            }
            if(students == null)
            {
                return RedirectToAction("Modules");
            }
            return View(students);
        }
        public ActionResult EditLecture(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lecture lecture = db.Lectures.Find(id);
            if (lecture == null)
            {
                return HttpNotFound();
            }

            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "ModuleTitle", lecture.ModuleID);
            return View(lecture);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditLecture([Bind(Include = "ID,ModuleID,LectureTitle,LectureActivity")] Lecture lecture)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lecture).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Modules");
            }
            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "ModuleTitle", lecture.ModuleID);
            return View(lecture);
        }
        public ActionResult DeleteLecture(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lecture lecture = db.Lectures.Where(x => x.ID == id).FirstOrDefault();
            if (lecture == null)
            {
                return HttpNotFound();
            }
            db.Lectures.Remove(lecture);
            db.SaveChanges();
            return RedirectToAction("Modules");
        }
        public ActionResult AddStudent()
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            var id = ((int)Session["UserID"]);
            var module = db.Modules.Where(x => x.UserID == id).FirstOrDefault();
            if (module == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var student = db.Users.Where(x => x.Role == 3).FirstOrDefault();
            if(student == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ModuleID = new SelectList(db.Modules.Where(x=>x.UserID == id), "ModuleID", "ModuleTitle");
            ViewBag.StudentID = new SelectList(db.Users.Where(x => x.Role == 3), "UserID", "Surname");
            return View();
        }

        // POST: StudentModules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStudent([Bind(Include = "ConnectionID,StudentID,ModuleID,LecturerID")] StudentModule studentModule)
        {
            var id = ((int)Session["UserID"]);
            if (ModelState.IsValid)
            {
                if (db.StudentModules.Any(s => s.ModuleID.Equals(studentModule.ModuleID) && s.StudentID.Equals(studentModule.StudentID)))
                {
                    ViewBag.Error = "Toks modulis šiam studentui jau yra pridėtas.";
                    ViewBag.ModuleID = new SelectList(db.Modules.Where(x => x.UserID== id), "ModuleID", "ModuleTitle", studentModule.ModuleID);
                    ViewBag.StudentID = new SelectList(db.Users.Where(x => x.Role == 3), "UserID", "Surname", studentModule.StudentID);
                    
                    return View(studentModule);
                }
                studentModule.LecturerID = ((int)Session["UserID"]);
                db.StudentModules.Add(studentModule);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
           
            ViewBag.ModuleID = new SelectList(db.Modules.Where(x=>x.UserID == id), "ModuleID", "ModuleTitle", studentModule.ModuleID);
            ViewBag.StudentID = new SelectList(db.Users.Where(x => x.Role == 3), "UserID", "Surname", studentModule.StudentID);
            return View(studentModule);
        }
        public ActionResult AddLecture()
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");

            var id = ((int)Session["UserID"]);
            var module = db.Modules.Where(x => x.UserID == id).FirstOrDefault();
            if(module == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ModuleID = new SelectList(db.Modules.Where(x=>x.UserID == id), "ModuleID", "ModuleTitle");
            return View();
        }

        // POST: Lectures/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddLecture([Bind(Include = "ID,ModuleID,LectureTitle,LectureActivity")] Lecture lecture)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            var id = ((int)Session["UserID"]);
            if (ModelState.IsValid)
            {
                db.Lectures.Add(lecture);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ModuleID = new SelectList(db.Modules.Where(x=>x.UserID == id), "ModuleID", "ModuleTitle", lecture.ModuleID);
            return View(lecture);
        }
        public ActionResult DeleteModule(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Module module = db.Modules.Where(x => x.ModuleID == id).FirstOrDefault();
            if (module == null)
            {
                return HttpNotFound();
            }
            return View(module);
        }
        [HttpPost, ActionName("DeleteModule")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteModuleConfirmed(int id)
        {
            while (true)
            {
                StudentGrade grade = db.StudentGrades.Where(x => x.ModuleID == id).FirstOrDefault();
                if (grade != null)
                {
                    db.StudentGrades.Remove(grade);
                    db.SaveChanges();
                }
                else
                {
                    break;
                }
            }
            while (true)
            {
                GradeTitle title = db.GradeTitles.Where(x => x.ModuleID == id).FirstOrDefault();
                if (title != null)
                {
                    db.GradeTitles.Remove(title);
                    db.SaveChanges();
                }
                else
                {
                    break;
                }
            }
            while (true)
                {
                    Lecture lectures = db.Lectures.Where(x => x.ModuleID == id).FirstOrDefault();
                    if (lectures != null)
                    {
                        db.Lectures.Remove(lectures);
                        db.SaveChanges();
                    }
                    else
                    {
                        break;
                    }
                }
                while (true)
                {
                    StudentModule studentModules = db.StudentModules.Where(x => x.ModuleID == id).FirstOrDefault();
                    if (studentModules != null)
                    {
                        db.StudentModules.Remove(studentModules);
                        db.SaveChanges();
                    }
                    else
                    {
                        break;
                    }
                }
            StudentModule studentModule = db.StudentModules.Where(x => x.StudentID == id).FirstOrDefault();
            Module module = db.Modules.Where(x => x.ModuleID == id).FirstOrDefault();
            db.Modules.Remove(module);
            db.SaveChanges();
            return RedirectToAction("Modules");
        }
        public ActionResult DeleteFromModule(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentModule user = db.StudentModules.Where(x => x.StudentID == id).FirstOrDefault();
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        [HttpPost, ActionName("DeleteFromModule")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StudentModule studentModule = db.StudentModules.Where(x => x.StudentID == id).FirstOrDefault();
            db.StudentModules.Remove(studentModule);
            db.SaveChanges();
            return RedirectToAction("Modules");
        }
        public ActionResult AddGradeTitle(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            ViewBag.ModuleID = new SelectList(db.Modules.Where(x=>x.ModuleID == id), "ModuleID", "ModuleTitle");
            return View();
        }

        // POST: GradeTitles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddGradeTitle([Bind(Include = "GradeTitleID,ModuleID,GradeTitle1")] GradeTitle gradeTitle)
        {
            if (ModelState.IsValid)
            {
                db.GradeTitles.Add(gradeTitle);
                db.SaveChanges();
                return RedirectToAction("Modules");
            }

            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "ModuleTitle", gradeTitle.ModuleID);
            return View(gradeTitle);
        }
        public ActionResult GradeTitles(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(db.GradeTitles.Where(x=>x.ModuleID == id).ToList());
        }
        public ActionResult AddGradeForStudent(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            var lol = ((int)Session["GradeModule"]);
            ViewBag.GradeTitleID = new SelectList(db.GradeTitles.Where(x=>x.ModuleID == lol), "GradeTitleID", "GradeTitle1");
            ViewBag.ModuleID = new SelectList(db.Modules.Where(x=>x.ModuleID == ((int) Session["GradeModule"])), "ModuleID", "ModuleCode");
            ViewBag.StudentID = new SelectList(db.Users.Where(x=>x.UserID == id), "UserID", "Name");
            
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddGradeForStudent([Bind(Include = "GradeID,ModuleID,LecturerID,StudentID,GradeTitleID,Grade")] StudentGrade studentGrade)
        {
            if (ModelState.IsValid)
            {
                studentGrade.LecturerID = ((int)Session["UserID"]);
                studentGrade.ModuleID = ((int)Session["GradeModule"]);
                db.StudentGrades.Add(studentGrade);
                db.SaveChanges();
                return RedirectToAction("Modules");
            }

            ViewBag.GradeTitleID = new SelectList(db.GradeTitles, "GradeTitleID", "GradeTitle1", studentGrade.GradeTitleID);
            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "ModuleCode", studentGrade.ModuleID);
            ViewBag.StudentID = new SelectList(db.Users, "UserID", "Name", studentGrade.StudentID);
            return View(studentGrade);
        }
        public ActionResult StudentGrades(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            var lol = ((int)Session["GradeModule"]);
            var studentGrades = db.StudentGrades.Include(s => s.GradeTitle).Include(s => s.Module).Where(x=>x.ModuleID == lol).Include(s => s.User).Where(x=>x.StudentID == id);
            return View(studentGrades.ToList());
        }
        public ActionResult EditGrade(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentGrade studentGrade = db.StudentGrades.Find(id);
            if (studentGrade == null)
            {
                return HttpNotFound();
            }
            var lol = ((int)Session["GradeModule"]);
            var student = db.StudentGrades.Where(x => x.GradeID == id).FirstOrDefault();
            ViewBag.GradeTitleID = new SelectList(db.GradeTitles.Where(x => x.ModuleID == lol), "GradeTitleID", "GradeTitle1");
            //ViewBag.GradeTitleID = new SelectList(db.GradeTitles, "GradeTitleID", "GradeTitle1", studentGrade.GradeTitleID);
            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "ModuleCode", studentGrade.ModuleID);
            ViewBag.StudentID = new SelectList(db.Users.Where(x=>x.UserID == student.StudentID), "UserID", "Name", studentGrade.StudentID);
            return View(studentGrade);
        }

        // POST: StudentGrades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditGrade([Bind(Include = "GradeID,ModuleID,LecturerID,StudentID,GradeTitleID,Grade")] StudentGrade studentGrade)
        {
            if (ModelState.IsValid)
            {
                var lol = ((int)Session["GradeModule"]);
                studentGrade.ModuleID = lol;
                db.Entry(studentGrade).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Modules");
            }
            ViewBag.GradeTitleID = new SelectList(db.GradeTitles, "GradeTitleID", "GradeTitle1", studentGrade.GradeTitleID);
            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "ModuleCode", studentGrade.ModuleID);
            ViewBag.StudentID = new SelectList(db.Users, "UserID", "Name", studentGrade.StudentID);
            return View(studentGrade);
        }
        public ActionResult Delete(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentGrade studentGrade = db.StudentGrades.Find(id);
            if (studentGrade == null)
            {
                return HttpNotFound();
            }
            return View(studentGrade);
        }

        // POST: StudentGrades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmeded(int id)
        {
            StudentGrade studentGrade = db.StudentGrades.Find(id);
            db.StudentGrades.Remove(studentGrade);
            db.SaveChanges();
            return RedirectToAction("Modules");
        }
        public ActionResult DeleteGradeTitle(int? id)
        {
            if (Check() == false)
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GradeTitle gradeTitle = db.GradeTitles.Find(id);
            if (gradeTitle == null)
            {
                return HttpNotFound();
            }
            return View(gradeTitle);
        }

        // POST: GradeTitles/Delete/5
        [HttpPost, ActionName("DeleteGradeTitle")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed2(int id)
        {
            while (true)
            {
                StudentGrade studentgrade = db.StudentGrades.Where(x => x.GradeTitleID == id).FirstOrDefault();
                if (studentgrade != null)
                {
                    db.StudentGrades.Remove(studentgrade);
                    db.SaveChanges();
                }
                else
                {
                    break;
                }
            }
            GradeTitle gradeTitle = db.GradeTitles.Find(id);
            db.GradeTitles.Remove(gradeTitle);
            db.SaveChanges();
            return RedirectToAction("Modules");
        }
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}