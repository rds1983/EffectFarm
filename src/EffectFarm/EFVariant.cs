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
	}
}
