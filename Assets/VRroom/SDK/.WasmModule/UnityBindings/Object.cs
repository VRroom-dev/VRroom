using System.Runtime.InteropServices;

public interface IObject {
    int InstanceId { get; }
    string name { get; set; }
    string ToString();
}

public readonly struct Object(int id) : IObject {
    public int InstanceId { get; } = id;

    public string name {
        get => ObjectInterop.GetName(InstanceId);
        set => ObjectInterop.SetName(InstanceId, value);
    }

    public override string ToString() => ObjectInterop.ToString(InstanceId);

    public static void Destroy(IObject obj) => ObjectInterop.object_destroy(obj.InstanceId);
    public static void Instantiate(IObject obj) => ObjectInterop.object_instantiate(obj.InstanceId);

    public static implicit operator Object(Component component) => new(component.InstanceId);
    public static implicit operator Object(GameObject gameObject) => new(gameObject.InstanceId);
}

internal static class ObjectInterop {
    public static string GetName(int id) {
        object_name_get(id);
        return ReadString();
    }
    
    public static unsafe void SetName(int id, string name) {
        fixed (char* str = name) {
            WriteString(str, name.Length);
            object_name_set(id);
        }
    }
    
    public static string ToString(int id) {
        object_toString(id);
        return ReadString();
    }
    
    
    [WasmImportLinkage, DllImport("unity")]
    private static extern void object_name_get(int id);

    [WasmImportLinkage, DllImport("unity")]
    private static extern void object_name_set(int id);

    [WasmImportLinkage, DllImport("unity")]
    private static extern void object_toString(int id);

    [WasmImportLinkage, DllImport("unity")]
    public static extern void object_destroy(int id);

    [WasmImportLinkage, DllImport("unity")]
    public static extern void object_instantiate(int id);
}