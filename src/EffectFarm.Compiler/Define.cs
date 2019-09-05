namespace EffectFarm
{
	class Define
	{
		public string Name = string.Empty;
		public string Value = string.Empty;

		public override string ToString()
		{
			return string.IsNullOrEmpty(Name)?string.Empty:Name + "=" + Value;
		}
	}
}
