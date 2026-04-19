public interface IPickable
{
    string Id { get; }
    string DisplayName { get; }
    int MaxStackSize { get; }
    void Pick(IStorable receiver);
}