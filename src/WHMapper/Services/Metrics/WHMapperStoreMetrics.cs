using System.Diagnostics.Metrics;


namespace WHMapper.Services.Metrics;

public class WHMapperStoreMetrics
{
    // WHMapperStoreMetrics
    private  Counter<int> UsersConnectedCounter { get; }
    private  Counter<int> UsersDisconnectedCounter { get; }
    private  UpDownCounter<int> TotalUsersUpDownCounter { get; }


    //Maps meters
    private  Counter<int> MapsAddedCounter { get; }
    private  Counter<int> MapsDeletedCounter { get; }
    private  Counter<int> MapsUpdatedCounter { get; }
    private  UpDownCounter<int> TotalMapsUpDownCounter { get; }


    //Systems meters
    private Counter<int> SystemsAddedCounter { get; }
    private Counter<int> SystemsDeletedCounter { get; }
    private Counter<int> SystemsUpdatedCounter { get; }
    private ObservableGauge<int> TotalSystemsGauge { get; }
    private int _totalSystems = 0;



    public WHMapperStoreMetrics(IMeterFactory meterFactory, IConfiguration configuration)
    {
        var meter = meterFactory.Create(configuration["WHMapperStoreMeterName"] ?? 
                                        throw new NullReferenceException("WHMapperStore meter missing a name"));
        
        UsersConnectedCounter = meter.CreateCounter<int>("users-connected","User","Amount authenticated users connected");
        UsersDisconnectedCounter = meter.CreateCounter<int>("users-disconnected","User","Amount authenticated users disconnected");
        TotalUsersUpDownCounter = meter.CreateUpDownCounter<int>("total-users","User","Total amount of authenticated users");

        MapsAddedCounter = meter.CreateCounter<int>("maps-added", "Map");
        MapsDeletedCounter = meter.CreateCounter<int>("maps-deleted", "Map");
        MapsUpdatedCounter = meter.CreateCounter<int>("maps-updated", "Map");
        TotalMapsUpDownCounter = meter.CreateUpDownCounter<int>("total-maps", "Map");

        SystemsAddedCounter = meter.CreateCounter<int>("systems-added", "System");
        SystemsDeletedCounter = meter.CreateCounter<int>("systems-deleted", "System");
        SystemsUpdatedCounter = meter.CreateCounter<int>("systems-updated", "System");
        TotalSystemsGauge = meter.CreateObservableGauge<int>("total-systems", () => _totalSystems);

    }

    //WHMAPPER Metrics
    public void ConnectUser()=>UsersConnectedCounter.Add(1);
    public void DisconnectUser()=>UsersDisconnectedCounter.Add(1);
    public void IncreaseTotalUsers()=>TotalUsersUpDownCounter.Add(1);
    public void DecreaseTotalUsers()=>TotalUsersUpDownCounter.Add(-1);

    //Maps Metrics
    public void AddMap()=>MapsAddedCounter.Add(1);
    public void DeleteMap()=>MapsDeletedCounter.Add(1);
    public void DeleteMaps(int delCount)=>MapsDeletedCounter.Add(delCount);
    public void UpdateMap()=>MapsUpdatedCounter.Add(1);
    public void IncreaseTotalMaps()=>TotalMapsUpDownCounter.Add(1);
    public void DecreaseTotalMaps()=>TotalMapsUpDownCounter.Add(-1);
    public void DecreaseTotalMaps(int count)=>TotalMapsUpDownCounter.Add(count);

    //Systems Metrics
    public void AddSystem()=>SystemsAddedCounter.Add(1);
    public void DeleteSystem()=>SystemsDeletedCounter.Add(1);
    public void DeleteSystems(int delCount)=>SystemsDeletedCounter.Add(delCount);
    public void UpdateSystem()=>SystemsUpdatedCounter.Add(1);
    public void IncreaseTotalSystems() => _totalSystems++;
    public void DecreaseTotalSystems() => _totalSystems--;
    public void DecreaseTotalSystems(int count) => _totalSystems -= count;

}
