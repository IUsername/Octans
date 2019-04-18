namespace Octans
{
    public interface IObjectArena
    {
        T Create<T>() where T : new();
    }
}