namespace SecureBlog.API.Middleware
{
    
    public class SecurityHeaderMiddlware
    {
        private readonly RequestDelegate _next;

        public SecurityHeaderMiddlware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";


                return Task.CompletedTask;
            });

            await _next(context);

            
        }
    }


    public static class MiddlewareExtenssions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder applicationBuilder) =>
            applicationBuilder.UseMiddleware<SecurityHeaderMiddlware>(); 
    }

}
