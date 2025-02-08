using System;
using UnityEngine.UIElements;

namespace VRroom.Base.UI {
	public class ButtonBuilder : BaseBuilder<Button> {
		public ButtonBuilder Text(string text) {
			BaseElement.text = text;
			return this;
		}

		public ButtonBuilder OnClick(Action action) {
			BaseElement.clicked += action;
			return this;
		}
	}
}