using Shared.Common.Constants;
namespace Shared.Common.Helpers;

public static class PermissionHelper
{
    public static string GetPermission(FunctionCode functionCode, CommandCode commandCode)
    {
        return string.Join(".", functionCode, commandCode);
    }
}