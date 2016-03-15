using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace IdeasRepository.Models
{
    public static class SeedData
    {

        private static RoleManager<IdentityRole> _roleManager;
        private static UserManager<ApplicationUser> _userManager;
        private static IdentityRole[] _roles;
        private static ApplicationUser[] _users;
        private static List<KeyValuePair<ApplicationUser, IdentityRole>> _usersToRoles;
        private static ApplicationDbContext _context;
        private static int _total;


        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            //Пользователи
            _roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
            _userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            _context = serviceProvider.GetService<ApplicationDbContext>();
            _roles = new[]
            {
                new IdentityRole("Administrator"),
                new IdentityRole("User")
            };
            _users = new[]
            {
                new ApplicationUser {UserName = "AdministratorTest@gmail.com", Email = "AdministratorTest@gmail.com"},
                new ApplicationUser {UserName = "User1Test@gmail.com", Email = "User1Test@gmail.com"},
                new ApplicationUser {UserName = "User2Test@gmail.com", Email = "User2Test@gmail.com"}
            };
            _usersToRoles = new List<KeyValuePair<ApplicationUser, IdentityRole>>();
            foreach (var appUser in _users)
            {
                _usersToRoles.Add(new KeyValuePair<ApplicationUser, IdentityRole>(appUser,
                    _roles.Where(l => appUser.UserName.Contains(l.Name)).OrderByDescending(l => l.Name.Length).First()));
            }

            foreach (var role in _roles)
            {
                var roleExists = await _roleManager.RoleExistsAsync(role.Name);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(role);
                }
            }
            foreach (var appUser in _users)
            {
                if (!_userManager.Users.Any(l => l.Email == appUser.Email))
                {
                    await _userManager.CreateAsync(appUser, "User1!");
                    await
                        _userManager.AddToRolesAsync(appUser,
                            _usersToRoles.Where(l => l.Key == appUser).Select(l => l.Value.Name));
                }
            }


            //Записи
            var admin = (await _userManager.GetUsersInRoleAsync("Administrator")).First();
            var user = (await _userManager.GetUsersInRoleAsync("User")).First();
            var text = "<p><strong> Have the Entity Framework automatically drop and re - create the database based on the new model class schema. </strong></p><ul><li>This<a href=\"http://www.google.com\"> approach </a>is very convenient early in the development cycle when you are doing active development on a test database; it allows you to quickly evolve the model and database schema together.</li></ul><p><em>The downside, though, is that you lose existing data in the database &mdash; so you don&rsquo;t want to use this approach on a production database! </em></p><ol><li>Using an initializer to automatically seed a database with test data is often a productive way to develop an application.</li></ol>";
            var userWish = (await _userManager.GetUsersInRoleAsync("User")).First(l=>l.Id != user.Id);
            var textWish = "<strong>I am a Jedi like my father before me. Unless Hogwarts sends me a letter like my mother.</strong>";
            _context.IdeaRecord.RemoveRange(_context.IdeaRecord);
            _context.SaveChanges();

            AddRecords(text, admin.Id, IdeaRecordStatusEnum.Created, 10);
            AddRecords(text, user.Id, IdeaRecordStatusEnum.Created, 30);
            AddRecords(text, user.Id, IdeaRecordStatusEnum.RemovedByAdmin, 10);
            AddRecords(text, user.Id, IdeaRecordStatusEnum.RemovedByUser, 10);
            AddRecords(text, user.Id, IdeaRecordStatusEnum.ArchiveByAdmin, 10);
            AddRecords(text, user.Id, IdeaRecordStatusEnum.ArchiveByUser, 10);
            AddRecords(textWish, userWish.Id, IdeaRecordStatusEnum.Created, 1, "Wish");
        }

        public static void AddRecords(string text, string userId, IdeaRecordStatusEnum status, int n, string title = null)
        {
            var rand = new Random();
            for (var i = _total; i < _total + n; i++)
            {
                var nowDate = DateTime.Now.AddMinutes(- rand.Next(59));
                _context.IdeaRecord.Add(new IdeaRecord
                {
                    ID = Guid.NewGuid().ToString(),
                    Title = title??$"Title {i}",
                    Text = text,
                    Prewiew = IdeaRecord.GetPrewiew(text),
                    Date = nowDate,
                    AuthorId = userId,
                    StatusDate = nowDate,
                    Status = status
                });
            }
            _total += n;
            _context.SaveChanges();
        }
    }
}