using System.Runtime.InteropServices;

public interface IComponent : IObject {
    GameObject gameObject { get; }
    Transform transform { get; }
    string tag { get; set; }
}

public readonly struct Component(int id) : IComponent {
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

    public static implicit operator Component(Transform transform) => new(transform.InstanceId);
}

internal static class ComponentInterop {
    public static string GetTag(int id) {
        component_tag_get(id);
        return ReadString();
    }
    
    public static unsafe void SetTag(int id, string name) {
        fixed (char* str = name) {
            WriteString(str, name.Length);
            component_tag_set(id);
        }
    }
    
    public static GameObject GetGameObject(int id) {
        return new(component_gameObject_get(id));
    }
    
    public static Transform GetTransform(int id) {
        return new(component_transform_get(id));
    }
    
    [WasmImportLinkage, DllImport("unity")]
    private static extern int component_gameObject_get(int id);
    
    [WasmImportLinkage, DllImport("unity")]
    private static extern int component_transform_get(int id);
    
    [WasmImportLinkage, DllImport("unity")]
    private static extern void component_tag_get(int id);

    [WasmImportLinkage, DllImport("unity")]
    private static extern void component_tag_set(int id);
}