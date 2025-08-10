using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace HomePage
{
    public class SignInRepository : SingleRepository<SignInInfo>
    {
        public override string FileName => "SignIn.txt";

        public static bool IsLoggedIn(ISession session)
        {
            var bytes = session.Get("IsLoggedIn");
            return bytes != null && BitConverter.ToBoolean(bytes, 0);
        }

        public static bool LogInWithPassword(ISession session, HttpResponse response, string password)
        {
            var signInInfo = new SignInRepository().Get();
            var actualPassword = signInInfo.Password;
            if (password == actualPassword)
            {
                LogIn(session, response, Person.Anna.Name);
                return true;
            }

            var jensPassword = signInInfo.JensPassword;
            if (password == jensPassword)
            {
                LogIn(session, response, Person.Jens.Name);
                return true;
            }

            return false;
        }

        public static void LogIn(ISession session, HttpResponse response, string person)
        {
            LogIn(session, person);
            var cookieId = Guid.NewGuid().ToString();
            response.Cookies.Append("Auth-Id", cookieId, new CookieOptions { Expires = DateTime.Now.AddMonths(1) });
            var repo = new SignInRepository();
            var info = repo.Get();
            if (person == Person.Jens.Name)
            {
                info.JensLoggedInCookies.Add(cookieId);
            } else
            {
                info.LoggedInCookies.Add(cookieId);
            }
            
            repo.Save(info);
        }

        public static void LogIn(ISession session, string person)
        {
            session.Set("IsLoggedIn", BitConverter.GetBytes(true));
            session.SetString("LoggedInPerson", person);
        }

        public static Person LoggedInPerson(ISession session)
        {
            var person = session.GetString("LoggedInPerson");
            if (person == Person.Jens.Name)
            {
                return Person.Jens;
            } else if (person == Person.Anna.Name)
            {
                return Person.Anna;
            }

            return null;
        }

        public static void TryLogIn(ISession session, HttpRequest request, HttpResponse response)
        {
            if (IsLoggedIn(session))
            {
                return;
            }

            request.Cookies.TryGetValue("Auth-Id", out var cookie);
            if (cookie == null)
            {
                return;
            }

            var info = new SignInRepository().Get();
            if (info.LoggedInCookies.Contains(cookie))
            {
                LogIn(session, Person.Anna.Name);
                response.Cookies.Append("Auth-Id", cookie, new CookieOptions { Expires = DateTime.Now.AddMonths(1) });
            } else if (info.JensLoggedInCookies.Contains(cookie)) {
                LogIn(session, Person.Jens.Name);
                response.Cookies.Append("Auth-Id", cookie, new CookieOptions { Expires = DateTime.Now.AddMonths(1) });
            }
        }
    }
}
