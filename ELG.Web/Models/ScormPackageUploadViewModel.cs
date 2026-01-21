using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ELG.Web.Models
{
    public class ScormPackageUploadViewModel
    {
        [Required(ErrorMessage = "Course title is required")]
        [Display(Name = "Course Title")]
        [MaxLength(100)]
        public string CourseTitle { get; set; }

        [Display(Name = "Course Description")]
        [MaxLength(400)]
        public string CourseDescription { get; set; }

        [Required(ErrorMessage = "SCORM package (ZIP file) is required")]
        [Display(Name = "SCORM Package")]
        public IFormFile ScormPackage { get; set; }

        [Display(Name = "Course Thumbnail")]
        public IFormFile Thumbnail { get; set; }
    }
}
