# DomainEventDispatcher

A lightweight domain event dispatcher for .NET applications based on Clean Architecture and Domain-Driven Design (DDD).  
Enables decoupled propagation of domain events via handlers registered in the dependency injection container.

---

## 📌 Features

- Supports dispatching multiple events in a single call.
- Dynamically resolves handlers using `IServiceProvider`.
- Asynchronous dispatching with cancellation support.
- Efficient design with type caching for performance optimization.
- Facilitates separation of concerns and maintainability.

---

## 🔌 Installation

Add the `DomainEventDispatcher.cs` file to your project.

---

## 🖥️ Usage

### 🎟 Define domain events

Implement the `IDomainEvent` interface in your domain events:

```csharp
public interface IDomainEvent { }

public class UserCreatedEvent : IDomainEvent {
  public string UserId { get; }
  public UserCreatedEvent(string userId) => UserId = userId;
}
```

### 📋 Create Handlers
Implement the `IDomainEventHandler<T>` to handle events:

```csharp
public class UserCreatedEventHandler : IDomainEventHandler<UserCreatedEvent> {
  public async Task HandleAsync(UserCreatedEvent domainEvent, CancellationToken cancellationToken) {
    // event handling logic
    Console.WriteLine($"User created with ID: {domainEvent.UserId}");
    await Task.CompletedTask;
  }
}
```

### Register in DI (example with Microsoft.Extensions.DependencyInjection)

```chsarp
services.AddScoped<IDomainEventHandler<UserCreatedEvent>, UserCreatedEventHandler>();
services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
```

### 📮 Dispatch events

```csharp
var dispatcher = serviceProvider.GetRequiredService<IDomainEventDispatcher>();

var events = new List<IDomainEvent> {
  new UserCreatedEvent("123")
};

await dispatcher.DispatchAsync(events);
```
