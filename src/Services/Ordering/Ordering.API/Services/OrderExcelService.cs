using ClosedXML.Excel;
using Ordering.Application.Common.Models;

namespace Ordering.API.Services;

public interface IOrderExcelService
{
    byte[] ExportOrdersToExcel(IEnumerable<OrderDto> orders);
}

public class OrderExcelService : IOrderExcelService
{
    public byte[] ExportOrdersToExcel(IEnumerable<OrderDto> orders)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Orders");

        worksheet.Cell(1, 1).Value = "Order ID";
        worksheet.Cell(1, 2).Value = "Username";
        worksheet.Cell(1, 3).Value = "Full Name";
        worksheet.Cell(1, 4).Value = "Email";
        worksheet.Cell(1, 5).Value = "Shipping Address";
        worksheet.Cell(1, 6).Value = "Invoice Address";
        worksheet.Cell(1, 7).Value = "Total Price";
        worksheet.Cell(1, 8).Value = "Status";

        var headerRange = worksheet.Range(1, 1, 1, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        foreach (var order in orders)
        {
            worksheet.Cell(row, 1).Value = order.Id;
            worksheet.Cell(row, 2).Value = order.UserName;
            worksheet.Cell(row, 3).Value = $"{order.FirstName} {order.LastName}";
            worksheet.Cell(row, 4).Value = order.EmailAddress;
            worksheet.Cell(row, 5).Value = order.ShippingAddress;
            worksheet.Cell(row, 6).Value = order.InvoiceAddress;
            worksheet.Cell(row, 7).Value = order.TotalPrice;
            worksheet.Cell(row, 8).Value = order.Status.ToString();
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
