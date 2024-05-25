namespace SpinBladeArena.Performance;

public class PerformanceManager
{
    public CircularList<PerformanceData> Datas { get; } = new(120);

    public PerformanceData Latest => Datas.Count > 0 ? Datas[^1] : PerformanceData.Zero;

    public PerformanceData Average
    {
        get
        {
            if (Datas.Count == 0)
            {
                return PerformanceData.Zero;
            }

            PerformanceData sum = PerformanceData.Zero;
            foreach (PerformanceData data in Datas)
            {
                sum += data;
            }
            return sum / Datas.Count;
        }
    }
    
    public void Add(PerformanceData performanceData)
    {
        Datas.Add(performanceData);
    }
}
