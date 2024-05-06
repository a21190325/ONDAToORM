namespace Contracts
{
    public interface IInstanceMapper
    {
        TDestination Map<TDestination>(object source);
    }
}
