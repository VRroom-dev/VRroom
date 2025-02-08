using UnityEngine.UIElements;

namespace VRroom.Base.UI {
	public class LabelBuilder : BaseBuilder<Label> {
		public LabelBuilder Text(string text) {
			BaseElement.text = text;
			return this;
		}
		
		public LabelBuilder EnableRichText(bool enable = true) {
			BaseElement.enableRichText = enable;
			return this;
		}
	}
}