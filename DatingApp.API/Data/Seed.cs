using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Models;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    // takes json data (UserSeedData.json), take json objects and serialize them into user objects to match the models
    public class Seed
    {
        public static void SeedUsers(DataContext context) 
        {
            // make sure there are no users in database
            if(!context.Users.Any())
            {
                //read from our UserSeedData.json file
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                //convert to user objects that we can then loop through
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                foreach (var user in users)
                {
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash("password", out passwordHash, out passwordSalt);

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                    user.Username = user.Username.ToLower();
                    context.Users.Add(user);
                }

                context.SaveChanges();

                //will only run once
                //in order to make it run when application starts, we put it in program.cs
            }
        }
        
        // create hashpassword using given password, store in passwordHash and passwordSalt
        // passwordHash and passwordSalt are given as a reference, so no return is needed
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            //HashAlogrithm implements IDisposable, thus, HMACSHA512 is disposed after use 
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key; // generates random salt
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); //compute hash using password (converted to byte array) and salt
            }
            
        }
    }
}