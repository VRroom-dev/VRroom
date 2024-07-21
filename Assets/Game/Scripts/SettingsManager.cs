using System.Collections.Generic;

public class StateManager {
    public static StateManager Instance { get; } = new();
    private static readonly Dictionary<string, object> States = new();
    private static readonly Dictionary<string, SettingChangedHandler> StateChangedEvents = new();

    public object this[string key] {
        get => States.GetValueOrDefault(key);
        set {
            if (!States.TryAdd(key, value)) {
                States[key] = value;
            }
            StateChangedEvents[key]?.Invoke(value);
        }
    }
    
    public static void Subscribe(string key, SettingChangedHandler handler) {
        StateChangedEvents[key] += handler;
    }
    
    public static void Unsubscribe(string key, SettingChangedHandler handler) {
        if (StateChangedEvents.ContainsKey(key)) {
            StateChangedEvents[key] -= handler;
        }
    }
    
    public delegate void SettingChangedHandler(object value);
}