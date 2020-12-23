using ApiService.Contexts;
using ApiService.Exceptions;
using ApiService.Models;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Threading.Tasks;

namespace ApiService.Logic
{
    public class RegisterLogic
    {
        public async Task<User> CreateUser(DatabaseContext dbContext, User user)
        {
            var loadedUser = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName.Equals(user.UserName));

            if (loadedUser != null)
                throw new HttpException("User already exists", 409);

            var newUser = new User
            {
                UserName = user.UserName,
            };

            newUser.Roles.Add(new Role() { Name = "User" });
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, BCrypt.Net.BCrypt.GenerateSalt());

            await dbContext.Users.AddAsync(newUser);
            var res = await dbContext.SaveChangesAsync();

            if (res == 0)
                throw new HttpException("Failed saving user", 500);

            return newUser;
        }
    }
}
