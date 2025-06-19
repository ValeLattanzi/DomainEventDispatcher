using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DomainEventDispatcher;

public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher {
  private static readonly ConcurrentDictionary<Type, Type> HandleTypeDict = new();
  private static readonly ConcurrentDictionary<Type, Type> WrapperTypeDict = new();
  
  public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) {
    foreach (var domainEvent in domainEvents) {
      using IServiceScope scope = serviceProvider.CreateScope();
      var domainEventType = domainEvent.GetType();
      var handlerType = HandleTypeDict.GetOrAdd(
        domainEventType,
        eventType => typeof(IDomainEventHandler<>).MakeGenericType(eventType)
        );

      var handlers  = scope.ServiceProvider.GetServices(handlerType);
      foreach (var handler in handlers) {
        if (handler is null) continue;

        var handlerWrapper = HandlerWrapper.Create(handler, domainEventType);
        await handlerWrapper.Handle(domainEvent, cancellationToken);
      }
    }
  }

  private abstract class HandlerWrapper {
    public abstract Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken);

    public static HandlerWrapper Create(object handler, Type domainEventType) {
      Type wrapperType = WrapperTypeDict.GetOrAdd(domainEventType,
        eventType => typeof(HandlerWrapper<>).MakeGenericType(eventType));
      return (HandlerWrapper)Activator.CreateInstance(wrapperType, handler);
    }
  }

  private sealed class HandlerWrapper<T>(object handler) : HandlerWrapper where T : IDomainEvent {
    private readonly IDomainEventHandler<T> _handler = (IDomainEventHandler<T>)handler;

    public override async Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken) {
      await _handler.HandleAsync((T)domainEvent, cancellationToken);
    }
  }
}