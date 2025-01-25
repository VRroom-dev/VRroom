

public readonly struct GameObject(int id) : IObject {
	public int InstanceId { get; } = id;

	public string name {
		get => ObjectInterop.GetName(InstanceId);
		set => ObjectInterop.SetName(InstanceId, value);
	}

	public override string ToString() => ObjectInterop.ToString(InstanceId);
}