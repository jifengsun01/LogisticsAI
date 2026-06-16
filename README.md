# AI Logistics Assistant MVP

Full-stack logistics management with AI-powered shipment insights via Gemini.

## Stack

- **API**: ASP.NET Core 8, Entity Framework Core 8, PostgreSQL (Npgsql)
- **UI**: React 18 + TypeScript, Vite, Recharts, Axios

## Prerequisites

- .NET 8 SDK
- Node.js 20+
- PostgreSQL 15+
- (Optional) Gemini API key for AI insights

## Setup

### 1. Database

Create the database:

```sql
CREATE DATABASE logistics_ai;
```

### 2. Configure the API

Copy `.env.example` values into `LogisticsAI.Api/appsettings.Development.json`:

```json
{
  "DATABASE_URL": "Host=localhost;Database=logistics_ai;Username=postgres;Password=yourpassword",
  "GEMINI_API_KEY": "your_key_here"
}
```

### 3. Run EF Core migrations

```bash
cd LogisticsAI.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Start the API

```bash
cd LogisticsAI.Api
dotnet run
```

API runs at `http://localhost:5000`. Swagger UI at `http://localhost:5000/swagger`.

### 5. Start the UI

```bash
cd logistics-ui
npm run dev
```

UI runs at `http://localhost:5173`.

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/shipments` | List all shipments |
| GET | `/api/shipments/{id}` | Get shipment by ID |
| POST | `/api/shipments` | Create shipment (triggers AI insight) |
| PATCH | `/api/shipments/{id}/status` | Update shipment status |
| DELETE | `/api/shipments/{id}` | Delete shipment |

## Project Structure

```
LogisticsAI/
├── LogisticsAI.Api/
│   ├── Controllers/   # ShipmentsController
│   ├── Services/      # ShipmentService, GroqAgentService, GroqInsightService
│   ├── Models/        # Shipment, Route, ShipmentEvent, RcaResult, ChatSession, ChatMessage
│   ├── Data/          # AppDbContext, SeedData
│   └── Program.cs
├── logistics-ui/
│   └── src/
│       ├── api/       # axios client + shipments API
│       ├── components/ # ShipmentCard, StatusChart
│       └── pages/     # Dashboard
├── .env.example
└── README.md
```
