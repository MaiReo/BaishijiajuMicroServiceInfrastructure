namespace Core.Messages
{
    public interface IMessageConverter
    {
        IMessage Deserialize(IMessageDescriptor descriptor, byte[] message);

        byte[] Serialize(IMessage message);
    }
}