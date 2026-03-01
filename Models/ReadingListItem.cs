namespace BookHub.Models
{
    public enum ReadingStatus
    {
        WantToRead,
        CurrentlyReading,
        Read
    }
    public class ReadingListItem
    {
        public int Id {get;set;}
        
        public int ReadingListId {get;set;}
        public ReadingList ReadingList {get;set;}

        public int BookId {get;set;}
        public Book Book {get;set;}

        //May add more or less information to reading list
        public DateTime DateAdded {get;set;} = DateTime.UtcNow;
        public ReadingStatus Status {get;set;} = ReadingStatus.WantToRead;
    }
}