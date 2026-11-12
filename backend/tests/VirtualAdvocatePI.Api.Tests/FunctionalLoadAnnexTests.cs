using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Features.Documents;

namespace VirtualAdvocatePI.Api.Tests;

public sealed class FunctionalLoadAnnexTests
{
    [Fact]
    public void Build_WithNoRecords_ShowsEmptyMessagesAndSafetyNote()
    {
        var blocks = FunctionalLoadAnnex.Build([], [], []);

        Assert.Contains(blocks, b => b.Text == FunctionalLoadAnnex.SafetyNote);
        Assert.Contains(blocks, b => b.Text.Contains("No functional impact entries"));
        Assert.Contains(blocks, b => b.Text.Contains("No load or exposure records"));
    }

    [Fact]
    public void Build_WithRecords_IncludesConditionNameAndDetails()
    {
        var condition = new ClaimCondition { ConditionName = "Lumbar spondylosis" };
        var entry = new FunctionalImpactEntry
        {
            ConditionId = condition.Id,
            ActivityDomain = "SELF_CARE",
            GoodDayDescription = "Dress unaided",
            AverageDayDescription = "Dress slowly with rests",
            BadDayDescription = "Need help with socks",
            BadDayFrequency = "WEEKLY"
        };
        var record = new LoadExposureRecord
        {
            RecordType = "LIFTING_CARRYING",
            ActivityDescription = "Carrying stores",
            TypicalWeightKg = 25.5m,
            ConditionId = condition.Id
        };

        var text = string.Join("\n", FunctionalLoadAnnex.Build([entry], [record], [condition]).Select(b => b.Text));

        Assert.Contains("Lumbar spondylosis", text);
        Assert.Contains("Good day: Dress unaided", text);
        Assert.Contains("Average day: Dress slowly with rests", text);
        Assert.Contains("Bad day: Need help with socks", text);
        Assert.Contains("Lifting carrying", text);
        Assert.Contains("25.5 kg", text);
    }
}
