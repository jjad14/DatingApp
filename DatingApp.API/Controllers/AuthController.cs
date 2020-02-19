using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        //inject created respository
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _config = config;
            _mapper = mapper;
            _repo = repo;

        }

        /*
        parameters that we send up to our methods via http, asp.net core mvc is automatically going to try to infer 
        the parameters from either the body, the query string, or from a form
        */
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists");

            // Map<DESTINATION>(SOURCE)
            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);

            return CreatedAtRoute("GetUser", new {controller = "Users", id = createdUser.Id}, userToReturn);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {            
            //checking to make sure that we have a user, and that their username and password matches whats stored in the database for that particular user
            //userForLoginDto is in lower case so we are always attmepting to login with a lowercase username
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            //if login fails, we dont want to say how it failed to the user since we dont want to give out any information (i.e. user name is right but password is not)
            //thus an Unauthorized is given
            if (userFromRepo == null)
                return Unauthorized();

            //Start creating the token - if user login is success
            //token contains two claims, one is the user's Id, the other is the user's username
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)

            };
            
            //token key
            //in appsettings.json - AppSettings:Token name - in the real world the name of token would be an extremely long randomly generated selection of characters (in the json file)
            //In order to make sure that the tokens are valid when it comes back, the server needs to sign this token 
            //creating security key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value)); 

            //using key from above, as part of the signing credentials and encrypting the key with HmacSha512
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //now we create the token, we create a token descriptor and pass our claims as the subjects, add an expiry date (set to 24 hours), then pass our signing credentials from above 
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
              Subject = new ClaimsIdentity(claims), //pass claims as subject
              Expires = DateTime.Now.AddDays(1), //24 hour expiry
              SigningCredentials = creds  // signing credentials
            };

            //create the token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            //allows us to create the token based on the token descriptor being passed in here using the token handler
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var user = _mapper.Map<UserForListDto>(userFromRepo);

            //we then use the token variable to write the token into a response that we send back to the clients
            return Ok(new 
            {
                token = tokenHandler.WriteToken(token),
                user
            });
        }

    }
}