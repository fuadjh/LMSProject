using System.Text;

namespace Common.Validation;

public static class IranianIdentityNormalizer
{
    public static string NormalizeNationalCode(string? value)
    {
        return NormalizeDigitsOnly(value);
    }

    public static bool IsValidNationalCode(string? value)
    {
        var nationalCode = NormalizeNationalCode(value);

        if (nationalCode.Length != 10)
        {
            return false;
        }

        if (nationalCode.Distinct().Count() == 1)
        {
            return false;
        }

        var sum = 0;

        for (var index = 0; index < 9; index++)
        {
            sum += (nationalCode[index] - '0') * (10 - index);
        }

        var remainder = sum % 11;
        var controlDigit = nationalCode[9] - '0';

        return remainder < 2
            ? controlDigit == remainder
            : controlDigit == 11 - remainder;
    }

    public static string NormalizeMobile(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = NormalizeDigits(value)
            .Trim()
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("(", string.Empty, StringComparison.Ordinal)
            .Replace(")", string.Empty, StringComparison.Ordinal);

        if (normalized.StartsWith("+98", StringComparison.Ordinal))
        {
            normalized = $"0{normalized[3..]}";
        }
        else if (normalized.StartsWith("0098", StringComparison.Ordinal))
        {
            normalized = $"0{normalized[4..]}";
        }
        else if (normalized.StartsWith("98", StringComparison.Ordinal)
                 && normalized.Length == 12)
        {
            normalized = $"0{normalized[2..]}";
        }

        return NormalizeDigitsOnly(normalized);
    }

    public static bool IsValidMobile(string? value)
    {
        var mobile = NormalizeMobile(value);

        return mobile.Length == 11
               && mobile.StartsWith("09", StringComparison.Ordinal)
               && mobile.All(char.IsDigit);
    }

    public static string NormalizeUserName(string? value)
    {
        return NormalizeDigits(value).Trim();
    }

    public static string NormalizeDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var result = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            result.Append(character switch
            {
                '۰' => '0',
                '۱' => '1',
                '۲' => '2',
                '۳' => '3',
                '۴' => '4',
                '۵' => '5',
                '۶' => '6',
                '۷' => '7',
                '۸' => '8',
                '۹' => '9',

                '٠' => '0',
                '١' => '1',
                '٢' => '2',
                '٣' => '3',
                '٤' => '4',
                '٥' => '5',
                '٦' => '6',
                '٧' => '7',
                '٨' => '8',
                '٩' => '9',

                _ => character
            });
        }

        return result.ToString();
    }

    private static string NormalizeDigitsOnly(string? value)
    {
        return new string(
            NormalizeDigits(value)
                .Where(char.IsDigit)
                .ToArray());
    }
}