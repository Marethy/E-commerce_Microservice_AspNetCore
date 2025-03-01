namespace Basket.API.Extensions
{
    public static class ApplicationExtensions
    {
        public static void UseInfrastructure(this WebApplication app)
        {
          //  if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Middleware chuẩn trong ASP.NET Core
            // app.UseHttpsRedirection(); // Bật nếu dùng HTTPS
            app.UseRouting(); // Bắt buộc trước Authorization

            app.UseAuthentication();  // Gọi Authentication trước Authorization
            app.UseAuthorization();

            app.MapControllers();
        }
    }
}
