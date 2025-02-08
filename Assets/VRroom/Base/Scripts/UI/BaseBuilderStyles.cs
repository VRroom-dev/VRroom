using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRroom.Base.UI {
	public partial class BaseBuilder<T> {
		public BaseBuilder<T> AlignContent(Align value) {
			BaseElement.style.alignContent = value;
			return this;
		}

		public BaseBuilder<T> AlignItems(Align value) {
			BaseElement.style.alignItems = value;
			return this;
		}

		public BaseBuilder<T> AlignSelf(Align value) {
			BaseElement.style.alignSelf = value;
			return this;
		}

		public BaseBuilder<T> BackgroundColor(Color value) {
			BaseElement.style.backgroundColor = value;
			return this;
		}

		public BaseBuilder<T> BackgroundImage(StyleBackground value) {
			BaseElement.style.backgroundImage = value;
			return this;
		}

		public BaseBuilder<T> BackgroundPositionX(StyleBackgroundPosition value) {
			BaseElement.style.backgroundPositionX = value;
			return this;
		}

		public BaseBuilder<T> BackgroundPositionY(StyleBackgroundPosition value) {
			BaseElement.style.backgroundPositionY = value;
			return this;
		}

		public BaseBuilder<T> BackgroundRepeat(StyleBackgroundRepeat value) {
			BaseElement.style.backgroundRepeat = value;
			return this;
		}

		public BaseBuilder<T> BackgroundSize(StyleBackgroundSize value) {
			BaseElement.style.backgroundSize = value;
			return this;
		}

		public BaseBuilder<T> BorderLeftColor(Color value) {
			BaseElement.style.borderLeftColor = value;
			return this;
		}

		public BaseBuilder<T> BorderRightColor(Color value) {
			BaseElement.style.borderRightColor = value;
			return this;
		}

		public BaseBuilder<T> BorderTopColor(Color value) {
			BaseElement.style.borderTopColor = value;
			return this;
		}

		public BaseBuilder<T> BorderBottomColor(Color value) {
			BaseElement.style.borderBottomColor = value;
			return this;
		}

		public BaseBuilder<T> BorderColor(Color value) {
			BaseElement.style.borderLeftColor = value;
			BaseElement.style.borderRightColor = value;
			BaseElement.style.borderTopColor = value;
			BaseElement.style.borderBottomColor = value;
			return this;
		}

		public BaseBuilder<T> BorderTopLeftRadius(Length value) {
			BaseElement.style.borderTopLeftRadius = value;
			return this;
		}

		public BaseBuilder<T> BorderTopRightRadius(Length value) {
			BaseElement.style.borderTopRightRadius = value;
			return this;
		}

		public BaseBuilder<T> BorderBottomLeftRadius(Length value) {
			BaseElement.style.borderBottomLeftRadius = value;
			return this;
		}

		public BaseBuilder<T> BorderBottomRightRadius(Length value) {
			BaseElement.style.borderBottomRightRadius = value;
			return this;
		}

		public BaseBuilder<T> BorderRadius(Length value) {
			BaseElement.style.borderTopLeftRadius = value;
			BaseElement.style.borderTopRightRadius = value;
			BaseElement.style.borderBottomLeftRadius = value;
			BaseElement.style.borderBottomRightRadius = value;
			return this;
		}

		public BaseBuilder<T> BorderLeftWidth(float value) {
			BaseElement.style.borderLeftWidth = value;
			return this;
		}

		public BaseBuilder<T> BorderRightWidth(float value) {
			BaseElement.style.borderRightWidth = value;
			return this;
		}

		public BaseBuilder<T> BorderTopWidth(float value) {
			BaseElement.style.borderTopWidth = value;
			return this;
		}

		public BaseBuilder<T> BorderBottomWidth(float value) {
			BaseElement.style.borderBottomWidth = value;
			return this;
		}

		public BaseBuilder<T> BorderWidth(float value) {
			BaseElement.style.borderLeftWidth = value;
			BaseElement.style.borderRightWidth = value;
			BaseElement.style.borderTopWidth = value;
			BaseElement.style.borderBottomWidth = value;
			return this;
		}

		public BaseBuilder<T> Bottom(Length value) {
			BaseElement.style.bottom = value;
			return this;
		}

		public BaseBuilder<T> Color(Color value) {
			BaseElement.style.color = value;
			return this;
		}

		public BaseBuilder<T> Cursor(StyleCursor value) {
			BaseElement.style.cursor = value;
			return this;
		}

		public BaseBuilder<T> Display(DisplayStyle value) {
			BaseElement.style.display = value;
			return this;
		}

		public BaseBuilder<T> FlexBasis(StyleLength value) {
			BaseElement.style.flexBasis = value;
			return this;
		}

		public BaseBuilder<T> FlexDirection(FlexDirection value) {
			BaseElement.style.flexDirection = value;
			return this;
		}

		public BaseBuilder<T> FlexGrow(float value) {
			BaseElement.style.flexGrow = value;
			return this;
		}

		public BaseBuilder<T> FlexShrink(float value) {
			BaseElement.style.flexShrink = value;
			return this;
		}

		public BaseBuilder<T> FlexWrap(Wrap value) {
			BaseElement.style.flexWrap = value;
			return this;
		}

		public BaseBuilder<T> FontSize(Length value) {
			BaseElement.style.fontSize = value;
			return this;
		}

		public BaseBuilder<T> Height(Length value) {
			BaseElement.style.height = value;
			return this;
		}

		public BaseBuilder<T> JustifyContent(Justify value) {
			BaseElement.style.justifyContent = value;
			return this;
		}

		public BaseBuilder<T> Left(Length value) {
			BaseElement.style.left = value;
			return this;
		}

		public BaseBuilder<T> LetterSpacing(Length value) {
			BaseElement.style.letterSpacing = value;
			return this;
		}

		public BaseBuilder<T> MarginBottom(Length value) {
			BaseElement.style.marginBottom = value;
			return this;
		}

		public BaseBuilder<T> MarginLeft(Length value) {
			BaseElement.style.marginLeft = value;
			return this;
		}

		public BaseBuilder<T> MarginRight(Length value) {
			BaseElement.style.marginRight = value;
			return this;
		}

		public BaseBuilder<T> MarginTop(Length value) {
			BaseElement.style.marginTop = value;
			return this;
		}

		public BaseBuilder<T> MaxHeight(Length value) {
			BaseElement.style.maxHeight = value;
			return this;
		}

		public BaseBuilder<T> MaxWidth(Length value) {
			BaseElement.style.maxWidth = value;
			return this;
		}

		public BaseBuilder<T> MinHeight(Length value) {
			BaseElement.style.minHeight = value;
			return this;
		}

		public BaseBuilder<T> MinWidth(Length value) {
			BaseElement.style.minWidth = value;
			return this;
		}

		public BaseBuilder<T> Opacity(float value) {
			BaseElement.style.opacity = value;
			return this;
		}

		public BaseBuilder<T> Overflow(Overflow value) {
			BaseElement.style.overflow = value;
			return this;
		}

		public BaseBuilder<T> PaddingBottom(Length value) {
			BaseElement.style.paddingBottom = value;
			return this;
		}

		public BaseBuilder<T> PaddingLeft(Length value) {
			BaseElement.style.paddingLeft = value;
			return this;
		}

		public BaseBuilder<T> PaddingRight(Length value) {
			BaseElement.style.paddingRight = value;
			return this;
		}

		public BaseBuilder<T> PaddingTop(Length value) {
			BaseElement.style.paddingTop = value;
			return this;
		}

		public BaseBuilder<T> Position(Position value) {
			BaseElement.style.position = value;
			return this;
		}

		public BaseBuilder<T> Right(Length value) {
			BaseElement.style.right = value;
			return this;
		}

		public BaseBuilder<T> Rotate(Rotate value) {
			BaseElement.style.rotate = value;
			return this;
		}

		public BaseBuilder<T> Scale(Scale value) {
			BaseElement.style.scale = value;
			return this;
		}

		public BaseBuilder<T> TextOverflow(TextOverflow value) {
			BaseElement.style.textOverflow = value;
			return this;
		}

		public BaseBuilder<T> TextShadow(TextShadow value) {
			BaseElement.style.textShadow = value;
			return this;
		}

		public BaseBuilder<T> Top(Length value) {
			BaseElement.style.top = value;
			return this;
		}

		public BaseBuilder<T> TransformOrigin(TransformOrigin value) {
			BaseElement.style.transformOrigin = value;
			return this;
		}

		public BaseBuilder<T> TransitionDelay(List<TimeValue> value) {
			BaseElement.style.transitionDelay = value;
			return this;
		}

		public BaseBuilder<T> TransitionDuration(List<TimeValue> value) {
			BaseElement.style.transitionDuration = value;
			return this;
		}

		public BaseBuilder<T> TransitionProperty(StyleList<StylePropertyName> value) {
			BaseElement.style.transitionProperty = value;
			return this;
		}

		public BaseBuilder<T> TransitionTimingFunction(StyleList<EasingFunction> value) {
			BaseElement.style.transitionTimingFunction = value;
			return this;
		}

		public BaseBuilder<T> Translate(Translate value) {
			BaseElement.style.translate = value;
			return this;
		}

		public BaseBuilder<T> UnityBackgroundImageTintColor(Color value) {
			BaseElement.style.unityBackgroundImageTintColor = value;
			return this;
		}

		public BaseBuilder<T> UnityEditorTextRenderingMode(EditorTextRenderingMode value) {
			BaseElement.style.unityEditorTextRenderingMode = value;
			return this;
		}

		public BaseBuilder<T> UnityFont(Font value) {
			BaseElement.style.unityFont = value;
			return this;
		}

		public BaseBuilder<T> UnityFontDefinition(FontDefinition value) {
			BaseElement.style.unityFontDefinition = value;
			return this;
		}

		public BaseBuilder<T> UnityFontStyleAndWeight(FontStyle value) {
			BaseElement.style.unityFontStyleAndWeight = value;
			return this;
		}

		public BaseBuilder<T> UnityOverflowClipBox(OverflowClipBox value) {
			BaseElement.style.unityOverflowClipBox = value;
			return this;
		}

		public BaseBuilder<T> UnityParagraphSpacing(Length value) {
			BaseElement.style.unityParagraphSpacing = value;
			return this;
		}

		public BaseBuilder<T> UnitySliceBottom(int value) {
			BaseElement.style.unitySliceBottom = value;
			return this;
		}

		public BaseBuilder<T> UnitySliceLeft(int value) {
			BaseElement.style.unitySliceLeft = value;
			return this;
		}

		public BaseBuilder<T> UnitySliceRight(int value) {
			BaseElement.style.unitySliceRight = value;
			return this;
		}

		public BaseBuilder<T> UnitySliceTop(int value) {
			BaseElement.style.unitySliceTop = value;
			return this;
		}

		public BaseBuilder<T> UnitySliceScale(float value) {
			BaseElement.style.unitySliceScale = value;
			return this;
		}

		public BaseBuilder<T> UnityTextAlign(TextAnchor value) {
			BaseElement.style.unityTextAlign = value;
			return this;
		}

		public BaseBuilder<T> UnityTextGenerator(TextGeneratorType value) {
			BaseElement.style.unityTextGenerator = value;
			return this;
		}

		public BaseBuilder<T> UnityTextOutlineColor(Color value) {
			BaseElement.style.unityTextOutlineColor = value;
			return this;
		}

		public BaseBuilder<T> UnityTextOutlineWidth(float value) {
			BaseElement.style.unityTextOutlineWidth = value;
			return this;
		}

		public BaseBuilder<T> UnityTextOverflowPosition(TextOverflowPosition value) {
			BaseElement.style.unityTextOverflowPosition = value;
			return this;
		}

		public BaseBuilder<T> Visibility(Visibility value) {
			BaseElement.style.visibility = value;
			return this;
		}

		public BaseBuilder<T> WhiteSpace(WhiteSpace value) {
			BaseElement.style.whiteSpace = value;
			return this;
		}

		public BaseBuilder<T> Width(Length value) {
			BaseElement.style.width = value;
			return this;
		}

		public BaseBuilder<T> WordSpacing(Length value) {
			BaseElement.style.wordSpacing = value;
			return this;
		}
	}
}