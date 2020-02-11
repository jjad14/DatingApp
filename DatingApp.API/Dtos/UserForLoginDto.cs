namespace DatingApp.API.Dtos
{
    //Data Transfer Object (DTO), 
    // are often mapped to their domain models such as our user class into simpler objects that ultimately get returned or displayed by end of view
    //DTO to represent our username and password
    public class UserForLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    
    }
}