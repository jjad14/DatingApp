using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{

    //AuthRepository implements IAuthRepository
    //this repository is responsible for querying our database via Entity Framework
    //used by controller to call exact methods
    //ability to return http responses
    public class AuthRepository : IAuthRepository
    {
        //inject our DataContext
        private readonly DataContext _context;

        //injecting database into constructor
        public AuthRepository(DataContext context)
        {
            _context = context; 
        }

        //login user by comparing the password with the password hash in the database by re-creating the hash
        //controller will return 401 response for null cases
        public async Task<User> Login(string username, string password)
        {
            //find user by username, FirstOrDefaultAsync returns the username or null
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);

            //if user is null, no user exists with parameter username
            if(user == null)
                return null;

            //passwords do not match
            if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        //method to compare the database hash with the user inserted password, if they are the same then they can login, else 401 response
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            //HashAlogrithm implements IDisposable, thus, HMACSHA512 is disposed after use 
            //create hash using password salt in parameter
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                //compute hash using password
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); 

                //compare both hash passwords
                for (int i=0; i< computedHash.Length; i++)
                {
                    if(computedHash[i] != passwordHash[i]) 
                        return false;
                }
                return true;
            }
        }

        //takes our user entity and string password
        public async Task<User> Register(User user, string password)
        {
            //hash and salt stored as byte arrays
            byte[] passwordHash, passwordSalt;
            //pass password and also references hash and salt
            CreatePasswordHash(password, out passwordHash, out passwordSalt); 

            //store hash and salt into user entity
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            //adds back to database
            await _context.Users.AddAsync(user);
            //save changes back to database
            await _context.SaveChangesAsync();

            return user;
        }

        // create hashpassword using given password, store in passwordHash and passwordSalt
        // passwordHash and passwordSalt are given as a reference, so no return is needed
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            //HashAlogrithm implements IDisposable, thus, HMACSHA512 is disposed after use 
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key; // generates random salt
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); //compute hash using password (converted to byte array) and salt
            }
            
        }

        //check if a user exists
        public async Task<bool> UserExists(string username)
        {
            //compare this username against any other user in the database
            if (await _context.Users.AnyAsync(x => x.Username == username))
                return true;
            
            return false;
        }
    }
}