namespace BlazorBlueprint.Components.CurrencyInput;

/// <summary>
/// Provides a catalog of supported currencies.
/// </summary>
public static class CurrencyCatalog
{
    /// <summary>
    /// Dictionary of supported currencies by ISO 4217 code.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, CurrencyDefinition> Currencies = new Dictionary<string, CurrencyDefinition>(StringComparer.OrdinalIgnoreCase)
    {
        // North America
        ["USD"] = new() { Code = "USD", Symbol = "$", DecimalPlaces = 2, CultureName = "en-US", Name = "US Dollar" },
        ["CAD"] = new() { Code = "CAD", Symbol = "$", DecimalPlaces = 2, CultureName = "en-CA", Name = "Canadian Dollar" },
        ["MXN"] = new() { Code = "MXN", Symbol = "$", DecimalPlaces = 2, CultureName = "es-MX", Name = "Mexican Peso" },

        // Europe
        ["EUR"] = new() { Code = "EUR", Symbol = "\u20AC", DecimalPlaces = 2, CultureName = "de-DE", Name = "Euro" },
        ["GBP"] = new() { Code = "GBP", Symbol = "\u00A3", DecimalPlaces = 2, CultureName = "en-GB", Name = "British Pound" },
        ["CHF"] = new() { Code = "CHF", Symbol = "CHF", DecimalPlaces = 2, CultureName = "de-CH", Name = "Swiss Franc", SymbolBefore = false },
        ["SEK"] = new() { Code = "SEK", Symbol = "kr", DecimalPlaces = 2, CultureName = "sv-SE", Name = "Swedish Krona", SymbolBefore = false },
        ["NOK"] = new() { Code = "NOK", Symbol = "kr", DecimalPlaces = 2, CultureName = "nb-NO", Name = "Norwegian Krone", SymbolBefore = false },
        ["DKK"] = new() { Code = "DKK", Symbol = "kr", DecimalPlaces = 2, CultureName = "da-DK", Name = "Danish Krone", SymbolBefore = false },
        ["PLN"] = new() { Code = "PLN", Symbol = "z\u0142", DecimalPlaces = 2, CultureName = "pl-PL", Name = "Polish Zloty", SymbolBefore = false },
        ["CZK"] = new() { Code = "CZK", Symbol = "K\u010D", DecimalPlaces = 2, CultureName = "cs-CZ", Name = "Czech Koruna", SymbolBefore = false },
        ["RUB"] = new() { Code = "RUB", Symbol = "\u20BD", DecimalPlaces = 2, CultureName = "ru-RU", Name = "Russian Ruble", SymbolBefore = false },

        // Asia Pacific
        ["JPY"] = new() { Code = "JPY", Symbol = "\u00A5", DecimalPlaces = 0, CultureName = "ja-JP", Name = "Japanese Yen" },
        ["CNY"] = new() { Code = "CNY", Symbol = "\u00A5", DecimalPlaces = 2, CultureName = "zh-CN", Name = "Chinese Yuan" },
        ["KRW"] = new() { Code = "KRW", Symbol = "\u20A9", DecimalPlaces = 0, CultureName = "ko-KR", Name = "South Korean Won" },
        ["INR"] = new() { Code = "INR", Symbol = "\u20B9", DecimalPlaces = 2, CultureName = "en-IN", Name = "Indian Rupee" },
        ["AUD"] = new() { Code = "AUD", Symbol = "$", DecimalPlaces = 2, CultureName = "en-AU", Name = "Australian Dollar" },
        ["NZD"] = new() { Code = "NZD", Symbol = "$", DecimalPlaces = 2, CultureName = "en-NZ", Name = "New Zealand Dollar" },
        ["SGD"] = new() { Code = "SGD", Symbol = "$", DecimalPlaces = 2, CultureName = "en-SG", Name = "Singapore Dollar" },
        ["HKD"] = new() { Code = "HKD", Symbol = "$", DecimalPlaces = 2, CultureName = "zh-HK", Name = "Hong Kong Dollar" },
        ["TWD"] = new() { Code = "TWD", Symbol = "NT$", DecimalPlaces = 2, CultureName = "zh-TW", Name = "Taiwan Dollar" },
        ["THB"] = new() { Code = "THB", Symbol = "\u0E3F", DecimalPlaces = 2, CultureName = "th-TH", Name = "Thai Baht" },
        ["MYR"] = new() { Code = "MYR", Symbol = "RM", DecimalPlaces = 2, CultureName = "ms-MY", Name = "Malaysian Ringgit" },
        ["IDR"] = new() { Code = "IDR", Symbol = "Rp", DecimalPlaces = 0, CultureName = "id-ID", Name = "Indonesian Rupiah" },
        ["PHP"] = new() { Code = "PHP", Symbol = "\u20B1", DecimalPlaces = 2, CultureName = "en-PH", Name = "Philippine Peso" },
        ["VND"] = new() { Code = "VND", Symbol = "\u20AB", DecimalPlaces = 0, CultureName = "vi-VN", Name = "Vietnamese Dong", SymbolBefore = false },

        // South America
        ["BRL"] = new() { Code = "BRL", Symbol = "R$", DecimalPlaces = 2, CultureName = "pt-BR", Name = "Brazilian Real" },
        ["ARS"] = new() { Code = "ARS", Symbol = "$", DecimalPlaces = 2, CultureName = "es-AR", Name = "Argentine Peso" },
        ["CLP"] = new() { Code = "CLP", Symbol = "$", DecimalPlaces = 0, CultureName = "es-CL", Name = "Chilean Peso" },
        ["COP"] = new() { Code = "COP", Symbol = "$", DecimalPlaces = 0, CultureName = "es-CO", Name = "Colombian Peso" },

        // Middle East & Africa
        ["AED"] = new() { Code = "AED", Symbol = "\u062F.\u0625", DecimalPlaces = 2, CultureName = "ar-AE", Name = "UAE Dirham" },
        ["SAR"] = new() { Code = "SAR", Symbol = "\u0631.\u0633", DecimalPlaces = 2, CultureName = "ar-SA", Name = "Saudi Riyal" },
        ["ILS"] = new() { Code = "ILS", Symbol = "\u20AA", DecimalPlaces = 2, CultureName = "he-IL", Name = "Israeli Shekel" },
        ["TRY"] = new() { Code = "TRY", Symbol = "\u20BA", DecimalPlaces = 2, CultureName = "tr-TR", Name = "Turkish Lira" },
        ["ZAR"] = new() { Code = "ZAR", Symbol = "R", DecimalPlaces = 2, CultureName = "en-ZA", Name = "South African Rand" },
        ["NGN"] = new() { Code = "NGN", Symbol = "\u20A6", DecimalPlaces = 2, CultureName = "en-NG", Name = "Nigerian Naira" },
        ["EGP"] = new() { Code = "EGP", Symbol = "\u00A3", DecimalPlaces = 2, CultureName = "ar-EG", Name = "Egyptian Pound" },
        ["KWD"] = new() { Code = "KWD", Symbol = "\u062F.\u0643", DecimalPlaces = 3, CultureName = "ar-KW", Name = "Kuwaiti Dinar" },
        ["BHD"] = new() { Code = "BHD", Symbol = "\u0628.\u062F", DecimalPlaces = 3, CultureName = "ar-BH", Name = "Bahraini Dinar" },
        ["OMR"] = new() { Code = "OMR", Symbol = "\u0631.\u0639", DecimalPlaces = 3, CultureName = "ar-OM", Name = "Omani Rial" },
    };

    /// <summary>
    /// Gets a currency by its ISO code, or USD as fallback.
    /// </summary>
    public static CurrencyDefinition GetCurrency(string code) =>
        Currencies.TryGetValue(code, out var currency) ? currency : Currencies["USD"];

    /// <summary>
    /// Gets all available currency codes.
    /// </summary>
    public static IEnumerable<string> GetAllCurrencyCodes() => Currencies.Keys;
}
