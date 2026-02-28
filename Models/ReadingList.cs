using System.Collections.Generic;

namespace BookHub.Models
{
    public class ReadingList
    {
        public int Id {get;set;}

        public string Name {get;set;}
        public string Description {get;set;}
        public bool IsPublic {get;set;}

        public string UserId {get;set;}
        public ApplicationUser User {get;set;}

        //Added initialized value to prevent null reference
        // TODO test for potential bugs caused by this
        public List<ReadingListItem> Items {get;set;} = new List<ReadingListItem>();
    }
}