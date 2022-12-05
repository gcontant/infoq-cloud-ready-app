using System.Text.RegularExpressions;

namespace Notifications.Services;

public static partial class EmailHelper
{
    public static bool IsEmail(string content)
    {
        return MyRegex().IsMatch(content);
    }

    [GeneratedRegex("\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z", RegexOptions.IgnoreCase, "en-CA")]
    private static partial Regex MyRegex();
}