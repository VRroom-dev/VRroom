namespace VRroom.Game.Networking {
	public class ItemTypes {
		public static string GetItemPrefix(ItemType type) {
			return type switch {
				ItemType.Avatar => "ava_",
				ItemType.Prop => "prp_",
				ItemType.World => "wrd_",
				ItemType.GameMode => "gam_",
				ItemType.User => "usr_",
				ItemType.Instance => "ins_",
				ItemType.Server => "ser_",
			};
		}
	}

	public enum ItemType {
		Avatar,
		Prop,
		World,
		GameMode,
		User,
		Instance,
		Server
	}
}