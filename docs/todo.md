# Backend Refactoring Plan: Modular Architecture, NATS v2 Event-Driven Messaging, & 3-Phase Application Lifecycle

This document outlines the step-by-step tasks required to refactor the current codebase based on the specifications in [pattern_module.md](file:///D:/c/training-dotnet/Medium.Api/docs/pattern_module.md) and [pattern_lifecycle.md](file:///D:/c/training-dotnet/Medium.Api/docs/pattern_lifecycle.md).

The refactoring is structured into 5 Sprints to ensure a logical progression from core infrastructure lifecycle setup to event-driven messaging integration and domain CQRS migration.

---

## 📅 Sprint 1: Asynchronous 3-Phase Lifecycle Infrastructure Setup
*Objective: Introduce the 3-phase application lifecycle pattern (Startup, Running, Shutdown) to manage connections asynchronously on startup rather than synchronously during DI registration.*

- [ ] **Define Lifecycle Interfaces & Infrastructure Contracts**
  - Define `IInfrastructureLifecycle` or separate connection lifecycle interfaces (`IDbConnectionLifecycle`, `ICacheLifecycle`) containing `ConnectAsync` and `DisconnectAsync` to manage connection status.
- [ ] **Refactor Redis Connection Registration**
  - Refactor [CacheModule.cs](file:///D:/c/training-dotnet/Medium.Api/Infrastructure/Cache/Module/CacheModule.cs) to remove the synchronous `ConnectionMultiplexer.Connect(options)` call from DI service registration.
  - Implement a deferred/lazy or asynchronous factory registration for Redis, resolving connections asynchronously during the startup phase.
- [ ] **Refactor Database Lifecycle Check**
  - Create a database connection lifecycle checker utilizing EF Core's `Database.CanConnectAsync()` to verify database health and warm up EF Core connections on startup.
- [ ] **Create the Application Module**
  - Create `ApplicationModule` (implementing `IAsyncDisposable`) to coordinate cleanup and connection termination of all active lifecycle dependencies when disposed.
- [ ] **Create the Application Lifecycle Service**
  - Create `ApplicationLifecycleService` (implementing `IHostedService` and `IAsyncDisposable`) to manage the 3 lifecycle phases:
    - **Fase 1 (Startup)**: Create scope, resolve and initialize DB, Cache (Redis), and NATS connections asynchronously, then instantiate `ApplicationModule`.
    - **Fase 2 (Running)**: Keep the application running and handle requests.
    - **Fase 3 (Shutdown)**: Dispose `ApplicationModule` and cleanly close all connections.
- [ ] **Update Program.cs**
  - Register `ApplicationLifecycleService` as a hosted service and `ApplicationModule` as a scoped/singleton service in [Program.cs](file:///D:/c/training-dotnet/Medium.Api/Program.cs).

---

## 📅 Sprint 2: NATS V2 Library Migration & Publishing Framework
*Objective: Migrate NATS package dependencies and implement standard and JetStream publishers using the modern `NATS.Client.Core` and `NATS.Client.JetStream` libraries.*

- [ ] **Upgrade NuGet Dependencies**
  - Modify [Medium.Api.csproj](file:///D:/c/training-dotnet/Medium.Api/Medium.Api.csproj) to remove the legacy `NATS.Client` (v1) reference.
  - Ensure references to `NATS.Client.Core` and `NATS.Client.JetStream` are locked to version 2.0.0+.
- [ ] **Define Core Event Models & Contracts**
  - Define base `DomainEvent` class containing `Id` and `Timestamp` (UTC).
  - Define concrete events: `UserRegisteredEvent` and `UserLoggedInEvent`.
  - Define Request-Reply models: `SendWelcomeEmailRequest` and `SendWelcomeEmailResponse`.
- [ ] **Implement NATS v2 Event Publisher**
  - Implement `INatsEventPublisher` and `NatsEventPublisher` using the new `INatsConnection` to support basic publishing and request-reply (`RequestAsync`).
- [ ] **Implement JetStream Event Publisher**
  - Implement `IJetStreamEventPublisher` and `JetStreamEventPublisher` using `NatsJSContext` to publish events to stream-backed subjects (`PublishToStreamAsync`).
- [ ] **Create JetStream Initializer**
  - Create `JetStreamInitializer` with `InitializeJetStreamAsync(...)` to provision the `USER_EVENTS` stream (subjects `user.>`, storage type, retention, and discard policies) asynchronously.
  - Register it to run during Fase 1 (Startup) of `ApplicationLifecycleService`.

---

## 📅 Sprint 3: NATS Event Consumers & Responders Integration
*Objective: Build push/pull background consumers and request-reply responders based on the new NATS v2 API.*

- [ ] **Implement Push Consumer (User Registered)**
  - Implement `UserRegisteredPushConsumer` inheriting from `BackgroundService` subscribing to JetStream's `user.registered.push` using `NatsJSContext.SubscribeAsync`.
  - Acknowledge (`AckAsync`) or negatively acknowledge (`NakAsync`) messages correctly.
- [ ] **Implement Pull Consumer (User Logged In)**
  - Implement `UserLoggedInPullConsumer` inheriting from `BackgroundService` to fetch messages periodically from the `user-logged-in-pull` durable consumer using `NatsJSContext.FetchAsync`.
- [ ] **Implement Request-Reply Responder (Email Responder)**
  - Implement `EmailServiceResponder` inheriting from `BackgroundService` subscribing to the `email.send-welcome` subject.
  - Process requests, simulate email sending, and reply using `args.Message.ReplyAsync`.
- [ ] **Update NATS DI Registrations**
  - Update [NatsModule.cs](file:///D:/c/training-dotnet/Medium.Api/Infrastructure/Nats/Module/NatsModule.cs) to register these hosted background services (`AddHostedService`).

---

## 📅 Sprint 4: CQRS Migration & Core Domain Integration (MediatR)
*Objective: Replace the custom CQRS bus with MediatR across domains, and modify command handlers to publish NATS events.*

- [ ] **Configure MediatR and Validation Pipelines**
  - Register MediatR in [DependencyInjection.cs](file:///D:/c/training-dotnet/Medium.Api/Infrastructure/DependencyInjection.cs).
  - Setup FluentValidation pipeline behavior for automatically validating MediatR requests.
- [ ] **Migrate Auth Commands & Queries to MediatR**
  - Refactor `RegisterCommand`, `LoginCommand`, and `GetUserByIdQuery` to implement `IRequest<T>`.
  - Refactor command handlers [RegisterCommandHandler](file:///D:/c/training-dotnet/Medium.Api/Domain/Auth/Commands/RegisterCommandHandler.cs), [LoginCommandHandler](file:///D:/c/training-dotnet/Medium.Api/Domain/Auth/Commands/LoginCommandHandler.cs), and `GetUserByIdQueryHandler` to implement MediatR's `IRequestHandler`.
- [ ] **Integrate JetStream & Request-Reply in Auth Handlers**
  - In [RegisterCommandHandler](file:///D:/c/training-dotnet/Medium.Api/Domain/Auth/Commands/RegisterCommandHandler.cs): Publish `UserRegisteredEvent` via `IJetStreamEventPublisher`, and trigger welcome email request using `INatsEventPublisher.RequestAsync` to the responder service.
  - In [LoginCommandHandler](file:///D:/c/training-dotnet/Medium.Api/Domain/Auth/Commands/LoginCommandHandler.cs): Publish `UserLoggedInEvent` to the JetStream stream.
- [ ] **Migrate User Domain to MediatR**
  - Refactor User commands/queries and update [UserModule.cs](file:///D:/c/training-dotnet/Medium.Api/Domain/User/Module/UserModule.cs).
- [ ] **Migrate Other Domains to MediatR**
  - Refactor other domains currently utilizing the legacy custom CQRS handlers (e.g. Storage, Article, Comment, Bookmark) to ensure consistent codebase CQRS implementation.

---

## 📅 Sprint 5: Testing, Validation, and Cleanup
*Objective: Verify the refactored architecture under full operational scenarios and clean up legacy code.*

- [ ] **Verify Connection Lifecycle Startup Phase**
  - Ensure that DB, Redis, and NATS connections are established asynchronously during Fase 1 (Startup) and do not cause blocking during service registration.
  - Verify that the DB Seeding logic operates cleanly on startup.
- [ ] **End-to-End Integration Testing**
  - Verify user registration and login flows: ensure JetStream stream is created, events are published, consumers receive them, and the email responder responds correctly.
- [ ] **Verify Graceful Shutdown Phase**
  - Verify that during Fase 3 (Shutdown), all active connections are cleanly terminated via `ApplicationModule.DisposeAsync()`.
- [ ] **Clean Up Legacy Code**
  - Delete legacy custom CQRS components (e.g., `Infrastructure/CQRS` directory).
  - Remove legacy NATS classes (`NatsPublisher` and `NatsSubscriber` based on v1) to prevent compilation conflicts.
