# RideLink

A ride-hailing backend built to learn microservices with .NET 10 and PostgreSQL.
Each service owns its own database and is built and run with Docker Compose.

## Services

| Service | Port (host) | Database | What it does |
|---|---|---|---|
| Auth   | 5218 | `auth_db` | Sign up, login (JWT), forgot/reset password (Redis tokens) |
| Ride   | 5219 | `ride_db` | Riders create, read and cancel rides; ride status lifecycle |
| Driver | 5220 | none yet  | Skeleton. Will hold driver location (Redis GEO) and consume ride events |

Shared infrastructure in `docker-compose.yml`: Postgres 17 (one server, one database per service) and Redis 7.4.

## Running it

1. Copy `example.env` to `.env` and fill in `DB_PASSWORD` and `JWT_KEY`.
2. Everything in Docker: `docker compose up --build`
3. Or only infrastructure in Docker and a service from your machine:
   `docker compose up -d postgres redis`, then `dotnet run` inside the service folder.

Connection rule: an app running on your Mac uses `localhost` for Postgres and Redis.
An app running in Docker uses the service names `postgres` and `redis` (set in compose `environment:`).
`appsettings.json` holds the local values; compose environment variables override them in Docker.

## API

All endpoints return the same shape, errors included:

```json
{ "success": true, "message": "...", "data": { } }
```

### Auth (`/auth`)
| Method | Path | Body |
|---|---|---|
| POST | `/auth/signin` | `{ name, email, password, role }` |
| POST | `/auth/login` | `{ email, password }` |
| POST | `/auth/forgot-password` | `{ email }` returns a reset token (later sent by mail) |
| POST | `/auth/reset-password` | `{ token, newPassword }` |

### Ride (`/rides`, JWT required)
| Method | Path | Who |
|---|---|---|
| POST | `/rides` | Rider |
| GET | `/rides/{id}` | Rider or assigned driver |
| POST | `/rides/{id}/cancel` | Rider |

Ride status: `Requested -> Accepted -> InProgress -> Completed`, or `Cancelled`.

## How password reset works

1. `forgot-password` creates 32 random bytes. The hex string goes to the user.
2. Redis stores `pw_reset:{hex of SHA-256(token bytes)}` -> userId, with an expiry.
3. `reset-password` hashes the token the same way and runs `GETDEL`, so a token works once.

## Roadmap

1. Logging and tracing: OpenTelemetry -> SigNoz
2. Tests: NUnit
3. Driver: location with Redis GEO (`GEOADD`, `GEOSEARCH`)
4. Kafka: Auth -> Mail (`auth-events`), Ride -> Driver/Mail (`ride-events`)
5. Gateway: Ocelot, then Nginx
6. CI

See `DECISIONS.md` for why things are the way they are.
