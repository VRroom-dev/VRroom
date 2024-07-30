using MemoryPack;

[MemoryPackable(SerializeLayout.Sequential)]
[MemoryPackUnion(0, typeof(IkData))]
public interface INetObject { }