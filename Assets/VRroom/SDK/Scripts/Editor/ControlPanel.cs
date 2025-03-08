using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRroom.Base.Networking;
using VRroom.Base.UI;
using VRroom.SDK.Networking;

namespace VRroom.SDK.Editor {
	public class ControlPanel : EditorWindow {
		private VisualElement _loginContainer;
		private VisualElement _logoutContainer;
		private VisualElement _loginButton;
		private TextField _usernameField;
		private TextField _passwordField;
		
		[MenuItem("VRroom/Control Panel")]
		private static void OpenWindow() => CreateWindow<ControlPanel>();
		
		public void CreateGUI() {
			rootVisualElement.Add(UIBuilder.Build(b => b
				.Element(b => b
					.Margin(5)
					.Get(out _loginContainer)
					.TextField(b => b
						.Label("Username")
						.Get(out _usernameField)
						.MinHeight(35)
						.Margin(0)
						.Column()
						.Enabled(false)
					)
					.TextField(b => b
						.Label("Password")
						.Get(out _passwordField)
						.MinHeight(35)
						.Margin(0)
						.Column()
						.Enabled(false)
					)
					.Button(b => b
						.OnClick(Login)
						.Text("Login")
						.Get(out _loginButton)
						.Margin(0, 0, 5, 0)
						.Enabled(false)
					)
				)
				.Element(b => b
					.Display(DisplayStyle.None)
					.Get(out _logoutContainer)
					.Button(b => b
						.OnClick(Logout)
						.Text("Logout")
						.Margin(0, 0, 5, 0)
					)
				)
			));

			if (SDKAPI.IsAuthenticated()) {
				_loginContainer.style.display = DisplayStyle.None;
				_logoutContainer.style.display = DisplayStyle.Flex;
			}
			
			_usernameField.SetEnabled(true);
			_passwordField.SetEnabled(true);
			_loginButton.SetEnabled(true);
		}

		private async void Login() {
			if (string.IsNullOrEmpty(_usernameField.value) || string.IsNullOrEmpty(_passwordField.value)) return;
			Response response = await SDKAPI.Login(_usernameField.value, _passwordField.value);
			if (!response.Success) {
				Debug.LogError($"Failed To Login: {response.Result}");
				return;
			}
			
			_loginContainer.style.display = DisplayStyle.None;
			_logoutContainer.style.display = DisplayStyle.Flex;
		}

		private void Logout() {
			SDKAPI.Logout();
			_loginContainer.style.display = DisplayStyle.Flex;
			_logoutContainer.style.display = DisplayStyle.None;
		}
	}
}