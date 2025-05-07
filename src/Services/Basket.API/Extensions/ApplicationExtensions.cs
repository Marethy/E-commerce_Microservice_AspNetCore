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

            // app.UseHttpsRedirection(); // Bật nếu dùng HTTPS
            app.UseAuthentication();  // Gọi Authentication trước Authorization

            app.UseRouting(); // Bắt buộc trước Authorization

            app.UseAuthorization();

            app.MapControllers();
        }
    }
}