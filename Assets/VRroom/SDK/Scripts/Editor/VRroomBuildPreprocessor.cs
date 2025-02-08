using VRroom.Base;

namespace VRroom.SDK.Editor {
	public abstract class VRroomBuildPreprocessor {
		public abstract int DefaultPriority { get; }

		public virtual PreprocessOrder RunsBeforeOrAfter(string type) => PreprocessOrder.Any;

		public virtual void OnPreprocess(ContentDescriptor descriptor, ContentType type) { }
		public virtual void OnCleanup() { }
	}
}