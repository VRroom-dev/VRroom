using UnityEngine.UIElements;

namespace VRroom.Base.UI {
	public partial class BaseBuilder<T> {
		public BaseBuilder<T> Row() {
			BaseElement.style.flexDirection = UnityEngine.UIElements.FlexDirection.Row;
			return this;
		}
		
		public BaseBuilder<T> RowReverse() {
			BaseElement.style.flexDirection = UnityEngine.UIElements.FlexDirection.RowReverse;
			return this;
		}

		public BaseBuilder<T> Column() {
			BaseElement.style.flexDirection = UnityEngine.UIElements.FlexDirection.Column;
			return this;
		}

		public BaseBuilder<T> ColumnReverse() {
			BaseElement.style.flexDirection = UnityEngine.UIElements.FlexDirection.ColumnReverse;
			return this;
		}
		
		public BaseBuilder<T> Grow() {
			BaseElement.style.flexGrow = 1;
			return this;
		}
		
		public BaseBuilder<T> Shrink() {
			BaseElement.style.flexShrink = 1;
			return this;
		}
		

		public BaseBuilder<T> Size(Length width, Length height) {
			BaseElement.style.width = width;
			BaseElement.style.height = height;
			return this;
		}

		public BaseBuilder<T> Position(Length top, Length right, Length bottom, Length left) {
			BaseElement.style.top = top;
			BaseElement.style.right = right;
			BaseElement.style.bottom = bottom;
			BaseElement.style.left = left;
			return this;
		}

		public BaseBuilder<T> Margin(Length horizontal, Length vertical) => Margin(horizontal, horizontal, vertical, vertical);
		public BaseBuilder<T> Margin(Length value) => Margin(value, value, value, value);
		public BaseBuilder<T> Margin(Length left, Length right, Length top, Length bottom) {
			BaseElement.style.marginLeft = left;
			BaseElement.style.marginRight = right;
			BaseElement.style.marginTop = top;
			BaseElement.style.marginBottom = bottom;
			return this;
		}
		
		public BaseBuilder<T> Padding(Length horizontal, Length vertical) => Padding(horizontal, horizontal, vertical, vertical);
		public BaseBuilder<T> Padding(Length value) => Padding(value, value, value, value);
		public BaseBuilder<T> Padding(Length left, Length right, Length top, Length bottom) {
			BaseElement.style.paddingLeft = left;
			BaseElement.style.paddingRight = right;
			BaseElement.style.paddingTop = top;
			BaseElement.style.paddingBottom = bottom;
			return this;
		}
	}
}