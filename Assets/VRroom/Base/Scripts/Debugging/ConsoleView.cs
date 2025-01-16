using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRroom.Base.Debugging {
	[PublicAPI, UxmlElement]
	public partial class ConsoleView : VisualElement {
		private readonly Queue<LogEntry> _processQueue = new();
		private readonly List<LogEntry> _lines = new();
		public ConsoleOutput Output { get; private set; }
		private float _currentScroll;
		private float _targetScroll;
		private float _contentHeight;
		private const float LineHeight = 16f;
		private const float CharWidth = 8.5f;
		private const float ScrollSpeed = 15f;
		private const float ScrollSmoothing = 20f;
		public bool Paused;

		public ConsoleView() {
			style.flexGrow = 1;
			style.overflow = Overflow.Hidden;
			
			RegisterCallback<WheelEvent>(OnMouseWheel);
			this.AddManipulator(new DragManipulator(this));
			generateVisualContent += OnGenerateVisualContent;
		}

		public void Update() {
			if (Math.Abs(_currentScroll - _targetScroll) > 0.01f) {
				_currentScroll = Mathf.Lerp(_currentScroll, _targetScroll, ScrollSmoothing * Time.deltaTime);
				MarkDirtyRepaint();
			}

			if (Paused) return;
			
			while (_processQueue.TryDequeue(out LogEntry entry)) {
				ProcessLog(entry);
			}
		}

		public void SetOutput(ConsoleOutput output) {
			if (Output != null) Output.OnLogAdded -= AddLog;
			Output = output;
			Output.OnLogAdded += AddLog;
        
			_lines.Clear();
			for (int i = 0; i < output.Count; i++) {
				ProcessLog(output[i]);
			}

			MarkDirtyRepaint();
		}

		public new void Clear() {
			Output.Clear();
			_lines.Clear();
			_contentHeight = 0;
			_currentScroll = 0;
			_targetScroll = 0;
			MarkDirtyRepaint();
		}

		private void AddLog(LogEntry log) => _processQueue.Enqueue(log);
		
		private void ProcessLog(LogEntry log) {
			Stack<string> splitting = new();

			foreach (var line in log.Log.Split('\n').Reverse()) {
				splitting.Push(line.Trim());
			}

			while (splitting.Count > 0) {
				string line = splitting.Pop();
				if (line.Length * CharWidth < resolvedStyle.width) {
					_lines.Add(new(line, log.Type));
					continue;
				}
				
				int splitIndex = (int)(resolvedStyle.width / CharWidth);
				if (splitIndex < 1) break;
				splitting.Push(line[splitIndex..].Trim());
				_lines.Add(new(line[..splitIndex], log.Type));
			}
			_contentHeight = (_lines.Count + 1) * LineHeight;

			float maxScroll = _contentHeight - resolvedStyle.height;
			_currentScroll = -maxScroll;
			_targetScroll = -maxScroll;
			MarkDirtyRepaint();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindWhitespaceSplit(string text) {
			int lastIndex = -1, i = 0;
			while (i < text.Length) {
				if (char.IsWhiteSpace(text[i])) lastIndex = i;
				if (++i * CharWidth > resolvedStyle.width) return lastIndex;
			}

			return -1;
		}

		private void OnMouseWheel(WheelEvent evt) {
			float maxScroll = _contentHeight - resolvedStyle.height;
			_targetScroll = Mathf.Clamp(_targetScroll - evt.delta.y * ScrollSpeed, -maxScroll, 0);
			MarkDirtyRepaint();
			evt.StopPropagation();
		}
		
		private void OnGenerateVisualContent(MeshGenerationContext context) {
			float height = contentRect.height;
			int startLine = Mathf.Max(0, Mathf.FloorToInt(-_currentScroll / LineHeight) - 1);
			int endLine = Mathf.Min(_lines.Count, startLine + Mathf.CeilToInt(height / LineHeight));

			for (int i = startLine; i < endLine; i++) {
				LogEntry entry = _lines[i];
				float y = i * LineHeight + _currentScroll + LineHeight;
				Color color = entry.Type switch {
					LogType.Error or LogType.Exception => new(1f, 0.5f, 0.5f),
					LogType.Warning => new(1f, 1f, 0.5f),
					_ => new(0.8f, 0.8f, 0.8f)
				};
				context.DrawText(entry.Log, new Vector2(5, y), 14, color);
			}
		}
		
		private class DragManipulator : MouseManipulator {
			private readonly ConsoleView _consoleView;
			private bool _isDragging;
			private Vector2 _dragStartMouse;
			private float _dragStartScroll;
        
			public DragManipulator(ConsoleView consoleView) {
				_consoleView = consoleView;
				activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
			}

			protected override void RegisterCallbacksOnTarget() {
				target.RegisterCallback<MouseDownEvent>(OnMouseDown);
				target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
				target.RegisterCallback<MouseUpEvent>(OnMouseUp);
			}

			protected override void UnregisterCallbacksFromTarget() {
				target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
				target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
				target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
			}

			private void OnMouseDown(MouseDownEvent evt) {
				if (!CanStartManipulation(evt)) return;
            
				_isDragging = true;
				_dragStartMouse = evt.mousePosition;
				_dragStartScroll = _consoleView._targetScroll;
				target.CaptureMouse();
				evt.StopPropagation();
			}

			private void OnMouseMove(MouseMoveEvent evt) {
				if (!_isDragging) return;
            
				float dragDelta = evt.mousePosition.y - _dragStartMouse.y;
				float maxScroll = _consoleView._contentHeight - _consoleView.resolvedStyle.height;
				_consoleView._targetScroll = Mathf.Clamp(_dragStartScroll + dragDelta, -maxScroll, 0);
				_consoleView.MarkDirtyRepaint();
            
				evt.StopPropagation();
			}

			private void OnMouseUp(MouseUpEvent evt) {
				if (!_isDragging) return;
            
				_isDragging = false;
				target.ReleaseMouse();
				evt.StopPropagation();
			}
		}
	}
}