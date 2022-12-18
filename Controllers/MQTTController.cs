using System.Text;
using ATARK_Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using MQTTnet.Server;
using MqttClient = MQTTnet.Client.MqttClient;

namespace ATARK_Backend.Controllers
{
    public class MqttController : IMqttController
    {
        private BackendContext _context;
        public MqttController(BackendContext context)
        {
            _context = context;
        }
        private static IMqttClient? _mqttClient;
        public async Task PublishMethodAsync(string topic, string messagePayload)
        {
            var mqttFactory = new MqttFactory();
            //client.ConnectAsync(mqttClientOptions);
            var message = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(messagePayload).Build();
            await _mqttClient.PublishAsync(message, CancellationToken.None);
            Console.WriteLine("MQTT application message is published.");
        }

        public async Task Handle_Received_Application_Message()
        {
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient();
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId("MQTTnetApp")
                .WithTcpServer("96a74ec03f044c93a2d5083f02fecba5.s2.eu.hivemq.cloud", 8883)
                .WithCredentials("backed", "8yv5ezn8yv5ezn")
                .WithTls()
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("Update");
                if (e.ApplicationMessage.Topic == "store/getData")
                {
                    var payloadText = Encoding.UTF8.GetString(
                        e?.ApplicationMessage?.Payload ?? Array.Empty<byte>());
                    string[] deviceData = payloadText.Split('/');
                    if (deviceData[0] == "store" && int.TryParse(deviceData[1], out var storeId))
                    {
                        return PublishMethodAsync("store/" + storeId, _context.GetMessage(storeId));
                    }
                }

                return Task.CompletedTask;
            };

            _mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    var payloadText = Encoding.UTF8.GetString(
                        e?.ApplicationMessage?.Payload ?? Array.Empty<byte>());
                    Console.WriteLine($"Received application on message. - {payloadText}");
                    return Task.CompletedTask;
                };

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("store/getData");
                    })
                .Build();
            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            await _mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("MQTT client subscribed to topic - store/getData");

        }
    }
}
