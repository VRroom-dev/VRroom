using System;
using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace VRroom.Base.UI {
	[PublicAPI]
	public partial class BaseBuilder<T> where T : VisualElement, new() {
		public readonly T BaseElement = new();
		
		public BaseBuilder<T> Element(Action<ElementBuilder> builderAction = null) {
			ElementBuilder builder = new();
			builderAction?.Invoke(builder);
			BaseElement.Add(builder.BaseElement);
			return this;
		}

		public BaseBuilder<T> Label(Action<LabelBuilder> builderAction = null) {
			LabelBuilder builder = new();
			builderAction?.Invoke(builder);
			BaseElement.Add(builder.BaseElement);
			return this;
		}

		public BaseBuilder<T> Button(Action<ButtonBuilder> builderAction = null) {
			ButtonBuilder builder = new();
			builderAction?.Invoke(builder);
			BaseElement.Add(builder.BaseElement);
			return this;
		}

		public BaseBuilder<T> Toggle(Action<ToggleBuilder> builderAction = null) {
			ToggleBuilder builder = new();
			builderAction?.Invoke(builder);
			BaseElement.Add(builder.BaseElement);
			return this;
		}
		
		public BaseBuilder<T> TextField(Action<TextFieldBuilder> builderAction = null) {
			TextFieldBuilder builder = new();
			builderAction?.Invoke(builder);
			BaseElement.Add(builder.BaseElement);
			return this;
		}

		public BaseBuilder<T> Subscribe<TEvent>(Action<TEvent, BaseBuilder<T>> builderAction, TrickleDown trickleDown = TrickleDown.NoTrickleDown) where TEvent : EventBase<TEvent>, new() {
			BaseElement.RegisterCallback<TEvent>(e => builderAction(e, this), trickleDown);
			return this;
		}

		public BaseBuilder<T> Get(out VisualElement element) {
			element = BaseElement;
			return this;
		}

		public BaseBuilder<T> Get(out Label element) {
			element = BaseElement as Label;
			return this;
		}

		public BaseBuilder<T> Get(out Button element) {
			element = BaseElement as Button;
			return this;
		}

		public BaseBuilder<T> Get(out Toggle element) {
			element = BaseElement as Toggle;
			return this;
		}

		public BaseBuilder<T> Get(out TextField element) {
			element = BaseElement as TextField;
			return this;
		}

		public BaseBuilder<T> AddClass(string name, bool enabled = true) {
			BaseElement.AddToClassList(name);
			BaseElement.EnableInClassList(name, enabled);
			return this;
		}

		public BaseBuilder<T> Focusable(bool focusable) {
			BaseElement.focusable = false;
			return this;
		}

		public BaseBuilder<T> Enabled(bool enabled) {
			BaseElement.SetEnabled(enabled);
			return this;
		}

		public BaseBuilder<T> Name(string name) {
			BaseElement.name = name;
			return this;
		}
	}
	
	public static class UIBuilder {
		public static VisualElement Build(Action<ElementBuilder> builderAction) {
			ElementBuilder builder = new();
			builderAction(builder);
			return builder.BaseElement;
		}
	}
	
	public class ElementBuilder : BaseBuilder<VisualElement> { }
}