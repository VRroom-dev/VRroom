using System;
using UnityEngine.UIElements;

namespace VRroom.Base.UI {
	public class TextFieldBuilder : BaseBuilder<TextField> {
		public TextFieldBuilder Value(string value) {
			BaseElement.value = value;
			return this;
		}

		public TextFieldBuilder Label(string text) {
			BaseElement.label = text;
			return this;
		}
		
		public TextFieldBuilder MaskCharacter(char mask) {
			BaseElement.maskChar = mask;
			return this;
		}

		public TextFieldBuilder IsPassword(bool isPassword = true) {
			BaseElement.isPasswordField = isPassword;
			return this;
		}

		public TextFieldBuilder OnValueChanged(Action<string> callback) {
			BaseElement.RegisterValueChangedCallback(evt => callback(evt.newValue));
			return this;
		}

		public TextFieldBuilder Multiline(bool multiline = true) {
			BaseElement.multiline = multiline;
			return this;
		}

		public TextFieldBuilder MaxLength(int maxLength) {
			BaseElement.maxLength = maxLength;
			return this;
		}
	}
}