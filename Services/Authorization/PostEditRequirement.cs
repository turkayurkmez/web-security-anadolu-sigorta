using Microsoft.AspNetCore.Authorization;

namespace SecureBlog.API.Services.Authorization
{
    /// <summary>
    /// Sözleşme: Bu kullanıcı, bu makaleyi edit'leyebilir mi?
    /// </summary>
    public class PostEditRequirement : IAuthorizationRequirement
    { 

        
    }
}
