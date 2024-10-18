using System.Collections.Generic;
using JetBrains.Annotations;

[PublicAPI]
public class GameStateManager {
    public static GameStateManager Instance { get; } = new();
    private static readonly Dictionary<string, object> States = new();
    private static readonly Dictionary<string, StateChangedHandler> StateChangedEvents = new();

    public object this[string key] {
        get => States.GetValueOrDefault(key);
        set {
            States[key] = value;
            StateChangedEvents.GetValueOrDefault(key)?.Invoke(key, value);
        }
    }

    /// <summary> sets a state without triggering a changed event </summary>
    public static void Set(string key, object value) {
        States[key] = value;
    }
    
    public static void Subscribe(string key, StateChangedHandler handler) {
        if (StateChangedEvents.ContainsKey(key)) {
            StateChangedEvents[key] += handler;
        }
    }
    
    public static void Unsubscribe(string key, StateChangedHandler handler) {
        if (StateChangedEvents.ContainsKey(key)) {
            StateChangedEvents[key] -= handler;
        }
    }
    
    public delegate void StateChangedHandler(string key, object value);
}