using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace VRroom.Base.Debugging {
	[PublicAPI]
	public static class Debug {
		private static Dictionary<string, Action> _buttons = new();
		public static Action<string> OnButtonAdded;
		
		public static string[] Buttons => _buttons.Keys.ToArray();
		
		public static void ActivateButton(string name) => _buttons[name]();
		public static void Button(string name, Action action) => _buttons[name] = action;
	}
}