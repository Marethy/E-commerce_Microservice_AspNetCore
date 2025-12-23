# Product Data Seeding Guide

## Overview

This guide explains how to seed product data from JSON files into the Product.API database.

## Prerequisites

1. PostgreSQL database running and accessible
2. Connection string configured in `appsettings.json`
3. JSON data files in the `clean` folder at `c:\Users\PC\Desktop\source\clean`

## Step 1: Apply Schema Migrations

First, ensure the database schema is up to date with the new ExternalId fields and ProductVariant table:

```powershell
cd c:\Users\PC\Desktop\source\backend_microservices\src\Services\Product\Product.API

# Apply migrations (creates ExternalId columns and ProductVariants table)
dotnet ef database update --project Product.API.csproj
```

## Step 2: Seed Product Data

Run the data seeding command to import all products from JSON files:

```powershell
# Using default data path (c:\Users\PC\Desktop\source\clean)
dotnet run --seed-data

# Or specify custom data path
dotnet run --seed-data --data-path "C:\path\to\clean"
```

The seeding process will:
1. Read all 26 JSON files from the `clean` folder
2. Import brands, sellers, and categories (avoiding duplicates)
3. Import products with their relationships
4. Import product images, specifications, and variants
5. Use batch processing (saves every 100 products for performance)

## Expected Results

**Data imported:**
- ~200,000 products (items with valid prices)
- 26 main categories (from file names)
- Additional categories from JSON data
- Brands from product data
- Sellers from product data
- Product images (~5-10 per product)
- Product specifications (various attributes)
- Product variants (color, size, material options)

**Database tables affected:**
- `Products` - Product catalog entries
- `Brands` - Product brands
- `Sellers` - Product sellers/merchants
- `Categories` - Product categories
- `ProductCategories` - Many-to-many product-category relationships
- `ProductImages` - Product images
- `ProductSpecifications` - Product technical specifications
- `ProductVariants` - Product variant options (NEW)

## Verification Queries

After seeding, verify the data:

```sql
-- Check total counts
SELECT COUNT(*) FROM "Products";
SELECT COUNT(*) FROM "Brands";
SELECT COUNT(*) FROM "Categories";
SELECT COUNT(*) FROM "Sellers";
SELECT COUNT(*) FROM "ProductImages";
SELECT COUNT(*) FROM "ProductSpecifications";
SELECT COUNT(*) FROM "ProductVariants";

-- Sample products
SELECT "Id", "ExternalId", "Name", "Price", "CategoryId" 
FROM "Products" 
LIMIT 10;

-- Products with brands
SELECT p."Name", b."Name" as "BrandName"
FROM "Products" p
LEFT JOIN "Brands" b ON p."BrandId" = b."Id"
WHERE b."Name" IS NOT NULL
LIMIT 10;

-- Products with variants
SELECT p."Name", v."AttributeName", v."AttributeValue"
FROM "Products" p
INNER JOIN "ProductVariants" v ON p."Id" = v."ProductId"
LIMIT 20;
```

## Troubleshooting

**Database connection error:**
- Check `appsettings.json` for correct PostgreSQL connection string
- Ensure PostgreSQL server is running
- Verify database exists

**JSON parsing errors:**
- Ensure JSON files are in UTF-8 encoding
- Check that files follow the pattern `clean_*.json`
- Verify JSON structure matches expected schema

**Duplicate data:**
- The seeder checks for existing products by `ExternalId`
- Re-running the seeder will skip already imported products
- To re-import, delete products first or drop the database

## Notes

- Seeding process may take 10-30 minutes depending on system performance
- Database writes are batched every 100 products for optimal performance
- ExternalId indexes ensure fast lookups and prevent duplicates
- The seeder is idempotent - safe to run multiple times
