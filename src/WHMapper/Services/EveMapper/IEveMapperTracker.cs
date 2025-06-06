﻿using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveMapper.EveEntity;

namespace WHMapper.Services.EveMapper;

public interface IEveMapperTracker : IAsyncDisposable
{
    event Func<int,EveLocation?,EveLocation, Task>? SystemChanged;
    event Func<int,Ship?, Ship,Task>? ShipChanged;
    Task StartTracking(int accountID);
    Task StopTracking(int accountID);

}
