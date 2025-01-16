using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace VRroom.Base.Debugging {
	[PublicAPI]
	public static class Console {
		private static readonly ConcurrentDictionary<string, ConsoleOutput> Outputs = new();
		private static readonly ConcurrentDictionary<string, Command> Commands = new();
		public static Action<ConsoleOutput> OnConsoleCreated;
		public static bool IgnoreUnityLogs;

		public static void Log(string output, string log) => InternalLog(output, log, 3);
		public static void Warn(string output, string log) => InternalWarn(output, log, 3);
		public static void Error(string output, string log) => InternalError(output, log, 3);
		public static void Exception(string output, string log) => InternalException(output, log, 3);
		public static void Exception(string output, Exception exception, string log) => InternalException(output, exception, log, 3);
		
		public static void Log(string log) => InternalLog("Global", log, 3);
		public static void Warn(string log) => InternalWarn("Global", log, 3);
		public static void Error(string log) => InternalError("Global", log, 3);
		public static void Exception(string log) => InternalException("Global", log, 3);
		public static void Exception(Exception exception, string log) => InternalException("Global", exception, log, 3);

		public static ConsoleOutput GetOutput(string output) {
			if (Outputs.TryGetValue(output, out ConsoleOutput console)) return console;
			console = Outputs.AddOrUpdate(output, new ConsoleOutput(output), (_, _) => null);
			OnConsoleCreated?.Invoke(console);
			return console;
		}
		
		public static ConsoleOutput[] GetOutputs() => Outputs.Values.ToArray();
		public static Command[] GetCommands() => Commands.Values.ToArray();
	
		public static void AddCommand(Command command) {
			Commands.AddOrUpdate(command.Name, command, (_, _) => command);
		}
	
		public static async void ExecuteCommand(string output, string command) {
			string[] parts = command.Split(' ');
			try {
				Log(output, await Commands.GetOrAdd(parts[0], _ => throw new CommandException("Command doesn't exist")).Callback.Invoke(parts[1..]));
			} catch (Exception e) {
				if (e is CommandException) Error(output, $"Command \"{parts[0]}\" doesn't exist");
				else Exception(output, e, $"Error while executing command {parts[0]}");
			}
		}
	
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void Init() => Application.logMessageReceived += HandleUnityLogs;

		private static void HandleUnityLogs(string condition, string stackTrace, LogType type) {
			if (IgnoreUnityLogs) return;
			if (type is LogType.Exception) InternalUnityException("Unity", $"{condition}\n{stackTrace}", stackTrace);
			if (type is LogType.Error) InternalError("Unity", condition, 9);
			if (type is LogType.Warning) InternalWarn("Unity", condition, 9);
			if (type is LogType.Log) InternalLog("Unity", condition, 9);
		}

		private static string GetUnityCallingClass() {
			return "Unity";
		}
		
		private static void InternalLog(string output, string log, int frames) {
			GetOutput(output).Add($"{GetLogInfo(frames)} {log}", LogType.Log);
		}
	
		private static void InternalWarn(string output, string log, int frames) {
			GetOutput(output).Add($"{GetLogInfo(frames)} {log}", LogType.Warning);
		}
	
		private static void InternalError(string output, string log, int frames) {
			GetOutput(output).Add($"{GetLogInfo(frames)} {log}", LogType.Error);
		}
	
		private static void InternalException(string output, string log, int frames) {
			GetOutput(output).Add($"{GetLogInfo(frames)} {log}\n{Environment.StackTrace}", LogType.Exception);
		}
	
		private static void InternalException(string output, Exception exception, string log, int frames) {
			GetOutput(output).Add($"{GetLogInfo(frames)} {log} {exception}\n{Environment.StackTrace}", LogType.Exception);
		}
	
		private static void InternalUnityException(string output, string log, string stackTrace) {
			GetOutput(output).Add($"{GetUnityLogInfo(stackTrace)} {log} {stackTrace}", LogType.Exception);
		}

		private static string GetUnityLogInfo(string stackTrace) {
			string firstLine = stackTrace[..stackTrace.IndexOf("()", StringComparison.Ordinal)];
			firstLine = firstLine[..firstLine.LastIndexOf('.')];
			string className = firstLine[(firstLine.LastIndexOf('.') + 1)..];
			return $" [{DateTime.UtcNow:HH:mm:ss}] [{className}]";
		}

		private static string GetLogInfo(int frames) {
			StackFrame frame = new(frames, false);
			MethodBase method = frame.GetMethod();
			string name = method.DeclaringType?.FullName;
			if (name == null) return $"[{DateTime.UtcNow:HH:mm:ss}] [Unknown]";
			int start = name.LastIndexOf('.') + 1;
			int end = name.LastIndexOf('+');
			name = end != -1 ? name[start..end] : name[start..];
			return $"[{DateTime.UtcNow:HH:mm:ss}] [{name}]";
		}

		internal static void Cleanup() {
			Application.logMessageReceived -= HandleUnityLogs;
			OnConsoleCreated = null;
			Outputs.Clear();
			Commands.Clear();
		}
	}

	public class CommandException : Exception {
		public CommandException(string message) : base(message) {}
	}
	
	[PublicAPI]
	public class ConsoleOutput {
		private readonly LogEntry[] _logs;
		private int _head;
		private int _count;
		private int _capacity = 1000;
		public string Name { get; private set; }
		public Action<LogEntry> OnLogAdded;

		public ConsoleOutput(string name) {
			_logs = new LogEntry[_capacity];
			Name = name;
		}

		public void Add(string log, LogType type) {
			LogEntry entry = new(log, type);
			_logs[(_head + _count) % _capacity] = entry;
			if (_count < _capacity) _count++;
			else _head = (_head + 1) % _capacity;
			OnLogAdded?.Invoke(entry);
		}

		public void Clear() {
			Array.Clear(_logs, 0, _capacity);
			_head = 0;
			_count = 0;
		}
		
		public LogEntry this[int index] => _logs[(_head + index) % _capacity];
		public int Count => _count;
	}
	
	[PublicAPI]
	public class Command {
		public readonly Func<string[], Task<string>> Callback;
		public readonly string Name;
		public readonly string Hint;
		
		public Command(Func<string[], Task<string>> callback, string name, string hint) {
			Callback = callback;
			Name = name;
			Hint = hint;
		}
	}

	[PublicAPI]
	public struct LogEntry {
		public LogType Type;
		public string Log;
			
		public LogEntry(string log, LogType type) {
			Type = type;
			Log = log;
		}
	}
}