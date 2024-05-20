namespace SpinBladeArena.Performance;

public class PerformanceManager
{
    public CircularList<PerformanceData> PerformanceCounters { get; } = new(120);

    public PerformanceData Latest => PerformanceCounters.Count > 0 ? PerformanceCounters[^1] : PerformanceData.Zero;

    public PerformanceData Average
    {
        get
        {
            if (PerformanceCounters.Count == 0)
            {
                return PerformanceData.Zero;
            }

            PerformanceData sum = PerformanceData.Zero;
            foreach (PerformanceData data in PerformanceCounters)
            {
                sum += data;
            }
            return sum / PerformanceCounters.Count;
        }
    }
    
    public void Add(PerformanceData performanceData)
    {
        PerformanceCounters.Add(performanceData);
    }
}
