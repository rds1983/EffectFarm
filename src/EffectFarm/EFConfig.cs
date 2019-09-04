using System.Collections.Generic;

namespace EffectFarm
{
	public class EFConfig
	{
		public EFTarget[] Targets { get; set; }
		public Dictionary<string, EFVariant> Variants { get; set; }
	}
}
