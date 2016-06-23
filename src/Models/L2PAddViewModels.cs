using System.ComponentModel.DataAnnotations;

namespace Grp.L2PSite.MobileApp.Models
{
    public class HyperLinkViewModel
    {
        [Required]
        [DataType(DataType.Url)]
        [Display(Name = "Hyperlink URL")]
        public string URL { get; set;}

        [StringLength(50)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [StringLength(200)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class AssignmentViewModel
    {
        [Required]
        [DataType(DataType.Url)]
        [Display(Name = "Hyperlink URL")]
        public string URL { get; set; }

        [StringLength(50)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [StringLength(200)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }

}