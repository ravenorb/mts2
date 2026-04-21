# MTS Razor Starter

A minimal ASP.NET Core Razor Pages starter for wrapping an existing MTS backend without rewriting the entire system.

## What this starter includes

- Razor Pages app shell with left navigation
- Dashboard page with station and queue cards
- SQLite-backed local app data for starter/demo metadata
- Typed `HttpClient` (`IMtsApiClient`) for calling your existing MTS API
- Stations page with a pallet move test form wired to the current-style `move-confirm` endpoint
- Placeholder sections for Engineering, Maintenance, and Planning

## Why this shape

This is the safe migration pattern:

1. Keep your current backend logic running.
2. Move the UI into Razor Pages.
3. Gradually move backend logic into C# services only after the workflows are stable.

## Prerequisites

- .NET 10 SDK
- Existing MTS API reachable from this app

If you must stay on .NET 8 for now, change the TargetFramework in `MTS.RazorStarter.csproj` from `net10.0` to `net8.0` and align package versions accordingly.

## Run

```bash
dotnet restore
dotnet run
```

## Configure your backend API

Edit `appsettings.json`:

```json
"MtsApi": {
  "BaseUrl": "http://127.0.0.1:8000/",
  "TimeoutSeconds": 30
}
```

Point `BaseUrl` at your current FastAPI/Flask/MTS backend.

## Current API assumptions

The starter expects:

- `GET /health`
- `POST /stations/{stationId}/pallet/{palletId}/move-confirm`

If your current endpoints differ, change only `MtsApiClient.cs` first.

## Suggested first migration steps

### Phase 1: UI wrapper
- Home dashboard
- Station dashboards
- Pallet move / queue forms
- Maintenance task pages

### Phase 2: Shared domain services
- Route resolution
- Queue validation
- Station state
- Maintenance scheduling

### Phase 3: Move backend logic selectively
- Pallet workflow rules
- BOM/component resolution
- Maintenance procedure authoring
- Auth/roles and approvals

## Recommended next pages

- `/Stations/Details/{id}`
- `/Planning/Queues`
- `/Engineering/FrameParts/{id}`
- `/Engineering/CutSheets/{id}`
- `/Maintenance/Procedures/{id}`

## Folder guide

- `Pages/` UI and page models
- `Services/` API wrappers and orchestration
- `Models/` view/domain models
- `Data/` EF Core context and seed data
- `wwwroot/` CSS and static files

## Notes

This starter uses `Database.EnsureCreated()` to stay simple. For real work, move to EF migrations.

Also: cookie auth is scaffolded very lightly here. For a real deployment, wire up ASP.NET Core Identity or your existing auth provider.
