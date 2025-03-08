using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRroom.Base;
using VRroom.Base.Networking;
using VRroom.Base.UI;
using VRroom.SDK.Networking;

namespace VRroom.SDK.Editor {
	[CustomEditor(typeof(ContentDescriptor))]
	public class ContentEditorGUI : UnityEditor.Editor {
		protected VisualElement RootElement;
		protected VisualElement AssetConfigContainer;
		private VisualElement _thumbnailElement;

		private TextField _guidField;
		private TextField _nameField;
		private TextField _descriptionField;

		private Button _guidButton;
		private Button _buildButton;

		private Toggle _explicitTag;
		private Toggle _goreTag;
		
		public override VisualElement CreateInspectorGUI() {
			StyleSheet sheet = StyleSheetBuilder.Build(b => b
				.Add(".description-field #unity-text-input", s => s
					.AlignSelf(Align.Auto)
					.FlexGrow(1)
					.WhiteSpace(WhiteSpace.Normal)
				)
				.Add(".content-tag .unity-base-field__input", s => s
					.FlexGrow(0)
					.Margin(5, 5)
				)
			);
			
			RootElement = UIBuilder.Build(b => b
				.Grow()
				// Guid
				.Element(b => b
					.AlignContent(Align.Center)
					.Row()
					.Grow()
					.Margin(0, 0, 5, 5)
					.Label(b => b
						.Text("Guid: ")
						.Margin(0, 5, 0, 0)
						.UnityTextAlign(TextAnchor.MiddleLeft)
					)
					.TextField(b => b
						.MaxLength(36)
						.Grow()
						.Margin(0, 5, 0, 0)
						.Get(out _guidField)
					)
					.Button(b => b
						.OnClick(SetGuid)
						.Focusable(false)
						.Margin(0)
						.Get(out _guidButton)
					)
				)
				.Element(b => b
					.Row()
					.MarginBottom(5)
					.MaxHeight(150)
					// Thumbnail
					.Element(b => b
						.AddClass("unity-help-box")
						.Margin(0, 5, 0, 0)
						.Width(150)
						.Height(150)
						.Element(b => b
							.Grow()
							.Get(out _thumbnailElement)
						)
						.Element(b => b
							.Position(Position.Absolute)
							.Left(0)
							.Right(0)
							.Bottom(0)
							.Height(30)
						)
					)
					.Element(b => b
						.Grow()
						// Name and Description
						.Element(b => b
							.Position(Position.Absolute)
							.Width(new(100, LengthUnit.Percent))
							.Height(new(100, LengthUnit.Percent))
							.TextField(b => b
								.Label("Name")
								.MaxLength(32)
								.Column()
								.Margin(0, 0, 0, 5)
								.MinHeight(35)
								.Get(out _nameField)
							)
							.TextField(b => b
								.Label("Description")
								.MaxLength(128)
								.Multiline()
								.AddClass("description-field")
								.Grow()
								.Shrink()
								.Column()
								.Margin(0)
								.Get(out _descriptionField)
							)
						)
						// Thumbnail Configuration
						.Element(b => b
							.Position(Position.Absolute)
							.Width(new(100, LengthUnit.Percent))
							.Height(new(100, LengthUnit.Percent))
							.Display(DisplayStyle.None)
						
						)
					)
				)
				// Content Tags
				.Element(b => b
					.MarginBottom(5)
					.Element(b => b
						.Row()
						.MarginBottom(5)
						.Label(b => b
							.Text("Content Warnings")
							.Grow()
							.UnityFontStyleAndWeight(FontStyle.Bold)
							.UnityTextAlign(TextAnchor.MiddleLeft)
							.Margin(0)
						)
						.Button(b => b
							.Text("Learn More")
							.Focusable(false)
							.Margin(0)
						)
					)
					.Element(b => b
						.Row()
						.Toggle(b => b
							.AddClass("unity-help-box")
							.AddClass("content-tag")
							.Get(out _explicitTag)
							.Focusable(false)
							.Margin(0, 5, 0, 0)
							.FlexGrow(1)
							.FlexBasis(0)
							.Overflow(Overflow.Hidden)
							.Element(b => b
								.Label(b => b
									.Text("Explicit")
									.Margin(0)
									.UnityFontStyleAndWeight(FontStyle.Bold)
								)
								.Label(b => b
									.Text("Features Nudity or sexual content")
									.FontSize(10)
									.UnityFontStyleAndWeight(FontStyle.BoldAndItalic)
								)
							)
						)
						.Toggle(b => b
							.AddClass("unity-help-box")
							.AddClass("content-tag")
							.Get(out _goreTag)
							.Focusable(false)
							.Margin(0)
							.FlexGrow(1)
							.FlexBasis(0)
							.Overflow(Overflow.Hidden)
							.Element(b => b
								.Label(b => b
									.Text("Gore")
									.Margin(0)
									.UnityFontStyleAndWeight(FontStyle.Bold)
								)
								.Label(b => b
									.Text("Shows Blood or other disturbing imagery")
									.FontSize(10)
									.UnityFontStyleAndWeight(FontStyle.BoldAndItalic)
								)
							)
						)
					)
				)
				.Element(b => b
					.MarginBottom(5)
					.Get(out AssetConfigContainer)
				)
				.Element(b => b
					.Button(b => b
						.Text("Build & Upload")
						.OnClick(BuildAndUpload)
						.Focusable(false)
						.Margin(0)
						.Enabled(false)
						.Get(out _buildButton)
					)
				)
			);
			
			RootElement.styleSheets.Add(sheet);
			InitializeGui();
			
			return RootElement;
		}

		private void InitializeGui() {
			ContentDescriptor descriptor = (ContentDescriptor)target;

			bool fetchFromApi = false;
			if (!string.IsNullOrEmpty(descriptor.guid)) {
				_guidField.value = descriptor.guid;
				_guidButton.text = "Detach GUID";
				_guidField.SetEnabled(false);
				//fetchFromApi = true;
			} else _guidButton.text = "Attach GUID";

			_nameField.value = descriptor.name;
			_descriptionField.value = descriptor.description;
			_explicitTag.value = descriptor.explicitTag;
			_goreTag.value = descriptor.goreTag;

			if (!fetchFromApi) {
				_buildButton.SetEnabled(true);
				return;
			}
			
			
		}

		private void SetGuid() {
			ContentDescriptor descriptor = (ContentDescriptor)target;
			if (!string.IsNullOrEmpty(descriptor.guid)) {
				descriptor.guid = null;
				_guidField.value = null;
				_guidField.SetEnabled(true);
				_guidButton.text = "Attach GUID";
			} else if (GUID.TryParse(_guidField.value.Replace("-", ""), out _)) {
				descriptor.guid = _guidField.value;
				_guidField.SetEnabled(false);
				_guidButton.text = "Detach GUID";
			}
		}

		private async void BuildAndUpload() {
			if (targets.Length != 1) {
				Debug.LogError("You can only build and upload one content at a time");
				return;
			}
			
			ContentDescriptor descriptor = (ContentDescriptor)target;
			descriptor.name = _nameField.value;
			descriptor.description = _descriptionField.value;
			descriptor.explicitTag = _explicitTag.value;
			descriptor.goreTag = _goreTag.value;

			if (string.IsNullOrEmpty(descriptor.guid)) {
				Response response = await SDKAPI.CreateContent(descriptor.name, descriptor.description, ContentType.World, ContentWarningTags.None);
				descriptor.guid = response.Result.Replace("\"", "");
			}

			string bundlePath = AssetBundleBuilder.Build(descriptor);

			await SDKAPI.UpdateContent(descriptor.guid, descriptor.name, descriptor.description, ContentWarningTags.None);
			await SDKAPI.UpdateBundle(descriptor.guid, bundlePath);
			
			Debug.Log("Upload Complete.");
		}
	}
}