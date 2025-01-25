

public class MonoBehaviour(int id) : IComponent {
	public int InstanceId { get; } = id;

	public string name {
		get => ObjectInterop.GetName(InstanceId);
		set => ObjectInterop.SetName(InstanceId, value);
	}

	public override string ToString() => ObjectInterop.ToString(InstanceId);

	public GameObject gameObject => ComponentInterop.GetGameObject(InstanceId);
	public Transform transform => ComponentInterop.GetTransform(InstanceId);
    
	public string tag {
		get => ComponentInterop.GetTag(InstanceId);
		set => ComponentInterop.SetTag(InstanceId, value);
	}
}