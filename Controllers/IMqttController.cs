namespace ATARK_Backend.Controllers
{
    public interface IMqttController
    {
        Task PublishMethodAsync(string topic, string messagePayload);

    }
}
