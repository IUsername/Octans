namespace Octans.Memory
{
    public interface IObjectArena
    {
        T Create<T>() where T : new();
    }
}