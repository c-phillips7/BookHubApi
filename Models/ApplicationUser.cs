using Microsoft.AspNetCore.Identity;

namespace BookHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName {get;set;}
        // Could be turned into uploaded photo at some point
        public string ProfilePictureUrl {get;set;}
        public DateTime DateJoined {get;set;}
        public string Bio {get;set;}
        // List of all user's reviews (for possible implementation later)
        public List<Review> Reviews {get;set;} = new List<Review>();
    }
}