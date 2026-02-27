using System.Collections.Generic;

namespace BookHub.Models
{
    public class Book
    {
        public int Id {get; set;}
        public string Title {get; set;}
        public int AuthorId {get;set;}
        public Author Author {get; set;}
        public string Description {get; set;}

        public List<BookGenre> BookGenres {get;set;}
        public List<Review> Reviews {get; set;}
        // Added so that can query all ReadingListsItems that contain a book
        public List<ReadingListItem> ReadingListItems {get;set;} = new List<ReadingListItem>();
    }

}