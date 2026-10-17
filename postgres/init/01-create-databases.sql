-- Runs once, the first time the postgres volume is created (empty data directory).
-- One Postgres server, one database per service: no service reads another's tables.
CREATE DATABASE auth_db;
CREATE DATABASE ride_db;
CREATE DATABASE payment_db;
