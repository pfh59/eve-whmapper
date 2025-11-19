using Humanizer;
using System.Diagnostics.Metrics;
using WHMapper.Repositories.WHAccesses;
using WHMapper.Repositories.WHJumpLogs;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHNotes;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Repositories.WHSystems;


namespace WHMapper.Services.Metrics;

public class WHMapperStoreMetrics
{
    private readonly ILogger<WHMapperStoreMetrics> _logger;

    // WHMapperStoreMetrics
    // Users Metrics
    private Counter<int> UsersConnectedCounter { get; }
    private Counter<int> UsersDisconnectedCounter { get; }
    private int _totalUsers = 0;

    //Systems Metrics
    private Counter<int> SystemsAddedCounter { get; }
    private Counter<int> SystemsDeletedCounter { get; }
    private int _totalSystems = 0;


    //Links Metrics
    private Counter<int> LinksAddedCounter { get; }
    private Counter<int> LinksDeletedCounter { get; }
    private int _totalLinks = 0;

    //Maps Metrics
    private Counter<int> MapsCreatedCounter { get; }
    private Counter<int> MapsDeletedCounter { get; }
    private int _totalMaps = 0;

    //Signatures Metrics
    private Counter<int> SignaturesCreatedCounter { get; }
    private Counter<int> SignaturesDeletedCounter { get; }
    private int _totalSignatures = 0;

    //Notes Metrics
    private Counter<int> NotesCreatedCounter { get; }
    private Counter<int> NotesDeletedCounter { get; }
    private int _totalNotes = 0;

    //Jump Logs Metrics
    private Counter<int> JumpLogsCreatedCounter { get; }
    private Counter<int> JumpLogsDeletedCounter { get; }
    private int _totalJumpLogs = 0;


    public WHMapperStoreMetrics(ILogger<WHMapperStoreMetrics> logger, IMeterFactory meterFactory, IConfiguration configuration)
    {
        _logger = logger;

        var meter = meterFactory.Create(configuration["WHMapperStoreMeterName"] ??
                                        throw new ArgumentNullException(nameof(configuration), "WHMapperStore meter missing a name"));
                                        
        UsersConnectedCounter = meter.CreateCounter<int>("users-connected", "User", "Amount authenticated users connected");
        UsersDisconnectedCounter = meter.CreateCounter<int>("users-disconnected", "User", "Amount authenticated users disconnected");
        meter.CreateObservableGauge("total-users", () => _totalUsers, "User", "Total amount of authenticated users");

        SystemsAddedCounter = meter.CreateCounter<int>("systems-added", "System", "Amount of systems added");
        SystemsDeletedCounter = meter.CreateCounter<int>("systems-deleted", "System", "Amount of systems deleted");
        meter.CreateObservableGauge("total-systems", () => _totalSystems, "System", "Total amount of systems in WHMapper");

        LinksAddedCounter = meter.CreateCounter<int>("links-added", "Link", "Amount of links added");
        LinksDeletedCounter = meter.CreateCounter<int>("links-deleted", "Link", "Amount of links deleted");
        meter.CreateObservableGauge("total-links", () => _totalLinks, "Link", "Total amount of links in WHMapper");

        MapsCreatedCounter = meter.CreateCounter<int>("maps-created", "Map", "Amount of maps created");
        MapsDeletedCounter = meter.CreateCounter<int>("maps-deleted", "Map", "Amount of maps deleted");
        meter.CreateObservableGauge("total-maps", () => _totalMaps, "Map", "Total amount of maps in WHMapper");

        SignaturesCreatedCounter = meter.CreateCounter<int>("signatures-created", "Signature", "Amount of signatures created");
        SignaturesDeletedCounter = meter.CreateCounter<int>("signatures-deleted", "Signature", "Amount of signatures deleted");
        meter.CreateObservableGauge("total-signatures", () => _totalSignatures, "Signature", "Total amount of signatures in WHMapper");

        NotesCreatedCounter = meter.CreateCounter<int>("notes-created", "Note", "Amount of notes created");
        NotesDeletedCounter = meter.CreateCounter<int>("notes-deleted", "Note", "Amount of notes deleted");
        meter.CreateObservableGauge("total-notes", () => _totalNotes, "Note", "Total amount of notes in WHMapper");

        JumpLogsCreatedCounter = meter.CreateCounter<int>("jumplogs-created", "JumpLog", "Amount of jump logs created");
        JumpLogsDeletedCounter = meter.CreateCounter<int>("jumplogs-deleted", "JumpLog", "Amount of jump logs deleted");
        meter.CreateObservableGauge("total-jumplogs", () => _totalJumpLogs, "JumpLog", "Total amount of jump logs in WHMapper");
    }

    public async Task InitializeTotalsAsync(IWHMapRepository mapRepository, IWHSystemRepository systemRepository, IWHSystemLinkRepository linkRepository, IWHSignatureRepository signatureRepository, IWHNoteRepository noteRepository,IWHJumpLogRepository jumpLogRepository)
    {
        try
        {
            _logger.LogInformation("Initializing WHMapperStore metrics totals");
            _totalMaps = await mapRepository.GetCountAsync();
            _totalSystems = await systemRepository.GetCountAsync();
            _totalLinks = await linkRepository.GetCountAsync();
            _totalSignatures = await signatureRepository.GetCountAsync();
            _totalNotes = await noteRepository.GetCountAsync();
            _totalJumpLogs = await jumpLogRepository.GetCountAsync();

            _logger.LogInformation("WHMapperStore metrics totals initialized : {Systems} systems, {Maps} maps, {Links} links, {Signatures} signatures, {Notes} notes, {JumpLogs} jump logs", _totalSystems, _totalMaps, _totalLinks, _totalSignatures, _totalNotes, _totalJumpLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing WHMapperStore metrics totals");
        }
    }

    //WHMAPPER Metrics
    public void ConnectUser()
    {
        UsersConnectedCounter.Add(1);
        Interlocked.Increment(ref _totalUsers);
    }
    public void DisconnectUser()
    {
        UsersDisconnectedCounter.Add(1);
        Interlocked.Decrement(ref _totalUsers);
    }
    
    //Maps Metrics
    public void CreateMap()
    {
        MapsCreatedCounter.Add(1);
        Interlocked.Increment(ref _totalMaps);
    }
    public void DeleteMap()
    {
        MapsDeletedCounter.Add(1);
        Interlocked.Decrement(ref _totalMaps);
    }
    public void DeleteMaps(int delCount)
    {
        MapsDeletedCounter.Add(delCount);
        Interlocked.Add(ref _totalMaps, -delCount);
    }

    //Systems Metrics
    public void AddSystem()
    {
        SystemsAddedCounter.Add(1);
        Interlocked.Increment(ref _totalSystems);
    }
    public void DeleteSystem()
    {
        SystemsDeletedCounter.Add(1);
        Interlocked.Decrement(ref _totalSystems);
    }
    public void DeleteSystems(int delCount)
    {
        SystemsDeletedCounter.Add(delCount);
        Interlocked.Add(ref _totalSystems, -delCount);
    }

    //Links Metrics
    public void AddLink()
    {
        LinksAddedCounter.Add(1);
        Interlocked.Increment(ref _totalLinks);
    }
    public void DeleteLink()
    {
        LinksDeletedCounter.Add(1);
        Interlocked.Decrement(ref _totalLinks);
    }

    public void DeleteLinks(int delCount)
    {
        LinksDeletedCounter.Add(delCount);
        Interlocked.Add(ref _totalLinks, -delCount);
    }

    //Signatures Metrics
    public void CreateSignature()
    {
        SignaturesCreatedCounter.Add(1);
        Interlocked.Increment(ref _totalSignatures);
    }
    public void DeleteSignature()
    {
        SignaturesDeletedCounter.Add(1);
        Interlocked.Decrement(ref _totalSignatures);
    }
    public void DeleteSignatures(int delCount)
    {
        SignaturesDeletedCounter.Add(delCount);
        Interlocked.Add(ref _totalSignatures, -delCount);
    }

    //Notes Metrics
    public void CreateNote()
    {
        NotesCreatedCounter.Add(1);
        Interlocked.Increment(ref _totalNotes);
    }
    public void DeleteNote()
    {
        NotesDeletedCounter.Add(1);
        Interlocked.Decrement(ref _totalNotes);
    }
    public void DeleteNotes(int delCount)
    {
        NotesDeletedCounter.Add(delCount);
        Interlocked.Add(ref _totalNotes, -delCount);
    }

    //Jump Logs Metrics
    public void CreateJumpLog()
    {
        JumpLogsCreatedCounter.Add(1);
        Interlocked.Increment(ref _totalJumpLogs);
    }
    public void DeleteJumpLog()
    {
        JumpLogsDeletedCounter.Add(1);
        Interlocked.Decrement(ref _totalJumpLogs);
    }
    public void DeleteJumpLogs(int delCount)
    {
        JumpLogsDeletedCounter.Add(delCount);
        Interlocked.Add(ref _totalJumpLogs, -delCount);
    }
}
