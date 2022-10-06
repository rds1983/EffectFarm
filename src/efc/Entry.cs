using System.Collections.Generic;

namespace EffectFarm
{
	class Entry
	{
		public Define[] Defines;
		public Entry[] Children;

		public List<EFVariant> Apply(List<EFVariant> source)
		{
			var result = new List<EFVariant>();

			foreach(var sourceVariant in source)
			{
				foreach(var define in Defines)
				{
					var variant = new EFVariant
					{
						Platform = sourceVariant.Platform,
						Defines = sourceVariant.Defines != null ?
							new Dictionary<string, string>(sourceVariant.Defines) :
							new Dictionary<string, string>()
					};

					variant.Defines[define.Name] = define.Value;

					result.Add(variant);
				}
			}

			foreach(var entry in Children)
			{
				result = entry.Apply(result);
			}

			return result;
		}
	}
}
