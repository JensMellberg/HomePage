using System.Security.Cryptography;
using System.Text;
using HomePage.Data;
using HomePage.Model;
using Microsoft.EntityFrameworkCore;

namespace HomePage
{
    public class SignInRepository(AppDbContext dbContext, DatabaseLogger logger)
    {
        public static bool IsLoggedIn(ISession session)
        {
            var bytes = session.Get("IsLoggedIn");
            return bytes != null && BitConverter.ToBoolean(bytes, 0);
        }

        public static bool IsLoggedInAsAdmin(ISession session)
        {
            var bytes = session.Get("IsAdmin");
            return bytes != null && BitConverter.ToBoolean(bytes, 0);
        }

        public bool CreateAccount(string username, string password, string name)
        {
            if (dbContext.UserInfo.ToList().Any(x => x.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase))) {
                return false;
            }

            var user = new UserInfo {
                UserName = username,
                DisplayName = name,
                PasswordHash = HashPassword(password)
            };
            dbContext.UserInfo.Add(user);
            dbContext.SaveChanges();

            return true;
        }

        public bool LogInWithPassword(ISession session, HttpResponse response, string username, string password)
        {
            var user = dbContext.UserInfo.Include(x => x.Cookies).ToList()
                .FirstOrDefault(x => x.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase));
            if (user == null)
            {
                logger.Warning($"Failed login for invalid username {username}.", null);
                return false;
            }

            var hashedPassword = HashPassword(password);
            if (!hashedPassword.SequenceEqual(user.PasswordHash))
            {
                logger.Warning($"Failed login for {username} with invalid password.", null);
                return false;
            }

            logger.Information($"User {username} logged in.", null);
            LogInAndSetCookie(session, response, user);
            return true;
        }

        public static byte[] HashPassword(string password)
        {
           var dataToHash = Encoding.UTF8.GetBytes(password);

            using SHA256 sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(dataToHash);
            return hashBytes;
        }

        public void LogOut(ISession session)
        {
            var userInfo = GetLoggedInUserInfo(session);

            if (userInfo != null) {
                var cookies = userInfo.Cookies;
                dbContext.SignInCookie.RemoveRange(cookies);
                dbContext.SaveChanges();
            }

            session.Clear();
        }

        public void LogInAndSetCookie(ISession session, HttpResponse response, UserInfo user)
        {
            SetLoggedInSession(session, user);
            var cookieId = Guid.NewGuid();
            var expires = DateHelper.DateTimeNow.AddMonths(1);
            response.Cookies.Append("Auth-Id", cookieId.ToString(), new CookieOptions { Expires = expires });
            dbContext.SignInCookie.Add(new SignInCookie { CookieId = cookieId, Expires = expires, UserId = user.UserName });
            dbContext.SaveChanges();
        }

        public static void SetLoggedInSession(ISession session, UserInfo user)
        {
            session.Set("IsLoggedIn", BitConverter.GetBytes(true));
            session.SetString("LoggedInPerson", user.UserName);
            session.Set("IsAdmin", BitConverter.GetBytes(user.IsAdmin));
        }

        public Person? LoggedInPerson(ISession session)
        {
           var userInfo = GetLoggedInUserInfo(session);
           if (userInfo == null)
           {
               return null;
           }

            return Person.FromUserInfo(userInfo);
        }

        private UserInfo? GetLoggedInUserInfo(ISession session)
        {
            var person = session.GetString("LoggedInPerson");
            if (person == null)
            {
                return null;
            }

            return dbContext.UserInfo.Include(x => x.Cookies).FirstOrDefault(x => x.UserName == person) ?? throw new Exception();
        }

        public void TryLogInFromCookie(ISession session, HttpRequest request, HttpResponse response)
        {
            if (IsLoggedIn(session))
            {
                return;
            }

            request.Cookies.TryGetValue("Auth-Id", out var cookie);
            if (cookie == null || !Guid.TryParse(cookie, out var cookieGuid))
            {
                return;
            }

            var users = dbContext.UserInfo.Include(x => x.Cookies).ToList();
            foreach (var user in users)
            {
                var validUserCookies = user.Cookies.Where(x => x.Expires > DateHelper.DateTimeNow).ToList();
                var validCookie = validUserCookies.FirstOrDefault(x => x.CookieId.Equals(cookieGuid));
                if (validCookie != null)
                {
                    SetLoggedInSession(session, user);
                    var expires = DateHelper.DateTimeNow.AddMonths(1);
                    response.Cookies.Append("Auth-Id", cookie, new CookieOptions { Expires = expires });
                    validCookie.Expires = expires;
                    dbContext.SaveChanges();
                }
            }
        }
    }
}
