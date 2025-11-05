// src/Services/Ordering/Ordering.API/Services/OrderPdfService.cs
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Ordering.Application.Common.Models;

namespace Ordering.API.Services;

public interface IOrderPdfService
{
    byte[] GenerateOrderInvoice(OrderDto order);
    byte[] GenerateOrderReceipt(OrderDto order);
}

public class OrderPdfService : IOrderPdfService
{
    public byte[] GenerateOrderInvoice(OrderDto order)
    {
        using var ms = new MemoryStream();
        using var writer = new PdfWriter(ms);
  using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

     // Header
        document.Add(new Paragraph("INVOICE")
   .SetFontSize(24)
        .SetBold()
            .SetTextAlignment(TextAlignment.CENTER));

        document.Add(new Paragraph($"Order #{order.Id}")
.SetFontSize(16)
     .SetTextAlignment(TextAlignment.CENTER)
    .SetMarginBottom(20));

        // Customer Details
        document.Add(new Paragraph("Bill To:")
            .SetFontSize(14)
            .SetBold());
      
        document.Add(new Paragraph($"{order.FirstName} {order.LastName}"));
        document.Add(new Paragraph(order.EmailAddress));
        document.Add(new Paragraph(order.AddressLine));
        document.Add(new Paragraph($"{order.State}, {order.ZipCode}, {order.Country}"));
        document.Add(new Paragraph($"Phone: {order.PhoneNumber}")
  .SetMarginBottom(20));

        // Order Details
        document.Add(new Paragraph($"Order Date: {order.CreatedDate:dd MMM yyyy}")
    .SetMarginBottom(10));

        // Items Table
        Table table = new Table(4);
 table.SetWidth(UnitValue.CreatePercentValue(100));
   
        // Header
        table.AddHeaderCell("Item");
  table.AddHeaderCell("Quantity");
        table.AddHeaderCell("Price");
        table.AddHeaderCell("Total");

     // Items (if you have OrderItems collection)
      // foreach (var item in order.Items)
        // {
     //     table.AddCell(item.ProductName);
        //     table.AddCell(item.Quantity.ToString());
        //     table.AddCell($"${item.UnitPrice:N2}");
    //     table.AddCell($"${item.TotalPrice:N2}");
        // }

     document.Add(table);

        // Total
   document.Add(new Paragraph($"Total Amount: ${order.TotalPrice:N2}")
            .SetFontSize(18)
            .SetBold()
     .SetTextAlignment(TextAlignment.RIGHT)
     .SetMarginTop(20));

        // Footer
        document.Add(new Paragraph("Thank you for your business!")
       .SetTextAlignment(TextAlignment.CENTER)
        .SetMarginTop(30));

        document.Close();
        return ms.ToArray();
    }

    public byte[] GenerateOrderReceipt(OrderDto order)
    {
        // Similar implementation for receipt
    return GenerateOrderInvoice(order); // Simplified for now
    }
}
