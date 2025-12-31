using Product.API.Entities;
using Product.API.Persistence;

namespace Product.API.Extensions;

public static class ReviewSeeder
{
    private static readonly string[] ReviewTitles = new[]
    {
        "Sản phẩm tuyệt vời!",
        "Rất hài lòng",
        "Đáng đồng tiền",
        "Chất lượng tốt",
        "Giao hàng nhanh",
        "Đúng như mô tả",
        "Sẽ mua lại",
        "Tạm ổn",
        "Không như mong đợi",
        "Cần cải thiện"
    };

    private static readonly string[] ReviewContents5Star = new[]
    {
        "Sản phẩm rất chất lượng, đóng gói cẩn thận. Shop phục vụ nhiệt tình. Mình rất hài lòng và sẽ giới thiệu cho bạn bè.",
        "Giao hàng nhanh, sản phẩm đẹp như hình. Chất liệu tốt, giá cả hợp lý. 5 sao cho shop!",
        "Đã mua nhiều lần ở shop, lần nào cũng ưng ý. Sản phẩm này đặc biệt tốt, xứng đáng 5 sao.",
        "Chất lượng vượt mong đợi! Đóng gói đẹp, giao hàng đúng hẹn. Recommendation!",
        "Tuyệt vời! Sản phẩm chính hãng, chất lượng cao. Mình sẽ quay lại mua tiếp."
    };

    private static readonly string[] ReviewContents4Star = new[]
    {
        "Sản phẩm tốt nhưng giao hàng hơi lâu. Nhìn chung vẫn hài lòng.",
        "Chất lượng ổn, giá hơi cao một chút nhưng chấp nhận được. 4 sao!",
        "Sản phẩm như mô tả, đóng gói cẩn thận. Trừ 1 sao vì ship hơi lâu.",
        "Tốt, nhưng màu sắc hơi khác ảnh một chút. Vẫn OK nhé!",
        "Chất lượng tốt, đóng gói cẩn thận. Sẽ mua lại nếu có nhu cầu."
    };

    private static readonly string[] ReviewContents3Star = new[]
    {
        "Bình thường, không có gì nổi bật. Giá thì hợp lý.",
        "Tạm được, chất lượng trung bình. Mong shop cải thiện.",
        "Sản phẩm OK nhưng đóng gói không được cẩn thận lắm.",
        "3 sao vì shipping chậm, nhưng sản phẩm thì ổn.",
        "Ổn, không quá tốt nhưng cũng không tệ."
    };

    private static readonly string[] ReviewContents2Star = new[]
    {
        "Không giống như mô tả. Hơi thất vọng.",
        "Chất lượng không tốt lắm. Giá không xứng đáng.",
        "Giao hàng quá lâu, sản phẩm có vết trầy xước.",
        "Mong shop cải thiện chất lượng sản phẩm.",
        "Không như kỳ vọng. Hơi thất vọng về chất lượng."
    };

    private static readonly string[] ReviewContents1Star = new[]
    {
        "Rất thất vọng. Không giống hình, chất lượng kém.",
        "Giao sai hàng, liên hệ shop không phản hồi.",
        "Sản phẩm kém chất lượng, không đáng tiền.",
        "1 sao vì ship quá lâu và hàng không đúng mô tả.",
        "Không hài lòng. Sẽ không quay lại mua nữa."
    };

    private static readonly string[] UserNames = new[]
    {
        "Nguyễn Văn A",
        "Trần Thị B",
        "Lê Văn C",
        "Phạm Thị D",
        "Hoàng Văn E",
        "Vũ Thị F",
        "Đặng Văn G",
        "Bùi Thị H",
        "Đinh Văn I",
        "Dương Thị K"
    };

    public static async Task SeedReviewsAsync(ProductContext context)
    {
        // Check if reviews already exist
        if (context.ProductReviews.Any())
        {
            Console.WriteLine("Reviews already seeded. Skipping...");
            return;
        }

        Console.WriteLine("Starting to seed reviews...");

        var products = context.Products.ToList();
        var random = new Random();
        int totalReviewsCreated = 0;

        foreach (var product in products)
        {
            // Generate 5-10 reviews per product
            int reviewCount = random.Next(5, 11);

            for (int i = 0; i < reviewCount; i++)
            {
                int rating = GenerateWeightedRating(random);

                var review = new ProductReview
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    UserId = $"user-{random.Next(1, 50)}",  // Random user ID
                    Rating = rating,
                    Title = ReviewTitles[random.Next(ReviewTitles.Length)],
                    Comment = GetContentByRating(rating, random),
                    ReviewDate = DateTimeOffset.UtcNow.AddDays(-random.Next(1, 180)),  // Random date in past 6 months
                    HelpfulVotes = random.Next(0, 50),
                    VerifiedPurchase = random.Next(0, 100) < 70,  // 70% verified
                    CreatedDate = DateTimeOffset.UtcNow.AddDays(-random.Next(1, 180)),
                    LastModifiedDate = DateTimeOffset.UtcNow
                };

                context.ProductReviews.Add(review);
                totalReviewsCreated++;
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"✅ Successfully seeded {totalReviewsCreated} reviews for {products.Count()} products!");
    }

    private static int GenerateWeightedRating(Random random)
    {
        // Weight ratings: more 4-5 stars,  fewer 1-2 stars
        // Distribution: 5% (1★), 10% (2★), 15% (3★), 35% (4★), 35% (5★)
        int[] weights = { 5, 10, 15, 35, 35 }; // 1,2,3,4,5 stars
        int total = weights.Sum();
        int randomValue = random.Next(total);

        int cumulative = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (randomValue < cumulative)
                return i + 1;
        }
        return 5;
    }

    private static string GetContentByRating(int rating, Random random)
    {
        return rating switch
        {
            5 => ReviewContents5Star[random.Next(ReviewContents5Star.Length)],
            4 => ReviewContents4Star[random.Next(ReviewContents4Star.Length)],
            3 => ReviewContents3Star[random.Next(ReviewContents3Star.Length)],
            2 => ReviewContents2Star[random.Next(ReviewContents2Star.Length)],
            1 => ReviewContents1Star[random.Next(ReviewContents1Star.Length)],
            _ => "Sản phẩm tốt!"
        };
    }
}
