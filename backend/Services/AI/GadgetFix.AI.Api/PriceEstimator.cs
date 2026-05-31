namespace GadgetFix.AI.Api;

public record EstimateRequest(string DeviceType, string? Model, string Problem);

public record EstimateResult(decimal Min, decimal Max, string Currency, string Explanation, double Confidence);

/// <summary>
/// Евристична оцінка вартості ремонту за типом гаджета та описом поломки.
/// Працює офлайн; за наявності LLM-ключа може бути замінена на виклик моделі.
/// </summary>
public class PriceEstimator
{
    private static readonly Dictionary<string, decimal> DeviceBase = new(StringComparer.OrdinalIgnoreCase)
    {
        ["smartphone"] = 800m,
        ["смартфон"] = 800m,
        ["laptop"] = 1000m,
        ["ноутбук"] = 1000m,
        ["tablet"] = 900m,
        ["планшет"] = 900m,
        ["watch"] = 600m,
        ["годинник"] = 600m,
    };

    // Ключові слова поломки -> (множник, опис)
    private static readonly (string[] Keywords, decimal Factor, string Label)[] ProblemRules =
    [
        (["екран", "дисплей", "розбит", "скло", "screen", "display"], 2.2m, "заміна екрана/дисплея"),
        (["акумулятор", "батаре", "battery", "тримає"], 1.2m, "заміна акумулятора"),
        (["вод", "залив", "волог", "water"], 1.8m, "ремонт після потрапляння вологи"),
        (["зарядк", "роз'єм", "разъем", "порт", "charging"], 1.1m, "ремонт роз'єму живлення"),
        (["камер", "camera"], 1.3m, "ремонт камери"),
        (["кнопк", "button"], 1.0m, "ремонт кнопок"),
        (["не вмика", "не включа", "не работает", "не запуск"], 1.6m, "комплексна діагностика та ремонт"),
    ];

    public EstimateResult Estimate(EstimateRequest request)
    {
        var baseline = DeviceBase.TryGetValue(request.DeviceType?.Trim() ?? "", out var b) ? b : 700m;

        var problem = request.Problem?.ToLowerInvariant() ?? "";
        decimal factor = 1.0m;
        var matched = new List<string>();

        foreach (var rule in ProblemRules)
        {
            if (rule.Keywords.Any(k => problem.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                factor = Math.Max(factor, rule.Factor);
                matched.Add(rule.Label);
            }
        }

        var mid = baseline * factor;
        var min = Math.Round(mid * 0.85m, 0);
        var max = Math.Round(mid * 1.25m, 0);

        var confidence = matched.Count == 0 ? 0.4 : Math.Min(0.9, 0.55 + matched.Count * 0.15);
        var what = matched.Count == 0 ? "загальна діагностика" : string.Join(", ", matched);
        var model = string.IsNullOrWhiteSpace(request.Model) ? "" : $" ({request.Model})";
        var explanation =
            $"Орієнтовно для {request.DeviceType}{model}: {what}. " +
            "Точна ціна — після діагностики майстром.";

        return new EstimateResult(min, max, "грн", explanation, confidence);
    }
}
