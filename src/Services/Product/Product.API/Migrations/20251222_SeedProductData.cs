using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Product.API.Persistence;

#nullable disable

namespace Product.API.Migrations
{
    [DbContext(typeof(ProductContext))]
    [Migration("20251222_SeedProductData")]
    public partial class SeedProductData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration creates a placeholder for data seeding
            // The actual seeding is done via the ProductDataSeeder class
            // which should be run separately after this migration
            
            migrationBuilder.Sql(@"
                -- Data seeding will be done via ProductDataSeeder.cs
                -- Run the seeding using: dotnet run --seed-data
                SELECT 1; -- Placeholder
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data seeding is idempotent, so no down migration needed
            // Products can be manually deleted if needed
        }
    }
}
