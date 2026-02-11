# Railcar Trips

A Blazor WebAssembly application that processes railcar equipment events into trips, built with .NET 8.

## Solution Structure

```
RailcarTrips.slnx
├── src/RailcarTrips.Server/     ASP.NET Core Web API (startup project)
│   ├── Controllers/             API endpoints (Upload, Trips)
│   ├── Data/                    EF Core DbContext, entities, migrations
│   └── Services/                CSV parsing & trip processing logic
├── src/RailcarTrips.Client/     Blazor WebAssembly UI
│   ├── Pages/                   Razor component pages
│   ├── Services/                API client service
│   └── Layout/                  Navigation & layout
└── src/RailcarTrips.Shared/     Shared DTOs between Client & Server
    └── Models/                  TripDto, EventDto, etc.
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- (Optional) EF Core CLI tools for manual migration management: `dotnet tool install --global dotnet-ef`

### Database Setup

The SQLite database file (`railcartrips.db`) is not source controlled. It is created automatically on first run — EF Core migrations are applied at startup, which creates the database and seeds reference data (cities and event code definitions).

If you need to recreate the database from scratch, delete `railcartrips.db` from `src/RailcarTrips.Server/` and restart the app.

To manage migrations manually:

```bash
cd src/RailcarTrips.Server
dotnet ef database update
```

### Run

```bash
cd src/RailcarTrips.Server
dotnet run
```

Navigate to the URL shown in console output (e.g. `http://localhost:5104`).

### Usage

1. Navigate to **Railcar Trips** in the sidebar.
2. Upload a CSV file with the following columns:
   ```
   Equipment Id,Event Code,Event Time,City Id
   ACAR1234,W,2025-01-05 08:00,1
   ```

   - **Event Codes:** W (Released), A (Arrived), D (Departed), Z (Placed)
   - **City Id:** References a seeded Canadian city (1–49)
3. Click **Process Events** to parse events into trips.
4. View processed trips in the data grid.
5. Click any trip row (or the **Details** button) to see its events in chronological order.

---

## Questions & Assumptions

### Questions I Would Ask

1. **Orphaned events:** What should happen with A/D events that occur before the first W event for an equipment, or Z events that arrive without an open trip? Currently these are counted and discarded.

2. **Consecutive W events:** If a second W (Released) event occurs before the previous trip is closed with a Z (Placed), should the first trip be discarded, closed as incomplete, or should this be treated as an error? Current implementation closes the previous trip as incomplete and starts a new one.

3. **Incomplete trips:** If the event data ends with an open trip (W started but no Z), should the trip still be stored? Current implementation saves it as incomplete using the last event's city/time as the destination.

4. **Re-upload behavior:** Should uploading a new file replace all previously processed data, or should it be additive/incremental? Current implementation replaces all existing trips and events.

5. **Time zone ambiguity:** During DST transitions, a local time can be ambiguous (e.g., 1:30 AM occurs twice during fall-back). Should we assume standard time or daylight time? .NET's `TimeZoneInfo.ConvertTimeToUtc` throws on ambiguous times by default — production code should handle this explicitly.

6. **Data validation:** Should the system reject an entire file if any rows are invalid, or process valid rows and report errors? Current implementation skips rows with unknown city IDs and logs warnings.

### Assumptions Made

1. **Trip boundaries are W→Z:** A trip starts with event code W (Released) and ends with event code Z (Placed). Events A (Arrived) and D (Departed) are intermediate events within a trip.

2. **Non-overlapping trips per equipment:** Each piece of equipment has at most one active trip at a time.

3. **Chronological ordering uses UTC:** Events are sorted by their UTC-converted time (not raw local time) since events span multiple time zones. This is critical for correct ordering.

4. **DST is handled by `TimeZoneInfo`:** The Windows time zone IDs in the cities CSV (e.g., "Eastern Standard Time") include DST rules. `Canada Central Standard Time` (Saskatchewan) correctly has no DST.

5. **SQLite for simplicity:** SQLite was chosen as the database provider for zero-configuration portability. In production, this would be swapped for SQL Server or PostgreSQL via configuration.

6. **Reference data is seeded via EF Core `HasData`:** The cities and event code definitions CSVs are scripted into the database as seed data in the migration, as specified in the requirements.

7. **Full file replacement on upload:** Each upload replaces all existing trip/event data. A production system would likely support incremental uploads.

---

## Design Considerations & TODOs

### Implemented

- ✅ CSV file upload and parsing with CsvHelper
- ✅ Local-to-UTC time conversion using city time zones (handles DST)
- ✅ Trip processing: W starts trip, A/D are intermediate, Z closes trip
- ✅ EF Core with SQLite, auto-migrations on startup
- ✅ Reference data seeded from canadian_cities.csv and event_code_definitions.csv
- ✅ Trips grid showing all processed trips
- ✅ Trip detail view showing events in chronological order (bonus feature)
- ✅ Proper handling of orphaned/incomplete trips with logging

### TODOs (documented in code)

- Pagination, filtering, and sorting on the trips grid for scalability
- File validation (size limits, content type, header validation)
- Error toasts/notifications on the client side
- Trip status field to distinguish complete vs. incomplete trips
- Incremental upload support (per-equipment replacement instead of full wipe)
- Production migration strategy (remove auto-migrate from startup)
- Unit tests for `TripProcessingService` (especially edge cases around W/Z ordering)
- Integration tests for API endpoints
- Loading the event descriptions from the `EventCodeDefinitions` DB table instead of hardcoding in the controller

## Technology Stack

| Component     | Technology                    |
| ------------- | ----------------------------- |
| Frontend      | Blazor WebAssembly (.NET 8)   |
| Backend       | ASP.NET Core Web API (.NET 8) |
| Database      | SQLite via EF Core 8          |
| CSV Parsing   | CsvHelper                     |
| Shared Models | .NET Class Library            |
