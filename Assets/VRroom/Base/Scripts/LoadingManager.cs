using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using static VRroom.Base.Debugging.Console;

namespace VRroom.Base {
	[PublicAPI]
	public static class LoadingManager {
		private static readonly SortedList<int, Action> StartupTasks = new();
		private static readonly SortedList<int, Action> LoadingTasks = new();
		private static readonly SortedList<int, Action> ShutdownTasks = new();
		private static CoroutineRunner _coroutineRunner;
		private const float LoadingFrameBudget = 50;
		
		public static float Progress { get; private set; }
		public static void AddStartupTask(int priority, Action callback) => StartupTasks.Add(priority, callback);
		public static void AddLoadingTask(int priority, Action callback) => LoadingTasks.Add(priority, callback);
		public static void AddShutdownTask(int priority, Action callback) => ShutdownTasks.Add(priority, callback);

		public static async void LoadScene(string scenePath) {
			try {
				CameraTransition.Transition(true); 
				await WaitForTransition().AsTask();
				DisableSystems();
				AsyncOperation operation = SceneManager.LoadSceneAsync(scenePath);
				if (operation == null) Error($"Failed to load scene: {scenePath}");
			} catch (Exception e) {
				Exception(e, $"Error loading scene: {scenePath}");
				EnableSystems();
			}
		}

		private static async void RunLoadingTasks(Scene scene, LoadSceneMode mode) {
			if (mode != LoadSceneMode.Single) return;
			await RunTasks(LoadingTasks).AsTask();
			EnableSystems();
			CameraTransition.Transition(false);
		}

		private static async void RunShutdownTasks() {
			CameraTransition.Transition(true);
			await WaitForTransition().AsTask();
			DisableSystems();
			await RunTasks(ShutdownTasks).AsTask();
		}

		private static IEnumerator RunTasks(SortedList<int, Action> tasks) {
			Stopwatch watch = Stopwatch.StartNew();

			int completed = 0;
			foreach (Action task in tasks.Values) {
				try {
					task?.Invoke();
				} catch (Exception e) {
					Exception(e, "Error running task");
				}

				Progress = ++completed / (float)tasks.Count;
				if (watch.ElapsedMilliseconds < LoadingFrameBudget) continue;
				yield return null;
				watch.Restart();
			}
		}

		private static void DisableSystems() {
			Physics.simulationMode = SimulationMode.Script;
			Physics2D.simulationMode = SimulationMode2D.Script;
			AudioListener.pause = true;
			QualitySettings.vSyncCount = 0;
		}

		private static void EnableSystems() {
			Physics.simulationMode = SimulationMode.FixedUpdate;
			Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
			AudioListener.pause = false;
			QualitySettings.vSyncCount = 1;
		}

		private static IEnumerator WaitForTransition() {
			yield return new WaitUntil(() => !CameraTransition.Transitioning);
		}
		
		private static Task AsTask(this IEnumerator coroutine) {
			TaskCompletionSource<bool> tcs = new();
			if (_coroutineRunner) _coroutineRunner.StartCoroutine(WrapCoroutine());
			else tcs.SetResult(true);
			return tcs.Task;
			IEnumerator WrapCoroutine() {
				yield return coroutine;
				tcs.SetResult(true);
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static async void Init() {
			DisableSystems();
			Application.quitting += RunShutdownTasks;
			SceneManager.sceneLoaded += RunLoadingTasks;
			_coroutineRunner = CoroutineRunner.Instance;
			await RunTasks(StartupTasks).AsTask();
			// remove when loading implemented
			EnableSystems();
			CameraTransition.Transition(false);
		}
	}
}