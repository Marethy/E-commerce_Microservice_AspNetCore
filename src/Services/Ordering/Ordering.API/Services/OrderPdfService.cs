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

        document.Add(new Paragraph("INVOICE")
            .SetFontSize(24)
   .SetBold()
            .SetTextAlignment(TextAlignment.CENTER));

      document.Add(new Paragraph($"Order #{order.Id}")
            .SetFontSize(16)
     .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(20));

        document.Add(new Paragraph("Bill To:")
      .SetFontSize(14)
            .SetBold());

        document.Add(new Paragraph($"{order.FirstName} {order.LastName}"));
   document.Add(new Paragraph(order.EmailAddress));
        document.Add(new Paragraph(order.ShippingAddress));
        document.Add(new Paragraph(order.InvoiceAddress)
        .SetMarginBottom(20));

        document.Add(new Paragraph($"Order Status: {order.Status}")
     .SetMarginBottom(10));

        Table table = new Table(4);
    table.SetWidth(UnitValue.CreatePercentValue(100));

        table.AddHeaderCell("Item");
        table.AddHeaderCell("Quantity");
        table.AddHeaderCell("Price");
        table.AddHeaderCell("Total");

        document.Add(table);

   document.Add(new Paragraph($"Total Amount: ${order.TotalPrice:N2}")
       .SetFontSize(18)
            .SetBold()
.SetTextAlignment(TextAlignment.RIGHT)
       .SetMarginTop(20));

        document.Add(new Paragraph("Thank you for your business!")
  .SetTextAlignment(TextAlignment.CENTER)
        .SetMarginTop(30));

        document.Close();
        return ms.ToArray();
    }

    public byte[] GenerateOrderReceipt(OrderDto order)
    {
        return GenerateOrderInvoice(order);
    }
}
