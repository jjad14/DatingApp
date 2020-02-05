using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    //Data Transfer Object (DTO), 
    // are often mapped to their domain models such as our user class into simpler objects that ultimately get returned or displayed by end of view
    //DTO to represent our username and password

    public class UserForRegisterDto
    {
        //receiving into register method in AuthController
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "You must specifiy password between 4 and 8 characters")]
        public string Password { get; set; }
    }
}