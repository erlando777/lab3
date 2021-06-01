//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;
namespace WebApplication7.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class StudentGrade
    {
        public int GradeID { get; set; }
        public Nullable<int> ModuleID { get; set; }
        public Nullable<int> LecturerID { get; set; }
        [Required(ErrorMessage = "Student is required.")]
        public Nullable<int> StudentID { get; set; }
        [Required(ErrorMessage = "Title is required.")]
        public Nullable<int> GradeTitleID { get; set; }
        [Required(ErrorMessage = "Grade is required.")]
        [Range(0, float.MaxValue, ErrorMessage = "Please enter valid integer Number")]
        public Nullable<double> Grade { get; set; }
    
        public virtual GradeTitle GradeTitle { get; set; }
        public virtual Module Module { get; set; }
        public virtual User User { get; set; }
    }
}