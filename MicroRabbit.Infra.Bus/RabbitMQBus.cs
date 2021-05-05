using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Infra.Bus
{
    class RabbitMQBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;

        public RabbitMQBus(IMediator mediator)
        {
            this._mediator = mediator;
        }

        public Task SendCommand<T>(T Command) where T : Command
        {
            return this._mediator.Send(Command);
        }

        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var queueName = typeof(T).Name;
                    channel.QueueDeclare(queueName, false, false, false, null);
                    var message = JsonConvert.SerializeObject(@event);
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish("", queueName, null, body);
                }
            }
        }



        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            if (_handlers[eventName].Any(t => t.GetType().Name == handlerType.Name))
            {
                throw new ArgumentException($"Handler type {handlerType.Name} is already registered for {eventName}");
            }

            _handlers[eventName].Add(handlerType);

            StartBasicConsume<T>();

        }

        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory() { HostName = "localhost", DispatchConsumersAsync = true };
            var eventName = typeof(T).Name;
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(eventName, false, false, false, null);

                    var consumer = new AsyncEventingBasicConsumer(channel);

                    consumer.Received += Consumer_Received;
                    channel.BasicConsume("", eventName, null,);
                }
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            try
            {
                await ProcessEvent(e.RoutingKey, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {

            }

        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                var subscriptions = _handlers[eventName];
                foreach (var subscription in subscriptions)
                {
                    var handler = Activator.CreateInstance(subscription);
                    if (handler != null)
                    {
                        var eventType = _eventTypes.FirstOrDefault(t => t.Name == eventName);
                        var @event = JsonConvert.DeserializeObject(message, eventType);
                        var eventHandler = typeof(IEventHandler<>).MakeGenericType(eventType);
                        await (Task)eventHandler.GetMethod("Handle").Invoke(handler, new object[] { @event });
                    }
                }
            }
        }
    }
}
