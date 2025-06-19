namespace DomainEventDispatcher;

public interface IDomainEventDispatcher {
  Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}