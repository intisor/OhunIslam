using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventBus
{
    public abstract class Eventbus : IEventBus
    {
        private readonly ConcurrentDictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;

        protected Eventbus()
        {
            _handlers = new ConcurrentDictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
        }
        public abstract void Publish(IntegrationEvent @event);

        public virtual void Subscribe<T, TH>() 
            where T : IntegrationEvent 
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            _handlers.AddOrUpdate(eventName, new List<Type> { handlerType }, (key, value) =>
            {
                if (value.Any(s => s == handlerType))
                {
                    throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
                }
                value.Add(handlerType);
                return value;
            });

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
        }

        public void UnSubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (!_handlers.TryGetValue(eventName, out var handlers))
            {
                return;
            }

            handlers.Remove(handlerType);

            if (!handlers.Any())
            {
                _handlers.TryRemove(eventName, out _);
                _eventTypes.RemoveAll(e => e.Name == eventName);
            }

        }

        protected virtual async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.TryGetValue(eventName,out var subscriptions))
            {
                Console.WriteLine($"No handlers registered for event '{eventName}'.");
            }

            foreach (var subscription in subscriptions!)
            {
                var handler = Activator.CreateInstance(subscription) as IIntegrationEventHandler<IntegrationEvent>;
                if (handler == null) continue;

                var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);
                if (eventType == null)
                {
                    Console.WriteLine($"Event type '{eventName}' not found.");
                    continue;
                }

                var integrationEvent = JsonSerializer.Deserialize(message, eventType);

                try
                {
                    if (integrationEvent != null)
                    {
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        var method = concreteType.GetMethod("Handle");
                        if (method != null)
                        {
                            var task = (Task)method.Invoke(handler, new object[] { integrationEvent });
                            await task!.ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to deserialize event '{eventName}'.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing event '{eventName}': {ex.Message}");
                }
            }
        }
    }
}
