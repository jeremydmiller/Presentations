namespace async.Internal
{
    public interface IMessagePublisher
    {
        void Publish<T>(T @event);
    }
}