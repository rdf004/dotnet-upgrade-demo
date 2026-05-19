using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using ContosoCommerce.Data;

namespace ContosoCommerce.Users.Filters
{
    /// <summary>
    /// Custom authorization filter that validates
    /// the X-Auth-Token header against tokens
    /// stored in the database. Applied to ALL
    /// module controllers. Uses HttpContext.Current
    /// which is a classic .NET Framework pattern
    /// that breaks in .NET 8.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class
        | AttributeTargets.Method,
        AllowMultiple = false)]
    public class TokenAuthorizeAttribute
        : AuthorizeAttribute
    {
        private const string TokenHeader =
            "X-Auth-Token";

        public override void OnAuthorization(
            HttpActionContext actionContext)
        {
            if (SkipAuthorization(actionContext))
                return;

            var request =
                HttpContext.Current.Request;
            var token =
                request.Headers[TokenHeader];

            if (string.IsNullOrEmpty(token))
            {
                HandleUnauthorized(
                    actionContext,
                    "Missing auth token.");
                return;
            }

            using (var db =
                new CommerceDbContext())
            {
                var authToken = db.AuthTokens
                    .FirstOrDefault(t =>
                        t.Token == token
                        && !t.IsRevoked
                        && t.ExpiresAt
                            > DateTime.UtcNow);

                if (authToken == null)
                {
                    HandleUnauthorized(
                        actionContext,
                        "Invalid or expired "
                        + "token.");
                    return;
                }

                HttpContext.Current
                    .Items["UserId"] =
                        authToken.UserId;
                HttpContext.Current
                    .Items["UserEmail"] =
                        authToken.User.Email;
            }
        }

        private static bool SkipAuthorization(
            HttpActionContext context)
        {
            return context.ActionDescriptor
                .GetCustomAttributes<
                    AllowAnonymousAttribute>()
                .Any()
                || context.ControllerContext
                    .ControllerDescriptor
                    .GetCustomAttributes<
                        AllowAnonymousAttribute>()
                    .Any();
        }

        private static void HandleUnauthorized(
            HttpActionContext context,
            string message)
        {
            context.Response =
                context.Request
                    .CreateErrorResponse(
                        HttpStatusCode
                            .Unauthorized,
                        message);
        }
    }
}
