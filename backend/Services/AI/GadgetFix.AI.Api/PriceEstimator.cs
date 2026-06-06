namespace GadgetFix.AI.Api;

public record EstimateRequest(string DeviceType, string? Model, string Problem);

public record PhotoRequest(string ImageBase64, string? MimeType);
public record PhotoResult(string? DeviceType, string? Model, string Note, string? Damage);

public record ChatMessage(string Role, string Content);
public record ChatRequest(List<ChatMessage> Messages);
public record ChatReply(string Reply);

/// <summary>Варіант ремонту за типом запчастини.</summary>
public record PartOption(string Tier, decimal Min, decimal Max, string Description);

public record EstimateResult(
    decimal Min,
    decimal Max,
    string Currency,
    string Explanation,
    double Confidence,
    IReadOnlyList<PartOption> Options);

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
        decimal R(decimal v) => Math.Round(v, 0);

        // Послуги без заміни деталі (чистка, профілактика, ПЗ) — одна фіксована ціна
        var isService = new[] { "чист", "профілакт", "налаштув", "переустанов", "по", "оновлен" }
            .Any(k => problem.Contains(k, StringComparison.OrdinalIgnoreCase))
            && !problem.Contains("екран") && !problem.Contains("батар") && !problem.Contains("акумул");

        List<PartOption> options;
        if (isService)
        {
            options = new List<PartOption>
            {
                new("Послуга", R(Math.Min(mid * 0.5m, 400m)), R(Math.Min(mid * 0.8m, 800m)),
                    "Сервісна робота без заміни запчастин."),
            };
        }
        else
        {
            // Реалістична стеля ціни залежно від типу пристрою
            var cap = baseline >= 1000m ? 8000m : 6000m;
            decimal C(decimal v) => Math.Min(R(v), cap);
            options = new List<PartOption>
            {
                new("Китайська якість", C(mid * 0.6m), C(mid * 0.85m),
                    "Бюджетні сумісні запчастини. Гарантія коротша."),
                new("Середня якість", C(mid * 0.9m), C(mid * 1.15m),
                    "Якісні сумісні запчастини — оптимальне співвідношення ціна/якість."),
                new("Оригінальні запчастини", C(mid * 1.25m), C(mid * 1.6m),
                    "Оригінальні комплектуючі виробника. Максимальна якість і гарантія."),
            };
        }

        var min = options.Min(o => o.Min);
        var max = options.Max(o => o.Max);

        var confidence = matched.Count == 0 ? 0.4 : Math.Min(0.9, 0.55 + matched.Count * 0.15);
        var what = matched.Count == 0 ? "загальна діагностика" : string.Join(", ", matched);
        var model = string.IsNullOrWhiteSpace(request.Model) ? "" : $" ({request.Model})";
        var explanation =
            $"Орієнтовно для {request.DeviceType}{model}: {what}. " +
            "Точна ціна — після діагностики майстром.";

        return new EstimateResult(min, max, "грн", explanation, confidence, options);
    }
}
