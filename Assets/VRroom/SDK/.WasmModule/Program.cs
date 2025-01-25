using System.Reflection;
using System.Runtime.InteropServices;

public static class Program {
    private static readonly Dictionary<int, MonoBehaviour> Behaviours = new();
    private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> Callbacks = new();
    
	[UnmanagedCallersOnly(EntryPoint = "CreateInstance")]
	public static void CreateInstance(int id) {
        string name = ReadString();
		Type type = Type.GetType(name)!;
        object obj = Activator.CreateInstance(type);
        if (obj is not MonoBehaviour behaviour) return;
        Behaviours[id] = behaviour;
        
        if (Callbacks.ContainsKey(type)) return;
        Dictionary<string, MethodInfo> callbacks = new();
        MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (MethodInfo method in methods) callbacks[method.Name] = method;
        Callbacks[type] = callbacks;
    }

    [UnmanagedCallersOnly(EntryPoint = "Alloc")]
    public static IntPtr Alloc(int length) {
        return Marshal.AllocHGlobal(length);
    }
    
    [UnmanagedCallersOnly(EntryPoint = "Call")]
    public static void Call(int id) {
        string method = ReadString();
        MonoBehaviour behaviour = Behaviours[id];
        Callbacks[behaviour.GetType()][method].Invoke(behaviour, null);
    }
    
    [UnmanagedCallersOnly(EntryPoint = "AllocArrayPassthrough")]
    public static unsafe int AllocArrayPassthrough() {
        ArrayPassthrough = (PassthroughArray*)Marshal.AllocHGlobal(8);
        return (int)ArrayPassthrough;
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ExecuteOnMainThreadAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class DefaultExecutionOrderAttribute(int order) : Attribute {
    public readonly int Order = order;
}