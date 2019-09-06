using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EffectFarm
{
	public struct EFVariant
	{
		public EFPlatform Platform;
		public Dictionary<string, string> Defines;

		public EFVariant(EFPlatform platform)
		{
			Platform = platform;
			Defines = null;
		}

		public override string ToString()
		{
			return BuildKey(Platform, Defines);
		}

		public static EFVariant FromString(string data)
		{
			var parts = data.Split('/');
			var platform = (EFPlatform)Enum.Parse(typeof(EFPlatform), parts[0]);
			Dictionary<string, string> defines = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(parts[1]))
			{
				var parts2 = parts[1].Split(';');
				foreach(var p in parts2)
				{
					var parts3 = p.Split('=');
					defines[parts3[0]] = parts3[1];
				}
			}

			return new EFVariant(platform)
			{
				Defines = defines
			};

		}

		public bool Equals(EFVariant v)
		{
			return ToString() == v.ToString();
		}

		public static string BuildKey(Dictionary<string, string> defines)
		{
			if (defines == null || defines.Count == 0)
			{
				return string.Empty;
			}

			return string.Join(";",
				from k in defines.Keys
				where !string.IsNullOrEmpty(k)
				orderby k
				select k + "=" + defines[k]);
		}

		public static string BuildKey(EFPlatform platform, Dictionary<string, string> defines)
		{
			return platform.ToString() + "/" + BuildKey(defines);
		}
	}
}
