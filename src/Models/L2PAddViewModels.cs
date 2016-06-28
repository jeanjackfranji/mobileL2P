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

        public int itemId { get; set; }
    }

    public class FolderViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Name")]
        public string Name { get; set; }

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
	
	 public class AnnouncementViewModel
    {
        [Required]
        [StringLength(100)]
        public string title { get; set; }

        [Required]
        [StringLength(100)]
        public string body { get; set; }

        public string folderName {get; set; }
        public int itemId { get; set; }
        
    }

    public class EmailViewModel
    {
        [Required]
        [StringLength(100)]
        public string subject { get; set; }

        [Required]
        [StringLength(100)]
        public string body { get; set; }

        [Required]
        public string recipients { get; set; }
        
        public string cc { get; set; }
        public string sender { get; set; }
        public int itemId { get; set; }
    }
}