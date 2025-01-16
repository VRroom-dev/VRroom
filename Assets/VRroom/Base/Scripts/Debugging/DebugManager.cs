using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRroom.Base.Debugging {
	public class DebugManager : MonoBehaviour {
		private UIDocument _document;
		private VisualElement _rootElement;
		private VisualElement _consoleTabs;
		private ConsoleView _consoleView;
		private TextField _consoleInput;
		private StatisticElement _statisticElement;
		private VisualElement _actionsContainer;
		private VisualElement[] _tabContainers;
		private Button[] _tabButtons;
	
		private int _currentTab;
		private bool _visible;
	
		private void Start() {
			_document = GetComponent<UIDocument>();
			_rootElement = _document.rootVisualElement;
			_rootElement.style.display = DisplayStyle.None;
			
			// console setup
			VisualElement consoleContainer = _rootElement.Q("ConsolePage");
			_consoleTabs = consoleContainer.Q("ConsoleTabs");
			_consoleInput = consoleContainer.Q<TextField>("ConsoleInput");
			_consoleView = consoleContainer.Q<ConsoleView>();
			_consoleView.SetOutput(Console.GetOutput("Global"));

			consoleContainer.Q<Button>("SendButton").clicked += SendConsoleCommand;
			consoleContainer.Q<Button>("PauseButton").clicked += () => _consoleView.Paused = !_consoleView.Paused;
			consoleContainer.Q<Button>("ClearButton").clicked += _consoleView.Clear;

			foreach (ConsoleOutput output in Console.GetOutputs()) {
				_consoleTabs.Add(new Button(() => _consoleView.SetOutput(output)) {text = output.Name});
			}
			
			Console.OnConsoleCreated += o => _consoleTabs.Add(new Button(() => _consoleView.SetOutput(o)) {text = o.Name, focusable = false});
			
			// statistics setup
			_statisticElement = _rootElement.Q<StatisticElement>();

			Debug.Button("Log", () => Console.Log("Meow"));
			Debug.Button("Warn", () => Console.Warn("Meow"));
			Debug.Button("Error", () => Console.Error("Meow"));
			Debug.Button("Exception", () => Console.Exception("Meow"));
			
			// actions setup
			_actionsContainer = _rootElement.Q("ActionsPage").Q<ScrollView>();
			foreach (string name in Debug.Buttons) CreateActionButton(name);
			Debug.OnButtonAdded += CreateActionButton;

			// tabs setup
			_tabButtons = _rootElement.Q("Tabs").Children().Cast<Button>().ToArray();
			_tabContainers = _rootElement.Q("Content").Children().ToArray();

			if (_tabButtons.Length != _tabContainers.Length) throw new Exception("incorrect number of tabs vs containers");

			for (int i = 0; i < _tabContainers.Length; i++) {
				int tab = i;
				_tabButtons[i].focusable = false;
				_tabContainers[i].visible = i == 0;
				_tabButtons[i].clicked += () => {
					_tabContainers[_currentTab].visible = false;
					_tabContainers[tab].visible = true;
					_currentTab = tab;
				};
			}
		}

		private void Update() {
			if (Input.GetKeyDown(KeyCode.F3)) {
				_visible = !_visible;
				InputSystem.SetCursorLock(!_visible);
				_rootElement.style.display = _visible ? DisplayStyle.Flex : DisplayStyle.None;
				if (_visible) StatisticTracker.ClearAll();
			}

			if (!_visible) return;
			StatisticTracker.Update();
			
			// Console Page
			if (_currentTab == 0) {
				if (Input.GetKeyDown(KeyCode.Return)) SendConsoleCommand();
				_consoleView.Update();
			}
			// Statistics Page
			else if (_currentTab == 1) {
				if (StatisticTracker.UpdatedThisFrame) _statisticElement.MarkDirtyRepaint();
			}
			// Actions Page
			else if (_currentTab == 2) {
				
			}
		}

		private void CreateActionButton(string name) {
			Button button = new Button(() => Debug.ActivateButton(name)) {
				text = name,
				focusable = false
			};
			button.AddToClassList("action-button");
			_actionsContainer.Add(button);
		}

		private void SendConsoleCommand() {
			if (string.IsNullOrWhiteSpace(_consoleInput.value)) return;
			Console.ExecuteCommand(_consoleView.Output.Name, _consoleInput.value);
			_consoleInput.value = string.Empty;
			_consoleInput.Focus();
		}

		private void OnDestroy() {
			_statisticElement.Dispose();
			Console.Cleanup();
		}
	}
}