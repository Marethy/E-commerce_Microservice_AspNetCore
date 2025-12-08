namespace IDP.Common;

public class SMTPEmailSetting
{
    public string DisplayName { get; set; } = string.Empty;
    public bool EnableVerification { get; set; }
    public string From { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public bool UseSsl { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
