# Kafka from start to end

A general guide to Apache Kafka, written for learning. It follows the official Apache Kafka documentation (version 3.9 and later). Each section links the docs page it's based on.

## 1. The problem Kafka solves

When many systems need to know about the same events, calling each other directly causes problems:

- If one system is slow or down, the system calling it slows down or fails too.
- Every sender must know about every receiver, and must change whenever a new receiver is added.
- A traffic spike hits every system at once.
- If a call fails, the information is lost.

Kafka sits in the middle. A sender writes an event once and moves on. Each receiver reads it when it is ready. The sender and receivers no longer know about each other, which is called **decoupling**.

Kafka calls itself an **event streaming platform**. It does three things:

1. **Publish and subscribe:** programs write streams of events, and other programs read them.
2. **Store:** events are kept on disk, safely and for as long as you choose.
3. **Process:** events can be transformed as they arrive (Kafka Streams).

Docs: [Introduction](https://kafka.apache.org/intro)

## 2. Events

An **event** (also called a record or message) is a fact that something happened. It has:

- a **key** (optional), such as an order id
- a **value**, the data itself, often JSON
- a **timestamp**
- optional **headers**, small extra fields

Events are never changed after they're written. You add new events instead.

Docs: [Main concepts and terminology](https://kafka.apache.org/documentation/#intro_concepts_and_terms)

## 3. Topics

A **topic** is a named stream of events, such as `orders` or `payments`. You can think of it as a folder, and events as the files in it.

- Many producers can write to one topic.
- Many consumers can read from one topic.
- Events are **not deleted when they're read**. They are kept according to the topic's retention settings (section 9), so they can be read again.

## 4. Partitions and offsets

Each topic is split into **partitions**. A partition is an ordered list of events that only grows: new events are always added at the end. This is called a **log**.

- Every event in a partition gets a number called its **offset**: 0, 1, 2, and so on.
- Order is guaranteed **inside one partition**, not across the whole topic.
- Partitions are spread over several servers so they can be read and written in parallel.

**Which partition does an event go to?**

- **With a key:** Kafka hashes the key, so the same key always goes to the same partition. All events for one order stay in order.
- **Without a key:** events are spread across partitions for balance.

**How many partitions:** you choose when creating the topic. You can increase the number later but never decrease it. After an increase, a key may map to a different partition, which can break ordering for that key during the change. The partition count is also the most consumers in one group that can work at once (section 8).

Docs: [Topics and logs](https://kafka.apache.org/documentation/#intro_concepts_and_terms), [Adding a topic](https://kafka.apache.org/documentation/#basic_ops_add_topic)

## 5. Brokers, clusters, and controllers

- A **broker** is a Kafka server. It stores partitions on disk and answers producers and consumers.
- A **cluster** is a group of brokers working together.
- A **controller** manages the cluster's metadata: which topics exist, which broker leads each partition, which brokers are alive, configs and permissions. It never handles your messages.

**KRaft vs ZooKeeper.** Older Kafka needed a separate program, ZooKeeper, to store this metadata. Since Kafka 3.3, Kafka can do it itself in **KRaft mode**: a few nodes act as controllers and use the Raft voting algorithm to elect one **active controller**. That controller writes every decision to an internal log (`__cluster_metadata`) that the others copy. Kafka 4.0 removed ZooKeeper entirely.

Each node gets roles through `process.roles`:

- `broker`: stores messages only
- `controller`: manages the cluster only
- `broker,controller`: both (fine for local or small setups)

Controllers usually come in odd numbers (3 or 5) so a majority can always be reached. With 3 controllers the cluster survives losing 1, and with 5 it survives losing 2.

Docs: [KRaft](https://kafka.apache.org/documentation/#kraft)

## 6. Replication: how data survives a crash

Each partition is copied to several brokers. The number of copies is the **replication factor** (often 3).

- One copy is the **leader**. All reads and writes go through it.
- The others are **followers**. They copy the leader.
- The copies that are fully caught up form the **in-sync replicas (ISR)**.
- If the leader's broker dies, the controller promotes a follower from the ISR to be the new leader.

`min.insync.replicas` sets how many copies must have an event before a write counts as successful when the producer uses `acks=all` (section 7). A common production setup is replication factor 3 with `min.insync.replicas=2`.

Docs: [Replication](https://kafka.apache.org/documentation/#replication)

## 7. Producers

A **producer** writes events to a topic.

**What happens when you send:**

1. The producer turns the key and value into bytes (**serialization**).
2. It picks the partition (from the key, or spread out if there's no key).
3. It groups events into **batches** for speed (`linger.ms`, `batch.size`).
4. It sends the batch to that partition's leader broker.
5. The broker answers with an acknowledgement and the event's offset.

**`acks`, how sure you want to be:**

- `acks=0`: don't wait for an answer. Fastest, but events can be lost.
- `acks=1`: wait until the leader has it. Lost if the leader dies before followers copy it.
- `acks=all`: wait until all in-sync replicas have it. Safest, and the default since Kafka 3.0.

**Retries and idempotence.** If a send fails, the producer retries. A retry could write the same event twice, so **idempotent producers** (`enable.idempotence=true`, the default since 3.0) give each event a sequence number, and the broker drops duplicates.

**Transactions.** A producer can write to several partitions all-or-nothing. This is used for exactly-once processing (section 10).

Docs: [Producer API](https://kafka.apache.org/documentation/#producerapi), [Producer configs](https://kafka.apache.org/documentation/#producerconfigs)

## 8. Consumers and consumer groups

A **consumer** reads events from a topic. It **pulls**: it asks the broker for new events in a loop (`poll`). Kafka never pushes.

**Consumer groups.** Consumers with the same `group.id` form a group that shares the work:

- Each partition is read by **exactly one** consumer in the group.
- Topic with 3 partitions and 3 consumers: one partition each.
- 2 consumers: one of them reads two partitions.
- 4 consumers: one sits idle.
- **Different groups each get every event.** A billing group and an email group both see all orders.

**Group coordinator and rebalancing.** One broker acts as the group's **coordinator**. When a consumer joins, leaves or crashes (it stops sending heartbeats), the coordinator **rebalances**: it hands partitions out again. Kafka 4.0 adds a newer, faster rebalance protocol.

**Offsets and commits.** Each group remembers how far it has read in each partition by **committing** offsets to an internal topic, `__consumer_offsets`. After a restart, it continues from the last committed offset.

- **Auto commit** (`enable.auto.commit=true`): commits every few seconds by itself. Simple, but it may commit before your code has finished handling an event.
- **Manual commit:** you commit after handling the event. Safer.

**Where a new group starts:** `auto.offset.reset=earliest` reads from the beginning, and `latest` reads only new events.

Docs: [Consumer API](https://kafka.apache.org/documentation/#consumerapi), [Consumer configs](https://kafka.apache.org/documentation/#consumerconfigs), [Consumer position](https://kafka.apache.org/documentation/#design_consumerposition)

## 9. Retention and compaction

Kafka keeps events after they're read. Each topic sets how long:

- **Delete policy (default):** remove old events after a time (`retention.ms`, 7 days by default) or after a size (`retention.bytes`).
- **Compact policy** (`cleanup.policy=compact`): keep only the **latest event for each key**. Useful for "current state" topics, such as each user's latest address. Sending a key with an empty value (a **tombstone**) deletes that key.

Docs: [Log compaction](https://kafka.apache.org/documentation/#compaction), [Topic configs](https://kafka.apache.org/documentation/#topicconfigs)

## 10. Delivery guarantees

Kafka can give you one of three promises:

- **At most once:** an event may be lost but is never handled twice. You commit the offset before handling the event.
- **At least once:** an event is never lost but may be handled twice. You handle it, then commit. This is the most common choice. Make your handlers **idempotent**, meaning that handling the same event twice has the same result as once, for example by checking an id.
- **Exactly once:** idempotent producers plus transactions plus `isolation.level=read_committed` on consumers. This works fully only when you read from Kafka and write back to Kafka.

Docs: [Message delivery semantics](https://kafka.apache.org/documentation/#semantics)

## 11. Listeners: how clients connect

A client first connects to any broker (the **bootstrap server**) and asks for the cluster's metadata. The broker replies with the address of each broker as set in **`advertised.listeners`**, and the client then connects to those addresses.

- `listeners`: the ports the broker actually opens.
- `advertised.listeners`: the addresses it tells clients to use.

In Docker this matters: a program on your computer reaches `localhost:9092`, while another container reaches `kafka:29092`. So you open one listener for each and advertise both.

Docs: [Broker configs](https://kafka.apache.org/documentation/#brokerconfigs_listeners), Confluent's [Kafka Listeners – Explained](https://www.confluent.io/blog/kafka-listeners-explained/)

## 12. The rest of the ecosystem

- **Kafka Connect:** ready-made connectors that move data between Kafka and databases, files or cloud services without writing code. [Docs](https://kafka.apache.org/documentation/#connect)
- **Kafka Streams:** a Java library for processing events as they arrive, such as filtering, joining and counting. [Docs](https://kafka.apache.org/documentation/streams/)
- **Schema Registry** (from Confluent, not part of Apache Kafka): stores the format of your events (Avro, JSON Schema, Protobuf) so producers and consumers agree on it.
- **Admin API:** create topics and change settings from code. [Docs](https://kafka.apache.org/documentation/#adminapi)

## 13. The whole flow, start to end

1. Start a cluster: brokers plus controllers (one node doing both is fine for learning).
2. Create a topic and choose its partitions, replication factor and retention.
3. A producer serializes an event, picks a partition from the key, batches it and sends it to the leader.
4. The leader writes the event to the end of the partition's log and gives it an offset. Followers copy it. Once the in-sync replicas have it, the producer gets its acknowledgement.
5. Consumers in a group poll their assigned partitions, handle the events and commit their offsets.
6. If a consumer dies, the group rebalances and another consumer continues from the committed offset.
7. If a broker dies, the controller promotes a new leader from the ISR, and clients switch to it on their own.
8. Old events are removed by retention, or compacted down to the latest value for each key.

## 14. Try it yourself (command-line tools)

These ship with Kafka in `bin/`. In the `apache/kafka` image they're in `/opt/kafka/bin/`.

```
# create a topic
kafka-topics.sh --bootstrap-server localhost:9092 --create --topic demo --partitions 3 --replication-factor 1

# describe it
kafka-topics.sh --bootstrap-server localhost:9092 --describe --topic demo

# write events (type lines, Ctrl+C to stop); key:value format
kafka-console-producer.sh --bootstrap-server localhost:9092 --topic demo --property parse.key=true --property key.separator=:

# read events from the beginning as group g1
kafka-console-consumer.sh --bootstrap-server localhost:9092 --topic demo --group g1 --from-beginning --property print.key=true --property print.partition=true

# see each group's offsets and lag
kafka-consumer-groups.sh --bootstrap-server localhost:9092 --describe --group g1
```

Things to try: run two consumers in group `g1` and watch the partitions split between them. Then start one in group `g2` and see it get every event.

Docs: [Quickstart](https://kafka.apache.org/quickstart)

## 15. In .NET

The official client is the NuGet package **Confluent.Kafka**:

- `ProducerBuilder<TKey, TValue>` builds an `IProducer`. Create one and share it (register as a singleton). Send with `ProduceAsync(topic, new Message { Key, Value })`.
- `ConsumerBuilder<TKey, TValue>` builds an `IConsumer`. Call `Subscribe(topic)`, then call `Consume(cancellationToken)` in a loop. In ASP.NET Core this loop usually lives in a `BackgroundService`.
- `AdminClientBuilder` builds an `IAdminClient` for creating topics from code.

Docs: [Confluent .NET client](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html), [Hosted services in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services)
