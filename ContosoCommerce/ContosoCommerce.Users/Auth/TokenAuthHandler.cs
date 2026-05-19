using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ContosoCommerce.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContosoCommerce.Users.Auth
{
    public class TokenAuthHandler
        : AuthenticationHandler<
            AuthenticationSchemeOptions>
    {
        private const string TokenHeader =
            "X-Auth-Token";

        private readonly CommerceDbContext _db;

        public TokenAuthHandler(
            IOptionsMonitor<
                AuthenticationSchemeOptions>
                options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            CommerceDbContext db)
            : base(
                options, logger, encoder)
        {
            _db = db;
        }

        protected override async
            Task<AuthenticateResult>
            HandleAuthenticateAsync()
        {
            if (!Request.Headers
                .ContainsKey(TokenHeader))
            {
                return AuthenticateResult
                    .NoResult();
            }

            var token = Request
                .Headers[TokenHeader]
                .FirstOrDefault();

            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult
                    .Fail(
                        "Missing auth token.");
            }

            var authToken = await _db
                .AuthTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == token
                    && !t.IsRevoked
                    && t.ExpiresAt
                        > DateTime.UtcNow);

            if (authToken == null)
            {
                return AuthenticateResult
                    .Fail(
                        "Invalid or expired"
                        + " token.");
            }

            Context.Items["UserId"] =
                authToken.UserId;
            Context.Items["UserEmail"] =
                authToken.User.Email;

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    authToken.UserId
                        .ToString()),
                new Claim(
                    ClaimTypes.Email,
                    authToken.User.Email),
                new Claim(
                    ClaimTypes.Role,
                    authToken.User.Role
                        .ToString())
            };

            var identity =
                new ClaimsIdentity(
                    claims, Scheme.Name);
            var principal =
                new ClaimsPrincipal(identity);
            var ticket =
                new AuthenticationTicket(
                    principal, Scheme.Name);

            return AuthenticateResult
                .Success(ticket);
        }
    }
}
