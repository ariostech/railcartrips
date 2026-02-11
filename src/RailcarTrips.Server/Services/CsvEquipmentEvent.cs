using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace RailcarTrips.Server.Services;

/// <summary>
/// Represents a raw row from the equipment_events.csv file.
/// </summary>
public class CsvEquipmentEvent
{
    public string EquipmentId { get; set; } = string.Empty;
    public string EventCode { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
    public int CityId { get; set; }
}

/// <summary>
/// CsvHelper class map for the equipment events CSV format.
/// </summary>
public sealed class CsvEquipmentEventMap : ClassMap<CsvEquipmentEvent>
{
    public CsvEquipmentEventMap()
    {
        Map(m => m.EquipmentId).Name("Equipment Id");
        Map(m => m.EventCode).Name("Event Code");
        Map(m => m.EventTime).Name("Event Time");
        Map(m => m.CityId).Name("City Id");
    }
}
