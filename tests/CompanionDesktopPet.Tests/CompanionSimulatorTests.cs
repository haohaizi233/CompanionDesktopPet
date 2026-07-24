using CompanionDesktopPet.Services;

namespace CompanionDesktopPet.Tests;

public sealed class CompanionSimulatorTests
{
    [Fact]
    public void ThirtyDaySimulation_IsDeterministicQuietAndCompletesStories()
    {
        var start = new DateTime(2026, 7, 1, 8, 0, 0);

        var first = CompanionSimulator.Run(start, days: 30, seed: 12345);
        var second = CompanionSimulator.Run(start, days: 30, seed: 12345);

        Assert.Equal(first.Entries.Select(entry => entry.SceneId), second.Entries.Select(entry => entry.SceneId));
        Assert.InRange(first.Entries.Count, 300, 1_500);
        Assert.True(first.MaximumDisplayedTextsPerDay <= 36, $"Max spoken/day: {first.MaximumDisplayedTextsPerDay}");
        Assert.True(first.ActionOnlyRatio >= 0.20, $"Action ratio: {first.ActionOnlyRatio:P1}");
        Assert.True(first.DirectRatio <= 0.25, $"Direct ratio: {first.DirectRatio:P1}");
        var enabledText = PersonaCorpus.All.Select(line => line.Text).ToHashSet(StringComparer.Ordinal);
        Assert.All(first.Entries.Where(entry => entry.ShouldDisplayText), entry => Assert.Contains(entry.Text, enabledText));
        Assert.Contains(first.Entries, entry => entry.SceneId.StartsWith("story:v2_story_", StringComparison.Ordinal)
                                                && entry.SceneId.EndsWith(":2", StringComparison.Ordinal));
    }
}
