using Contracts.Services;
using System.Text;

namespace Infrastructure.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private static readonly string root = AppDomain.CurrentDomain.BaseDirectory;

    public string ReadEmailTemplateContent(string emailTemplateName, string format = "html")
    {
        string filePath = Path.Combine(root, "EmailTemplates", $"{emailTemplateName}.{format}");
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(fs, Encoding.Default);
        string content = sr.ReadToEnd();
        sr.Close();

        return content;
    }
}