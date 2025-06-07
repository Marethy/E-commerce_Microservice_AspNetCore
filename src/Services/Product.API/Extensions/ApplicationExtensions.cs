using Infrastructure.Middlewares;

namespace Product.API.Extensions
{
    public static class ApplicationExtensions
    {
        public static void UseInfrastructure(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
            {
                    c.OAuthClientId("microservices_swagger");
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API");
                    c.DisplayRequestDuration();
                });
            }

            // app.UseHttpsRedirection();
            app.UseMiddleware<ErrorWrappingMiddleware>();


            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllers();
        }
    }
}