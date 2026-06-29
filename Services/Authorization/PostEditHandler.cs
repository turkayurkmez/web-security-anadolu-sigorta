using Microsoft.AspNetCore.Authorization;
using SecureBlog.API.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SecureBlog.API.Services.Authorization
{
    public class PostEditHandler : AuthorizationHandler<PostEditRequirement>
    {
        private readonly AppDbContext _dbContext;

        public PostEditHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, PostEditRequirement requirement)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return;
            }

            //Kaynak nereden geliyor?

            var httpContext = context.Resource as HttpContext;
            if (httpContext is null)
            {
                context.Fail();
                return;
            }


            //route içerisinde id parametresi var mı?
            if (!httpContext.Request.RouteValues.TryGetValue("id",out var idValue) || !int.TryParse(idValue?.ToString(), out var postId))
            {
                context.Fail();
                return;
            }


            //token'da userId bilgisi var mı?
            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Fail();
                return;
            }


            var post = await _dbContext.Posts.FindAsync(postId);
            if (post is not null && post.AuthorId == userId)
            {
                context.Succeed(requirement);
            }










        }
    }
}
