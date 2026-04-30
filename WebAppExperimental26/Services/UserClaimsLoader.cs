using WebAppExperimental26.Models.User;

namespace WebAppExperimental26.Services
{

    public interface IUserClaimsLoader
    {
        Task<UserClaims> LoadDataAsync(IEnumerable<System.Security.Claims.Claim> claims);
    }

    public class UserClaimsLoader : IUserClaimsLoader
    {
        public async Task<UserClaims> LoadDataAsync(IEnumerable<System.Security.Claims.Claim> Claims)
        {

            await Task.Delay(1);

            string? userClaimsSessionIdentifier = Claims.FirstOrDefault(c => c.Type == "sid")?.Value;
            string? userClaimsUserIdentifier = Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
            string? userClaimsName = Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            string[]? userClaimsRoles = Claims.Where(c => c.Type == "roles").Select(c => c.Value).ToArray();
            string? userClaimsEmail = Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            // Ensure required values are not null.
            string vUserClaimsSessionIdentifier = userClaimsSessionIdentifier ?? "None found.";
            string vUserClaimsUserIdentifier = userClaimsUserIdentifier ?? "None found.";
            string vUserClaimsName = userClaimsName ?? "None found.";
            string[] vUserClaimsRoles;

            if (userClaimsRoles.Length == 0)
            {
                vUserClaimsRoles = new string[1];
                vUserClaimsRoles[0] = "None Found";
            }
            else {
                vUserClaimsRoles = new string[userClaimsRoles.Length];
                vUserClaimsRoles = userClaimsRoles.ToArray();
            }
                string vUserClaimsEmail = userClaimsEmail ?? "None found.";

            UserClaims uc = new()
            {

                Sid = vUserClaimsSessionIdentifier,
                Oid = vUserClaimsUserIdentifier,
                Name = vUserClaimsName,
                Roles = vUserClaimsRoles,
                Email = vUserClaimsEmail
            };

            return uc;
        }

    }
}
