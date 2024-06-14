using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Assets
{
    public enum AssetLocationFlag
    {
        AssetSafety, 
        AutoFit,
        BoosterBay, 
        Cargo, 
        CorporationGoalDeliveries, 
        CorpseBay,
        Deliveries,
        DroneBay, 
        FighterBay,
        FighterTube0, 
        FighterTube1, 
        FighterTube2, 
        FighterTube3, 
        FighterTube4, 
        FleetHangar,
        FrigateEscapeBay, 
        Hangar, 
        HangarAll, 
        HiSlot0,
        HiSlot1, 
        HiSlot2, 
        HiSlot3, 
        HiSlot4, 
        HiSlot5, 
        HiSlot6, 
        HiSlot7, 
        HiddenModifiers,
        Implant, 
        LoSlot0, 
        LoSlot1, 
        LoSlot2, 
        LoSlot3, 
        LoSlot4, 
        LoSlot5, 
        LoSlot6, 
        LoSlot7, 
        Locked, 
        MedSlot0, 
        MedSlot1, 
        MedSlot2, 
        MedSlot3, 
        MedSlot4,
        MedSlot5, 
        MedSlot6,
        MedSlot7, 
        MobileDepotHold, 
        QuafeBay, 
        RigSlot0, 
        RigSlot1, 
        RigSlot2, 
        RigSlot3, 
        RigSlot4, 
        RigSlot5, 
        RigSlot6, 
        RigSlot7, 
        ShipHangar, 
        Skill, 
        SpecializedAmmoHold, 
        SpecializedAsteroidHold, 
        SpecializedCommandCenterHold, 
        SpecializedFuelBay, 
        SpecializedGasHold, 
        SpecializedIceHold, 
        SpecializedIndustrialShipHold, 
        SpecializedLargeShipHold, 
        SpecializedMaterialBay, 
        SpecializedMediumShipHold, 
        SpecializedMineralHold, 
        SpecializedOreHold,
         SpecializedPlanetaryCommoditiesHold, 
         SpecializedSalvageHold, 
         SpecializedShipHold, 
         SpecializedSmallShipHold, 
         StructureDeedBay, 
         SubSystemBay, 
         SubSystemSlot0, 
         SubSystemSlot1, 
         SubSystemSlot2, 
         SubSystemSlot3, 
         SubSystemSlot4, 
         SubSystemSlot5, 
         SubSystemSlot6, 
         SubSystemSlot7, 
         Unlocked, 
         Wardrobe
    }

    public enum AssetLocationType
    {
        station,
        solar_system,
        item,
        other
    }

    public class Asset
    {
        [JsonPropertyName("is_blueprint_copy")]
        public bool IsBluePrintCopy { get; set; }

        [JsonPropertyName("is_singleton")]
        public bool IsSingleton { get; set; }

        [JsonPropertyName("item_id")]
        public long ItemId { get; set; }

        [JsonPropertyName("location_flag")]
        public string? LocationFlag { get; set; }

        [JsonPropertyName("location_id")]
        public long LocationId { get; set; }

        [JsonPropertyName("location_type")]
        public string? LocationType { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("type_id")]
        public int TypeId { get; set; }

/*
        [JsonConstructor]
        public Asset(bool isBluePrintCopy, bool isSingleton, long itemId, string locationFlag, long locationId, string locationType, int quantity, int typeId) =>
        (IsBluePrintCopy, IsSingleton, ItemId, LocationFlag, LocationId, LocationType, Quantity, TypeId) = (isBluePrintCopy, isSingleton, itemId, locationFlag, locationId, locationType, quantity, typeId);
            //(IsBluePrintCopy, IsSingleton, ItemId, LocationFlag, LocationId, LocationType, Quantity, TypeId) = (isBluePrintCopy, isSingleton, itemId, (AssetLocationFlag) Enum.Parse(typeof(AssetLocationFlag), locationFlag), locationId, (AssetLocationType) Enum.Parse(typeof(AssetLocationType), locationType), quantity, typeId);

        [JsonConstructor]
        public Asset(bool isSingleton, long itemId, string locationFlag, long locationId, string locationType, int quantity, int typeId) =>
        (IsSingleton, ItemId, LocationFlag, LocationId, LocationType, Quantity, TypeId) = (isSingleton, itemId, locationFlag, locationId, locationType, quantity, typeId);
           // (IsSingleton, ItemId, LocationFlag, LocationId, LocationType, Quantity, TypeId) = (isSingleton, itemId, (AssetLocationFlag) Enum.Parse(typeof(AssetLocationFlag), locationFlag), locationId, (AssetLocationType) Enum.Parse(typeof(AssetLocationType), locationType), quantity, typeId);
    */
    }
}