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
    private UpDownCounter<int> TotalUsersUpDownCounter { get; }

    //Systems Metrics
    private Counter<int> SystemsAddedCounter { get; }
    private Counter<int> SystemsDeletedCounter { get; }
    private UpDownCounter<int> TotalSystemsUpDownCounter  { get; }


    //Links Metrics
    private Counter<int> LinksAddedCounter { get; }
    private Counter<int> LinksDeletedCounter { get; }
    private UpDownCounter<int> TotalLinksUpDownCounter { get; }

    //Maps Metrics
    private Counter<int> MapsCreatedCounter { get; }
    private Counter<int> MapsDeletedCounter { get; }
    private UpDownCounter<int> TotalMapsUpDownCounter { get; }

    //Signatures Metrics
    private Counter<int> SignaturesCreatedCounter { get; }
    private Counter<int> SignaturesDeletedCounter { get; }
    private UpDownCounter<int> TotalSignaturesUpDownCounter { get; }

    //Notes Metrics
    private Counter<int> NotesCreatedCounter { get; }
    private Counter<int> NotesDeletedCounter { get; }
    private UpDownCounter<int> TotalNotesUpDownCounter { get; }

    //Jump Logs Metrics
    private Counter<int> JumpLogsCreatedCounter { get; }
    private Counter<int> JumpLogsDeletedCounter { get; }
    private UpDownCounter<int> TotalJumpLogsUpDownCounter { get; }


    public WHMapperStoreMetrics(ILogger<WHMapperStoreMetrics> logger, IMeterFactory meterFactory, IConfiguration configuration)
    {
        _logger = logger;

        var meter = meterFactory.Create(configuration["WHMapperStoreMeterName"] ??
                                        throw new NullReferenceException("WHMapperStore meter missing a name"));

        

        UsersConnectedCounter = meter.CreateCounter<int>("users-connected", "User", "Amount authenticated users connected");
        UsersDisconnectedCounter = meter.CreateCounter<int>("users-disconnected", "User", "Amount authenticated users disconnected");
        TotalUsersUpDownCounter = meter.CreateUpDownCounter<int>("total-users", "User", "Total amount of authenticated users");

        SystemsAddedCounter = meter.CreateCounter<int>("systems-added", "System", "Amount of systems added");
        SystemsDeletedCounter = meter.CreateCounter<int>("systems-deleted", "System", "Amount of systems deleted");
        TotalSystemsUpDownCounter = meter.CreateUpDownCounter<int>("total-systems", "System", "Total amount of systems in WHMapper");

        LinksAddedCounter = meter.CreateCounter<int>("links-added", "Link", "Amount of links added");
        LinksDeletedCounter = meter.CreateCounter<int>("links-deleted", "Link", "Amount of links deleted");
        TotalLinksUpDownCounter = meter.CreateUpDownCounter<int>("total-links", "Link", "Total amount of links in WHMapper");

        MapsCreatedCounter = meter.CreateCounter<int>("maps-created", "Map", "Amount of maps created");
        MapsDeletedCounter = meter.CreateCounter<int>("maps-deleted", "Map", "Amount of maps deleted");
        TotalMapsUpDownCounter = meter.CreateUpDownCounter<int>("total-maps", "Map", "Total amount of maps in WHMapper");

        SignaturesCreatedCounter = meter.CreateCounter<int>("signatures-created", "Signature", "Amount of signatures created");
        SignaturesDeletedCounter = meter.CreateCounter<int>("signatures-deleted", "Signature", "Amount of signatures deleted");
        TotalSignaturesUpDownCounter = meter.CreateUpDownCounter<int>("total-signatures", "Signature", "Total amount of signatures in WHMapper");

        NotesCreatedCounter = meter.CreateCounter<int>("notes-created", "Note", "Amount of notes created");
        NotesDeletedCounter = meter.CreateCounter<int>("notes-deleted", "Note", "Amount of notes deleted");
        TotalNotesUpDownCounter = meter.CreateUpDownCounter<int>("total-notes", "Note", "Total amount of notes in WHMapper");

        JumpLogsCreatedCounter = meter.CreateCounter<int>("jumplogs-created", "JumpLog", "Amount of jump logs created");
        JumpLogsDeletedCounter = meter.CreateCounter<int>("jumplogs-deleted", "JumpLog", "Amount of jump logs deleted");
        TotalJumpLogsUpDownCounter = meter.CreateUpDownCounter<int>("total-jumplogs", "JumpLog", "Total amount of jump logs in WHMapper");
    }

    public async Task InitializeTotalsAsync(IWHMapRepository mapRepository, IWHSystemRepository systemRepository, IWHSystemLinkRepository linkRepository, IWHSignatureRepository signatureRepository, IWHNoteRepository noteRepository,IWHJumpLogRepository jumpLogRepository)
    {
        try
        {
            _logger.LogInformation("Initializing WHMapperStore metrics totals");
            int totalMaps = await mapRepository.GetCountAsync();
            int totalSystems = await systemRepository.GetCountAsync();
            int totalLinks = await linkRepository.GetCountAsync();
            int totalSignatures = await signatureRepository.GetCountAsync();
            int totalNotes = await noteRepository.GetCountAsync();
            int totalJumpLogs = await jumpLogRepository.GetCountAsync();

            TotalMapsUpDownCounter.Add(totalMaps);
            TotalSystemsUpDownCounter.Add(totalSystems);
            TotalLinksUpDownCounter.Add(totalLinks);
            TotalSignaturesUpDownCounter.Add(totalSignatures);
            TotalNotesUpDownCounter.Add(totalNotes);
            TotalJumpLogsUpDownCounter.Add(totalJumpLogs);

            _logger.LogInformation("WHMapperStore metrics totals initialized : {Systems} systems, {Maps} maps, {Links} links, {Signatures} signatures, {Notes} notes", totalSystems, totalMaps, totalLinks, totalSignatures, totalNotes);
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
        TotalUsersUpDownCounter.Add(1);
    }
    public void DisconnectUser()
    {
        UsersDisconnectedCounter.Add(1);
        TotalUsersUpDownCounter.Add(-1);
    }
    
    //Maps Metrics
    public void CreateMap()
    {
        MapsCreatedCounter.Add(1);
        TotalMapsUpDownCounter.Add(1);
    }
    public void DeleteMap()
    {
        MapsDeletedCounter.Add(1);
        TotalMapsUpDownCounter.Add(-1);
    }
    public void DeleteMaps(int delCount)
    {
        MapsDeletedCounter.Add(delCount);
        TotalMapsUpDownCounter.Add(-delCount);
    }

    //Systems Metrics
    public void AddSystem()
    {
        SystemsAddedCounter.Add(1);
        TotalSystemsUpDownCounter.Add(1);
    }
    public void DeleteSystem()
    {
        SystemsDeletedCounter.Add(1);
        TotalSystemsUpDownCounter.Add(-1);
    }
    public void DeleteSystems(int delCount)
    {
        SystemsDeletedCounter.Add(delCount);
        TotalSystemsUpDownCounter.Add(-delCount);
    }

    //Links Metrics
    public void AddLink()
    {
        LinksAddedCounter.Add(1);
        TotalLinksUpDownCounter.Add(1);
    }
    public void DeleteLink()
    {
        LinksDeletedCounter.Add(1);
        TotalLinksUpDownCounter.Add(-1);
    }

    public void DeleteLinks(int delCount)
    {
        LinksDeletedCounter.Add(delCount);
        TotalLinksUpDownCounter.Add(-delCount);
    }

    //Signatures Metrics
    public void CreateSignature()
    {
        SignaturesCreatedCounter.Add(1);
        TotalSignaturesUpDownCounter.Add(1);
    }
    public void DeleteSignature()
    {
        SignaturesDeletedCounter.Add(1);
        TotalSignaturesUpDownCounter.Add(-1);
    }
    public void DeleteSignatures(int delCount)
    {
        SignaturesDeletedCounter.Add(delCount);
        TotalSignaturesUpDownCounter.Add(-delCount);
    }

    //Notes Metrics
    public void CreateNote()
    {
        NotesCreatedCounter.Add(1);
        TotalNotesUpDownCounter.Add(1);
    }
    public void DeleteNote()
    {
        NotesDeletedCounter.Add(1);
        TotalNotesUpDownCounter.Add(-1);
    }
    public void DeleteNotes(int delCount)
    {
        NotesDeletedCounter.Add(delCount);
        TotalNotesUpDownCounter.Add(-delCount);
    }

    //Jump Logs Metrics
    public void CreateJumpLog()
    {
        JumpLogsCreatedCounter.Add(1);
        TotalJumpLogsUpDownCounter.Add(1);
    }
    public void DeleteJumpLog()
    {
        JumpLogsDeletedCounter.Add(1);
        TotalJumpLogsUpDownCounter.Add(-1);
    }
    public void DeleteJumpLogs(int delCount)
    {
        JumpLogsDeletedCounter.Add(delCount);
        TotalJumpLogsUpDownCounter.Add(-delCount);
    }
}
