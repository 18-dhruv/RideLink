# Decisions

Why RideLink is built the way it is. Newest at the bottom. Each entry: the decision and the reason.

## 2026-10-15 to 2026-10-17: Foundation

- **.NET 10 + PostgreSQL, one service per folder, Docker Compose.**
  Learning project; Compose runs everything with one command.
- **One Postgres server, one database per service** (`auth_db`, `ride_db`).
  Each service owns its data; no service reads another's tables. One server keeps local setup light.
- **Auth issues JWTs; other services only validate them** with the shared `Jwt__Key`.
  Ride and Driver never call Auth to check a user.
- **Ride has separate rider and driver endpoints**, with ride ids in the URL (`/rides/{id}/cancel`, `/rides/{id}/accept`).
  Clear ownership checks per role.
- **Ride uses the Postgres `xmin` column as a concurrency token.**
  Two drivers accepting the same ride at once cannot both win.
- **Payment service removed from the plan.** Keeps scope small. (`payment_db` is still created by the init script and can be dropped.)
- **Gateway will be Ocelot, not YARP.**
- **Driver service stays** and will consume ride events.

## 2026-10-19 to 2026-10-21: Redis

- **Redis image `redis:7.4-alpine`.**
  Older images (Redis 5) lack `GETDEL` and `GEOSEARCH`.(we can also use PostGis here but to reduce the pressure on db we using redis geo which will help us to reduce db calls and also will be fast).
- **One `IConnectionMultiplexer` registered as a singleton.**
  Connecting is expensive and the multiplexer is thread-safe and reconnects itself (StackExchange.Redis docs).
- **Password reset tokens live in Redis, not Postgres.**
  Key `pw_reset:{hex sha256}` -> userId, with a TTL so Redis deletes it on expiry; read with `GETDEL` so it is single-use.
  Only the hash is stored, so reading Redis does not leak usable tokens.
- **forgot-password always answers success**, even for unknown emails, so it cannot be used to find registered emails.

## 2026-10-19: API shape

- **Every endpoint returns `ApiResponse<T>` `{ success, message, data }`**, and `GlobalExceptionHandler` returns the same shape with `success: false`.
  Clients parse one format.

## 2026-10-21: Messaging and next steps

- **Kafka for messaging (instead of RabbitMQ).**
  One broker for the whole project. KRaft mode, no Zookeeper. Client: `Confluent.Kafka`.
- **Topics per publishing service:** `auth-events`, `ride-events`. Message key = `rideId` for ride events, so one ride's events stay in order.
- **Commands stay HTTP, events go to Kafka.**
  A driver accepts a ride with `POST /rides/{id}/accept` on Ride (needs an immediate yes/no); Ride then publishes `RideAccepted`.
  Only Ride changes ride status.
- **Save first, then publish.** Accepting possible lost events for now; outbox pattern later.
- **Consumers must tolerate duplicates** (Kafka is at-least-once).
- **Order of work:** logging/tracing (OpenTelemetry + SigNoz), NUnit tests, Redis GEO in Driver, then Kafka.
- **SigNoz runs as its own compose project**, separate from RideLink's, because it is heavy (ClickHouse, ~4 GB RAM).


## **Flow**
- **rider** will first publishing the ride to **kafka** and the **driver service** will consume this and save it to **redis** then the drivers can call through simple http request and get rides withing in 5km. Then the driver will change the status using the the **ride service**.