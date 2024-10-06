using System;
using System.Diagnostics.Metrics;


namespace WHMapper.Services.Metrics;

public class WHMapperStoreMetrics
{
    // WHMapperStoreMetrics
    private  Counter<int> UsersConnectedCounter { get; }
    private  Counter<int> UsersDisconnectedCounter { get; }
    private  UpDownCounter<int> TotalUsersUpDownCounter { get; }


    public WHMapperStoreMetrics(IMeterFactory meterFactory, IConfiguration configuration)
    {
        var meter = meterFactory.Create(configuration["WHMapperStoreMeterName"] ?? 
                                        throw new NullReferenceException("BookStore meter missing a name"));
        
        UsersConnectedCounter = meter.CreateCounter<int>("users-connected");
        UsersDisconnectedCounter = meter.CreateCounter<int>("users-disconnected");
        TotalUsersUpDownCounter = meter.CreateUpDownCounter<int>("total-users");
    }

    //WHMAPPER Metrics
    public void ConnectUser()=>UsersConnectedCounter.Add(1);
    public void DisconnectUser()=>UsersDisconnectedCounter.Add(1);
    public void IncreaseTotalUsers()=>TotalUsersUpDownCounter.Add(1);
    public void DecreaseTotalUsers()=>TotalUsersUpDownCounter.Add(-1);
}
