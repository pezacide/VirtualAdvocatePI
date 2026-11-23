namespace VirtualAdvocatePI.Api.Domain.Claims;

/// <summary>Calculated totals for a load exposure record. Null when the inputs needed are missing.</summary>
public sealed record LoadExposureTotals(
    decimal? EffectiveDays,
    decimal? TotalWeightKg,
    decimal? TotalSteps,
    decimal? TotalMinutes,
    decimal? LoadFactor);

/// <summary>
/// Arithmetic summaries of the veteran's own entries, following the supplied worksheets
/// (weight x lifts x days, total steps, total minutes, weight squared x hours). These are not
/// an impairment calculation, a work capacity assessment or a compensation estimate.
/// </summary>
public static class LoadExposureCalculator
{
    public static LoadExposureTotals Calculate(LoadExposureRecord record)
    {
        var days = EffectiveDays(record);

        return record.RecordType switch
        {
            "LIFTING_CARRYING" or "HEAVY_LOAD_CARRYING" or "NECK_SHOULDER_CARRIAGE" => new LoadExposureTotals(
                days,
                Multiply(record.TypicalWeightKg, record.RepetitionsPerDay, days),
                null,
                null,
                record.RecordType == "NECK_SHOULDER_CARRIAGE" ? null : LoadFactor(record)),

            "STAIRS_LADDERS_RUNGS" => new LoadExposureTotals(
                days,
                null,
                Multiply(record.StepsPerOccasion, record.RepetitionsPerDay, days),
                null,
                null),

            "KNEELING_SQUATTING" => new LoadExposureTotals(
                days,
                null,
                null,
                Multiply(record.MinutesPerOccasion, record.RepetitionsPerDay ?? 1m, days),
                null),

            _ => new LoadExposureTotals(days, null, null, null, null)
        };
    }

    /// <summary>Total days exposed: entered directly, or days per year x years exposed.</summary>
    public static decimal? EffectiveDays(LoadExposureRecord record)
    {
        if (record.TotalDaysExposed is { } total)
        {
            return total;
        }

        return record.DaysPerYear is { } perYear && record.YearsExposed is { } years
            ? Math.Round(perYear * years, 2)
            : null;
    }

    private static decimal? LoadFactor(LoadExposureRecord record) =>
        record.TypicalWeightKg is { } weight && record.HoursLifted is { } hours
            ? Math.Round(weight * weight * hours, 2)
            : null;

    private static decimal? Multiply(params decimal?[] values)
    {
        decimal product = 1m;

        foreach (var value in values)
        {
            if (value is not { } v)
            {
                return null;
            }

            product *= v;
        }

        return Math.Round(product, 2);
    }
}
