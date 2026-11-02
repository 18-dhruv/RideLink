# RideLink

A ride-hailing backend built to learn microservices with .NET 10 and PostgreSQL.
Each service owns its own database and is built and run with Docker Compose.

## Services

| Service | Port (host) | Database | What it does |
|---|---|---|---|
| Auth   | 5218 | `auth_db` | Sign up, login (JWT), forgot/reset password (Redis tokens) |
| Ride   | 5219 | `ride_db` | Riders create, read and cancel rides; ride status lifecycle |
| Driver | 5220 | none yet  | Consumes `ride-events` from Kafka (BackgroundService). Next: Redis GEO for nearby rides |

Shared infrastructure in `docker-compose.yml`:

| Container | Host port | Purpose |
|---|---|---|
| postgres | 5432 | Postgres 17, one database per service |
| redis | 6379 | Redis 7.4 (reset tokens, later GEO) |
| kafka | 9092 | Kafka 3.9 in KRaft mode (broker + controller in one node) |
| kafka-init | none | Creates the `ride-events` topic (3 partitions), then exits |
| kafka-ui | 8090 | Browse topics and messages at http://localhost:8090 |
| otel-collector | 4317 / 4318 | Receives OpenTelemetry traces and logs (OTLP), prints them with the debug exporter |

## Running it

1. Copy `example.env` to `.env` and fill in `DB_PASSWORD` and `JWT_KEY`.
2. Everything in Docker: `docker compose up --build`
3. Or only infrastructure in Docker and a service from your machine:
   `docker compose up -d postgres redis kafka kafka-init otel-collector`, then `dotnet run` inside the service folder.

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
| POST | `/rides/create_ride` | Rider. Body: `{ pickup, dropoff, pickupCoordinate: { latitude, longitude }, dropoffCoordinate: { latitude, longitude } }` |
| GET | `/rides/{id}` | Rider or assigned driver |
| POST | `/rides/{id}/cancel` | Rider |
| POST | `/rides/{id}/accept` | Driver |
| POST | `/rides/{id}/start` | Driver |
| POST | `/rides/{id}/complete` | Driver |
| POST | `/rides/{id}/driver-cancel` | Driver |

Roles in `signin` are numbers: `0` = Driver, `1` = Rider.

Ride status: `Requested -> Accepted -> InProgress -> Completed`, or `Cancelled`.

## Events (Kafka)

| Topic | Partitions | Producer | Consumer (group) | Event |
|---|---|---|---|---|
| `ride-events` | 3 | Ride (after the ride is saved) | Driver (`driver-service`) | `RideRequested { rideId, riderId, pickup, dropoff, pickupCoordinate, dropoffCoordinate, requestedAt }` |

The message key is the ride id (int), so every event for one ride lands on the same partition in order.
The Driver consumer commits the offset after handling each message (at-least-once).
See `KAFKA_GUIDE.md` for the full explanation.

## Observability

Auth, Ride and Driver log with NLog (console) and send traces and logs over OTLP to `otel-collector`.
Read them with `docker compose logs -f otel-collector`. Logs and spans from one request share a Trace ID.

## How password reset works

1. `forgot-password` creates 32 random bytes. The hex string goes to the user.
2. Redis stores `pw_reset:{hex of SHA-256(token bytes)}` -> userId, with an expiry.
3. `reset-password` hashes the token the same way and runs `GETDEL`, so a token works once.

## Roadmap

Done: Auth, Ride, NLog, OpenTelemetry to a collector, Kafka `ride-events` (Ride produces, Driver consumes).

Next:
1. Driver: Redis GEO. Consumer runs `GEOADD rides:open`, plus `GET /drivers/rides/nearby` with `GEOSEARCH`
2. Ride publishes `RideAccepted` / `RideCancelled`; Driver removes the ride from `rides:open`
3. Outbox pattern so no event is lost if Kafka is down
4. SigNoz as the OTLP destination
5. Mail service over Kafka (`auth-events`)
6. Tests (NUnit), Gateway (Ocelot, then Nginx), CI

See `DECISIONS.md` for why things are the way they are.
