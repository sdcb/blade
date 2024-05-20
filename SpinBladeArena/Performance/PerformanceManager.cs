using SpinBladeArena.LogicCenter;

namespace SpinBladeArena.Performance;

public class PerformanceManager
{
    private readonly CircularList<PerformanceData> _performanceCounters = new(120);

    public PerformanceData Latest => _performanceCounters.Count > 0 ? _performanceCounters[^1] : PerformanceData.Zero;

    public PerformanceData Average
    {
        get
        {
            if (_performanceCounters.Count == 0)
            {
                return PerformanceData.Zero;
            }

            PerformanceData sum = PerformanceData.Zero;
            foreach (PerformanceData data in _performanceCounters)
            {
                sum += data;
            }
            return sum / _performanceCounters.Count;
        }
    }
    
    public void Add(PerformanceData performanceData)
    {
        _performanceCounters.Add(performanceData);
    }
}
