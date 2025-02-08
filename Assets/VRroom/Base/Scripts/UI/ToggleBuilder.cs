using System;
using UnityEngine.UIElements;

namespace VRroom.Base.UI {
	public class ToggleBuilder : BaseBuilder<Toggle> {
		public ToggleBuilder Text(string text) {
			BaseElement.text = text;
			return this;
		}
		
		public ToggleBuilder Tooltip(string text) {
			BaseElement.tooltip = text;
			return this;
		}
		
		public ToggleBuilder Label(string text) {
			BaseElement.label = text;
			return this;
		}
		
		public ToggleBuilder Value(bool value) {
			BaseElement.value = value;
			return this;
		}
	}
}