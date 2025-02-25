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
                app.UseSwaggerUI();
            }

            // Uncomment if you want to use HTTPS redirection
            // app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
        }
    }
}
