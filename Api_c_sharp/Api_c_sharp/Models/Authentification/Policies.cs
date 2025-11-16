using Microsoft.AspNetCore.Authorization;

namespace Api_c_sharp.Models.Authentification;

public class Policies
{
    public const string Authorized = "Authorized";

    public static AuthorizationPolicy Logged()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Authorized).Build();
    }
}