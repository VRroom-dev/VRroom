using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace VRroom.Base.Debugging {
	[PublicAPI]
	public static class StatisticTracker {
		private static readonly Dictionary<string, Statistic> Statistics = new();
		private static float _timeSinceLastUpdate;
		public static bool UpdatedThisFrame { get; private set; }
		public const int LogWindow = 15;
		public const int LogFPS = 60;
		public const float LogInterval = 1f / LogFPS;

		static StatisticTracker() {
			AddStatistic(new Statistic(
                "FPS",
                Color.green,
                _ => 1f / Time.smoothDeltaTime
            ));
            
            AddStatistic(new Statistic(
                "Memory",
                Color.magenta,
                _ => GC.GetTotalMemory(false) / 1048576f
            ));
		}

		public static Statistic[] GetStatistics() => Statistics.Values.ToArray();
		public static Statistic GetStatistic(string name) => Statistics.GetValueOrDefault(name);
		public static void AddStatistic(Statistic statistic) => Statistics[statistic.Name] = statistic;

		internal static void Update() {
			_timeSinceLastUpdate += Time.deltaTime;
			UpdatedThisFrame = false;
			if (_timeSinceLastUpdate < LogInterval) return;
			UpdatedThisFrame = true;

			foreach (Statistic statistic in Statistics.Values)
				statistic.Update(_timeSinceLastUpdate);

			_timeSinceLastUpdate %= LogInterval;
		}

		public static void ClearAll() {
			foreach (Statistic statistic in Statistics.Values)
				statistic.Clear();
		}

		[PublicAPI]
		public class Statistic {
			public FrameLogger LogFrame;
			public string Name;
			public Color LineColor;
			public float[] Points;
			public bool Active = true;
			private int _head;
			private int _count;
			private const int Capacity = LogWindow * LogFPS;
		
			public float this[int index] => Points[(_head + index) % Capacity];
			public int Head => _head;
			public int Count => _count;
		
			public Statistic(string name, Color lineColor, FrameLogger frameLogger) {
				Name = name;
				LineColor = lineColor;
				LogFrame = frameLogger;
				Points = new float[Capacity];
			}

			public void Update(float deltaTime) {
				float point = Active ? LogFrame(deltaTime) : 0;
				Points[(_head + _count) % Capacity] = point;
				if (_count < Capacity) _count++;
				else _head = (_head + 1) % Capacity;
			}

			public void Clear() {
				Array.Clear(Points, 0, Capacity);
				_head = 0;
				_count = 0;
			}
		
			public delegate float FrameLogger(float deltaTime);
		}
	}
}