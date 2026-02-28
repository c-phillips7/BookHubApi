namespace BookHub.Models
{
    public class Review
    {
        public int Id {get;set;}
        public int BookId{get;set;}
        public Book Book {get;set;}

        public string UserId {get;set;} //FK to ApplicationUser
        public ApplicationUser User {get;set;}

        public string Content {get;set;}
        public int Rating {get;set;} // 1-5 scale
    }
}