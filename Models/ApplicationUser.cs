using Microsoft.AspNetCore.Identity;

namespace BookHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName {get;set;} = "New User";
        // Could be turned into uploaded photo at some point
        public string ProfilePictureUrl {get;set;} = "default.png";
        public DateTime DateJoined {get;set;} = DateTime.Now;
        public string Bio {get;set;} = "";
        // List of all user's reviews (for possible implementation later)
        public List<Review> Reviews {get;set;} = new List<Review>();
    }
}