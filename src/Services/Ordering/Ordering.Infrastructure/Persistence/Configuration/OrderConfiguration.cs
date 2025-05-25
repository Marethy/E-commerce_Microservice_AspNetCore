using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Entities;
using Shared.Enums.Order;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(x => x.Status)
            .HasDefaultValue(OrderStatus.New) // Giá trị mặc định cho trạng thái đơn hàng
            .IsRequired()
            .HasSentinel(OrderStatus.New);
    }
}