namespace Contracts.Services;

public interface IEmailTemplateService
{
    string ReadEmailTemplateContent(string emailTemplateName, string format = "html");
}