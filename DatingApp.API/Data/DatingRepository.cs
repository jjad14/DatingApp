using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context)
        {
            _context = context;

        }

        // no async method just synchronous code because when we add something into our context, we're not querying or doing anyhting with the database
        public void Add<T>(T entity) where T : class
        {
            // saved in memory until we actually save our changes back to the datbase
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }

        // we pass the id, then the id is used to extract the first or default from the database that matches the id that we're passing in
        // if we dont have a matching id in our database then we return the default for this particular type which is gonna be a user object
        // default is null
        public async Task<User> GetUser(int id)
        {
            // variable to store user and photos -  we want to show a user with their main profile photo
            // photos are considered navigation properties, so we must tell Entity Framework to include it
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }

        // return list of users
        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await _context.Users.Include(p => p.Photos).ToListAsync();

            return users;
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

    }
}