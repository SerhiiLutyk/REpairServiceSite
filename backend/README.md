# NeoFix Backend (.NET)

ASP.NET Core Web API for [NeoFix Studio](../) — repair booking platform. Mirrors the Supabase schema from `supabase/migrations/` and exposes REST endpoints for the React frontend.

## Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core + SQLite (local dev)
- ASP.NET Core Identity + JWT Bearer
- OpenAPI document at `/openapi/v1.json` (development)

## Quick start

```bash
cd backend
dotnet restore
dotnet ef migrations add InitialCreate --project NeoFix.Api
dotnet run --project NeoFix.Api
```

API: `https://localhost:7xxx` (see launchSettings)  
OpenAPI: `GET /openapi/v1.json` in Development

### Default admin (seeded on first run)

| Field    | Value              |
|----------|--------------------|
| Email    | `admin@neofix.local` |
| Password | `Admin123!`        |

## API overview

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/register` | — | Sign up |
| POST | `/api/auth/login` | — | Sign in, returns JWT |
| GET | `/api/auth/me` | User | Current user + admin flag |
| GET | `/api/profiles/me` | User | Profile |
| PUT | `/api/profiles/me` | User | Update profile |
| GET | `/api/services` | — | List services (`?summary=true` for landing) |
| POST/PUT/DELETE | `/api/services` | Admin | Manage services |
| GET | `/api/bookings/slots?date=yyyy-MM-dd` | — | Taken time slots |
| GET | `/api/bookings/me` | User | My bookings |
| POST | `/api/bookings` | User | Create booking |
| PATCH | `/api/bookings/{id}/cancel` | User | Cancel own booking |
| GET | `/api/bookings` | Admin | All bookings + profiles |
| PATCH | `/api/bookings/{id}/status` | Admin | Update status |
| PATCH | `/api/bookings/{id}/notes` | Admin | Master notes |
| GET | `/api/reviews` | — | Public reviews |
| POST | `/api/reviews` | User | Add review |
| GET | `/api/admin/analytics` | Admin | Dashboard stats |

Send JWT as: `Authorization: Bearer <token>`

## Connect frontend

1. Run the API (`dotnet run --project NeoFix.Api`).
2. Set in frontend `.env`:
   ```
   VITE_API_URL=http://localhost:5000
   ```
3. Replace Supabase calls with `fetch` to `/api/*` (or add an API client layer).

CORS allows `http://localhost:5173` by default (Vite).

## Production

- Use PostgreSQL: change `UseSqlite` to `UseNpgsql` and set `ConnectionStrings:DefaultConnection`.
- Set strong `Jwt:Key` via environment variables or secrets.
- Disable default admin password or remove seed in production.
