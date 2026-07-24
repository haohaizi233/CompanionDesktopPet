namespace CompanionDesktopPet.Services;

public sealed record SimulationEntry(
    DateTime At,
    string SceneId,
    SceneExpression Expression,
    bool ShouldDisplayText,
    string Text);

public sealed record SimulationResult(IReadOnlyList<SimulationEntry> Entries)
{
    public int MaximumDisplayedTextsPerDay => Entries
        .Where(entry => entry.ShouldDisplayText)
        .GroupBy(entry => entry.At.Date)
        .Select(group => group.Count())
        .DefaultIfEmpty(0)
        .Max();

    public double ActionOnlyRatio => Entries.Count == 0
        ? 0
        : Entries.Count(entry => entry.Expression == SceneExpression.ActionOnly) / (double)Entries.Count;

    public double DirectRatio => Entries.Count == 0
        ? 0
        : Entries.Count(entry => entry.Expression == SceneExpression.Direct) / (double)Entries.Count;
}

public static class CompanionSimulator
{
    public static SimulationResult Run(DateTime start, int days, int seed)
    {
        if (days <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(days));
        }

        var random = new Random(seed);
        var agent = new OfflineCompanionAgent();
        var dialogueScheduler = new DialogueScheduler(random);
        var end = start.AddDays(days);
        var entries = new List<SimulationEntry>(days * 40);

        Record(start, agent.Respond(CompanionEvent.Startup, start, random));
        var nextAutomatic = start + dialogueScheduler.NextDelay(start);
        var nextDayChange = start.Date.AddDays(1);

        while (true)
        {
            var nextStory = agent.State?.ActiveStories
                .Where(story => story.DueAt < end)
                .OrderBy(story => story.DueAt)
                .FirstOrDefault();
            var nextStoryAt = nextStory?.DueAt ?? DateTime.MaxValue;
            var nextAt = new[] { nextAutomatic, nextDayChange, nextStoryAt }.Min();
            if (nextAt >= end)
            {
                break;
            }

            if (nextAt == nextStoryAt)
            {
                Record(nextAt, agent.Respond(CompanionEvent.StoryTimerDue, nextAt, random));
            }
            else if (nextAt == nextDayChange)
            {
                Record(nextAt, agent.Respond(CompanionEvent.DayChanged, nextAt, random));
                nextDayChange = nextDayChange.AddDays(1);
            }
            else
            {
                Record(nextAt, agent.Respond(CompanionEvent.Automatic, nextAt, random));
                nextAutomatic = nextAt + dialogueScheduler.NextDelay(nextAt);
            }
        }

        return new SimulationResult(entries);

        void Record(DateTime at, AgentReply reply)
        {
            entries.Add(new SimulationEntry(
                at,
                reply.SceneId,
                reply.Expression,
                reply.ShouldDisplayText,
                reply.Text));
        }
    }
}
