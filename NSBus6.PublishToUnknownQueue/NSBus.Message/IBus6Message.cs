namespace NSBus.Message
{
    public interface IBus6Message : IMessage
    {
        string Data { get; set; }
    }

    public interface IMessage
    {
        string ID { get; set; }
    }
}