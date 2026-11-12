using System.Globalization;
using VirtualAdvocatePI.Api.Domain.Claims;

namespace VirtualAdvocatePI.Api.Features.Documents;

public enum AnnexBlockKind
{
    Heading,
    SubHeading,
    Paragraph,
    Bullet
}

public sealed record AnnexBlock(AnnexBlockKind Kind, string Text);

/// <summary>
/// Builds the Functional and Load Evidence annex content shared by the Claim
/// Starter Pack and Doctor Guidance Pack (DOCX and PDF). The annex only restates
/// what the veteran recorded; it does not calculate impairment, assess capacity
/// or estimate compensation.
/// </summary>
public static class FunctionalLoadAnnex
{
    public const string SafetyNote =
        "This annex restates information recorded by the veteran about physically demanding service and current " +
        "function, for preparation only. It does not calculate GARP M impairment points, assess work capacity, " +
        "estimate compensation, provide legal or medical advice, or make a DVA decision.";

    public static List<AnnexBlock> Build(
        IReadOnlyList<FunctionalImpactEntry> impactEntries,
        IReadOnlyList<LoadExposureRecord> exposureRecords,
        IReadOnlyList<ClaimCondition> conditions)
    {
        var blocks = new List<AnnexBlock>
        {
            new(AnnexBlockKind.Heading, "Functional and load evidence annex"),
            new(AnnexBlockKind.Paragraph, SafetyNote)
        };

        var conditionNames = conditions.ToDictionary(c => c.Id, c => c.ConditionName);

        blocks.Add(new(AnnexBlockKind.SubHeading, "Good day / bad day functional impact"));

        if (impactEntries.Count == 0)
        {
            blocks.Add(new(AnnexBlockKind.Paragraph, "No functional impact entries have been recorded."));
        }
        else
        {
            foreach (var group in impactEntries
                .GroupBy(e => e.ConditionId)
                .OrderBy(g => ConditionName(conditionNames, g.Key), StringComparer.OrdinalIgnoreCase))
            {
                blocks.Add(new(AnnexBlockKind.Paragraph, $"Condition: {ConditionName(conditionNames, group.Key)}"));

                foreach (var entry in group.OrderBy(e => e.ActivityDomain, StringComparer.Ordinal))
                {
                    blocks.Add(new(AnnexBlockKind.Bullet, $"{Label(entry.ActivityDomain)}"));
                    AddIfPresent(blocks, "Good day", entry.GoodDayDescription);
                    AddIfPresent(blocks, "Average day", entry.AverageDayDescription);
                    AddIfPresent(blocks, "Bad day", entry.BadDayDescription);
                    blocks.Add(new(AnnexBlockKind.Paragraph, $"    How often bad days occur: {Label(entry.BadDayFrequency)}"));
                    AddIfPresent(blocks, "Aids or help used", entry.AidsOrHelpUsed);
                    AddIfPresent(blocks, "Notes", entry.Notes);
                }
            }
        }

        blocks.Add(new(AnnexBlockKind.SubHeading, "Physically demanding service exposure"));

        if (exposureRecords.Count == 0)
        {
            blocks.Add(new(AnnexBlockKind.Paragraph, "No load or exposure records have been recorded."));
        }
        else
        {
            foreach (var group in exposureRecords
                .GroupBy(r => r.RecordType)
                .OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                blocks.Add(new(AnnexBlockKind.Paragraph, Label(group.Key)));

                foreach (var record in group.OrderBy(r => r.ServicePeriodFrom))
                {
                    blocks.Add(new(AnnexBlockKind.Bullet, record.ActivityDescription));
                    AddIfPresent(blocks, "Condition", record.ConditionId is { } id ? ConditionName(conditionNames, id) : null);
                    AddIfPresent(blocks, "Body area", Label(record.BodyAreaAffected));
                    AddIfPresent(blocks, "Typical weight", Kg(record.TypicalWeightKg));
                    AddIfPresent(blocks, "Maximum weight", Kg(record.MaxWeightKg));
                    AddIfPresent(blocks, "Frequency", Label(record.Frequency));
                    AddIfPresent(blocks, "Duration per occasion", record.DurationPerOccasion);
                    AddIfPresent(blocks, "Repetitions", record.RepetitionsDescription);
                    AddIfPresent(blocks, "Service period", Period(record.ServicePeriodFrom, record.ServicePeriodTo));
                    AddIfPresent(blocks, "Years exposed", Num(record.YearsExposed));
                    AddIfPresent(blocks, "Lifts or occasions per day", Num(record.RepetitionsPerDay));
                    AddIfPresent(blocks, "Days per year", Num(record.DaysPerYear));
                    AddIfPresent(blocks, "Total days exposed", Num(record.TotalDaysExposed));
                    AddIfPresent(blocks, "Steps per occasion", Num(record.StepsPerOccasion));
                    AddIfPresent(blocks, "Minutes per occasion", Num(record.MinutesPerOccasion));
                    AddIfPresent(blocks, "Hours lifted", Num(record.HoursLifted));
                    AddIfPresent(blocks, "Equipment or context", record.EquipmentOrContext);
                    AddIfPresent(blocks, "Hazard type", record.HazardType is null ? null : Label(record.HazardType));
                    AddIfPresent(blocks, "Notes", record.Notes);

                    var totals = LoadExposureCalculator.Calculate(record);
                    AddIfPresent(blocks, "Calculated total weight", Num(totals.TotalWeightKg, " kg"));
                    AddIfPresent(blocks, "Calculated total steps", Num(totals.TotalSteps));
                    AddIfPresent(blocks, "Calculated total minutes", Num(totals.TotalMinutes));
                    AddIfPresent(blocks, "Calculated load factor (weight squared x hours)", Num(totals.LoadFactor));
                }

                AddTypeTotals(blocks, group.Key, group);
            }

            blocks.Add(new(AnnexBlockKind.Paragraph,
                "Calculated totals multiply the figures the veteran entered (for example weight x lifts per day x days). " +
                "They are arithmetic summaries only, are only shown where every figure needed was entered, and are not " +
                "an impairment calculation or a compensation estimate."));
        }

        return blocks;
    }

    private static void AddTypeTotals(List<AnnexBlock> blocks, string recordType, IEnumerable<LoadExposureRecord> records)
    {
        var totals = records.Select(LoadExposureCalculator.Calculate).ToList();

        void Sum(string label, Func<LoadExposureTotals, decimal?> pick, string suffix = "")
        {
            var values = totals.Select(pick).Where(v => v.HasValue).Select(v => v!.Value).ToList();

            if (values.Count > 0)
            {
                blocks.Add(new(AnnexBlockKind.Paragraph, $"    {label} ({Label(recordType)}): {Num(values.Sum(), suffix)}"));
            }
        }

        Sum("Total weight across records", t => t.TotalWeightKg, " kg");
        Sum("Total steps across records", t => t.TotalSteps);
        Sum("Total minutes across records", t => t.TotalMinutes);
        Sum("Total load factor across records", t => t.LoadFactor);
    }

    private static string? Num(decimal? value, string suffix = "") =>
        value is null ? null : value.Value.ToString("#,0.##", CultureInfo.InvariantCulture) + suffix;

    private static string ConditionName(Dictionary<Guid, string> names, Guid id) =>
        names.TryGetValue(id, out var name) ? name : "Condition no longer active";

    private static void AddIfPresent(List<AnnexBlock> blocks, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            blocks.Add(new(AnnexBlockKind.Paragraph, $"    {label}: {value.Trim()}"));
        }
    }

    private static string? Kg(decimal? value) =>
        value is null ? null : $"{value.Value.ToString("0.##", CultureInfo.InvariantCulture)} kg";

    private static string? Period(DateOnly? from, DateOnly? to) =>
        from is null && to is null
            ? null
            : $"{from?.ToString("yyyy-MM-dd") ?? "?"} to {to?.ToString("yyyy-MM-dd") ?? "?"}";

    private static string Label(string value)
    {
        var text = value.Replace('_', ' ').ToLowerInvariant();
        return text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];
    }
}
