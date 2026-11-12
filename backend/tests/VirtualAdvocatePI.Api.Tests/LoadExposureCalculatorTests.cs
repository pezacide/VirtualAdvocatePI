using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Features.Documents;

namespace VirtualAdvocatePI.Api.Tests;

public sealed class LoadExposureCalculatorTests
{
    [Fact]
    public void Lifting_TotalWeight_IsWeightTimesLiftsTimesDaysPerYearTimesYears()
    {
        var record = new LoadExposureRecord
        {
            RecordType = "LIFTING_CARRYING",
            TypicalWeightKg = 20m,
            RepetitionsPerDay = 10m,
            DaysPerYear = 200m,
            YearsExposed = 3m,
            HoursLifted = 2.5m
        };

        var totals = LoadExposureCalculator.Calculate(record);

        Assert.Equal(600m, totals.EffectiveDays);
        Assert.Equal(20m * 10m * 600m, totals.TotalWeightKg);
        Assert.Equal(20m * 20m * 2.5m, totals.LoadFactor);
    }

    [Fact]
    public void TotalDaysExposed_TakesPrecedenceOverDaysPerYearTimesYears()
    {
        var record = new LoadExposureRecord
        {
            RecordType = "HEAVY_LOAD_CARRYING",
            TypicalWeightKg = 45m,
            RepetitionsPerDay = 12m,
            TotalDaysExposed = 15m,
            DaysPerYear = 100m,
            YearsExposed = 10m
        };

        Assert.Equal(45m * 12m * 15m, LoadExposureCalculator.Calculate(record).TotalWeightKg);
    }

    [Fact]
    public void MissingInputs_GiveNullTotals_NotZero()
    {
        var record = new LoadExposureRecord
        {
            RecordType = "LIFTING_CARRYING",
            TypicalWeightKg = 20m,
            RepetitionsPerDay = 10m
        };

        var totals = LoadExposureCalculator.Calculate(record);

        Assert.Null(totals.EffectiveDays);
        Assert.Null(totals.TotalWeightKg);
        Assert.Null(totals.LoadFactor);
    }

    [Fact]
    public void Stairs_TotalSteps_AndKneeling_TotalMinutes()
    {
        var stairs = new LoadExposureRecord
        {
            RecordType = "STAIRS_LADDERS_RUNGS",
            StepsPerOccasion = 40m,
            RepetitionsPerDay = 6m,
            TotalDaysExposed = 100m
        };
        var kneeling = new LoadExposureRecord
        {
            RecordType = "KNEELING_SQUATTING",
            MinutesPerOccasion = 30m,
            TotalDaysExposed = 50m
        };

        Assert.Equal(24000m, LoadExposureCalculator.Calculate(stairs).TotalSteps);
        Assert.Equal(1500m, LoadExposureCalculator.Calculate(kneeling).TotalMinutes);
    }

    [Fact]
    public void HazardExposure_HasNoTotals()
    {
        var totals = LoadExposureCalculator.Calculate(new LoadExposureRecord
        {
            RecordType = "HAZARD_EXPOSURE",
            TypicalWeightKg = 10m,
            TotalDaysExposed = 5m
        });

        Assert.Null(totals.TotalWeightKg);
        Assert.Null(totals.TotalSteps);
        Assert.Null(totals.TotalMinutes);
        Assert.Null(totals.LoadFactor);
    }

    [Fact]
    public void Annex_ShowsCalculatedTotalsAndPerTypeSum()
    {
        var records = new[]
        {
            new LoadExposureRecord { RecordType = "LIFTING_CARRYING", ActivityDescription = "Stores", TypicalWeightKg = 10m, RepetitionsPerDay = 5m, TotalDaysExposed = 20m },
            new LoadExposureRecord { RecordType = "LIFTING_CARRYING", ActivityDescription = "Ammo", TypicalWeightKg = 20m, RepetitionsPerDay = 2m, TotalDaysExposed = 10m }
        };

        var text = string.Join("\n", FunctionalLoadAnnex.Build([], records, []).Select(b => b.Text));

        Assert.Contains("Calculated total weight: 1,000 kg", text);
        Assert.Contains("Calculated total weight: 400 kg", text);
        Assert.Contains("Total weight across records (Lifting carrying): 1,400 kg", text);
        Assert.Contains("not an impairment calculation", text);
    }
}
