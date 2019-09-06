namespace EffectFarm
{
	public struct EFVariant
	{
		public EFPlatform Platform;
		public string Defines;

		public EFVariant(EFPlatform platform)
		{
			Platform = platform;
			Defines = string.Empty;
		}

		public override string ToString()
		{
			return Platform + "/" + Defines;
		}

		public bool Equals(EFVariant v)
		{
			return Platform == v.Platform && Defines == v.Defines;
		}

		public static bool operator ==(EFVariant a, EFVariant b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(EFVariant a, EFVariant b)
		{
			return !a.Equals(b);
		}
	}
}
