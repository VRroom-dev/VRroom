using System;

namespace VRroom.Base {
	public enum ContentType {
		Avatar,
		Prop,
		World,
		GameMode,
	}

	[Flags]
	public enum ContentWarningTags {
		None = 0,
		Explicit = 1,
		Gore = 2
	}
}