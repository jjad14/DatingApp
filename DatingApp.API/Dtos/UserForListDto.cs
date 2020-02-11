using System;

namespace DatingApp.API.Dtos
{
    // properties that we want to return when a request for our user list is made
    public class UserForListDto
    {
        // these are what we want to return to the member list page - anything exempt is not needed
        public int Id { get; set; }
        public string Username { get; set; }
        public string Gender { get; set; }

        // instead of DateTime date of birth, we rather have age
        public int Age { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        // this is what we'll use to display the user's main profile pic on the member list page
        public string PhotoUrl { get; set; }

    }
}