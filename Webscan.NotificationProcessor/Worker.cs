using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Webscan.NotificationProcessor.Models;
using Webscan.NotificationProcessor.Models.Repository;
using Webscan.Notifier;

namespace Webscan.NotificationProcessor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IOptions<KafkaSettings> kafkaSettings)
        {
            _kafkaSettings = kafkaSettings ?? throw new ArgumentNullException($"{nameof(kafkaSettings)} cannot be null");
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)} cannot be null");
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException($"{nameof(serviceProvider)} cannot be null");

            TopicTestAndCreate(_kafkaSettings.Value.NotifierTopicName).Wait();
        }

        public async Task TopicTestAndCreate(string topicName)
        {
            // Ensure the topic has been created.
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _kafkaSettings.Value.Broker }).Build())
            {
                try
                {
                    Metadata topicsMetaData = adminClient.GetMetadata(topicName, new TimeSpan(0, 0, 30));
                    bool doesTopicExist = topicsMetaData.Topics.Any(t => t.Topic == topicName);
                    if (!doesTopicExist)
                    {
                        await adminClient.CreateTopicsAsync(new TopicSpecification[] {
                            new TopicSpecification { Name = topicName, ReplicationFactor = 1, NumPartitions = 1 } });
                    }
                }
                catch (CreateTopicsException e)
                {
                    _logger.LogInformation($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
                }
            }
        }




    public IEnumerable<User> GetStatusCheck(StatusCheck statusCheck)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                _logger.LogInformation($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}: Retreiving Collection of Users for {statusCheck.Name} From Database");
                IStatusCheckRepository<StatusCheck> statusCheckRepository = scope.ServiceProvider.GetRequiredService<IStatusCheckRepository<StatusCheck>>();
                return statusCheckRepository.Get(statusCheck.Id).Users;
            }
        }
        /// <summary>
        /// SendMessageNotificationAsync - sends notification email based on the status check
        /// </summary>
        /// <param name="statusCheck"></param>
        /// <returns></returns>
        public async Task SendMessageNotificationAsync(StatusCheck statusCheck)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                _logger.LogInformation($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}: Retreiving Collection of Users for {statusCheck.Name} From Database");
                IStatusCheckRepository<StatusCheck> statusCheckRepository = scope.ServiceProvider.GetRequiredService<IStatusCheckRepository<StatusCheck>>();

                StatusCheck statusCheckFromDb = statusCheckRepository.Get(statusCheck.Id);
                IEnumerable<User> usersToBeNotified = statusCheckRepository.GetUsers(statusCheck.Id);

                // Check to see when it was last notified and 
                if(DateTime.Now > statusCheckFromDb.LastNotified.AddMinutes(1))
                { 
                    INotifierService notifierService = scope.ServiceProvider.GetRequiredService<INotifierService>();

                    foreach (User user in usersToBeNotified)
                    {
                        _logger.LogInformation($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}: Sending Notification of {statusCheck.Name} to {user.Username} at {user.email}");
                        await notifierService.SendTextEmail("llN3M3515ll@gmail.com", user.email, $"", $"{statusCheckFromDb.Name} Is Now In Stock At: {statusCheck.BitlyShortenedUrl}");
                    }
                    statusCheckFromDb.LastNotified = DateTime.Now;
                    statusCheckRepository.Update(statusCheckFromDb);
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                _logger.LogInformation($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}: NotificationProcessingService shutting down");
            });

            _logger.LogInformation($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}: NotificationProcessingService Started ");

            try
            {
                ConsumerConfig consumerConfig = new ConsumerConfig
                {
                    BootstrapServers = _kafkaSettings.Value.Broker,
                    GroupId = _kafkaSettings.Value.SchedulerTopicGroupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };

                using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
                {
                    consumer.Subscribe(_kafkaSettings.Value.NotifierTopicName);

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var consumeResult = consumer.Consume(cancellationToken);

                        StatusCheck statusCheck = JsonConvert.DeserializeObject<StatusCheck>(consumeResult.Message.Value);
                        _logger.LogInformation($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}:\tReceived Kafka Message on {_kafkaSettings.Value.NotifierTopicName}");
                        _logger.LogInformation($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}:\t\t{statusCheck.Name}\n\t\t\t{statusCheck.Url}");

                        if (statusCheck.TimeScheduled.AddMinutes(2) < DateTime.Now)
                        {
                            _logger.LogInformation($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}:\t\t{statusCheck.Name} was scheduled at {statusCheck.TimeScheduled.ToString("MM/dd/yyyy hh:mm:ss.fff")} and is stale skipping!!!");
                            continue;
                        }

                        // handle consumed message.
                        // 1.) Query database and get the users that need to be notified
                        // 2.) Send Notification to each Entity
                        await SendMessageNotificationAsync(statusCheck);
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                //Swallow this since we expect this to occur when shutdown has been signaled.
                _logger.LogWarning(e, "A task/operation cancelled exception was caught.");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "An unhandled exception was thrown in the task processing background service.");
            }
            finally
            {
                _logger.LogCritical("The task processing background service is shutting down!");
                // TODO Send email to group to alert to an issue.
                //_hostApplicationLifetime.StopApplication(); //Should we shutdown the app or alert somehow?
            }
        }
    }
}
