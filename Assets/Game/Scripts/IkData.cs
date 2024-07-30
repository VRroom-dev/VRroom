using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class IkData : INetObject {
	public Vector3 RootPos;
	public Quaternion RootRot;
}