using System.Collections.Generic;

namespace EffectFarm
{
	class Config
	{
		public EFPlatform[] Targets { get; set; }
		public Entry Root;

		public List<EFVariant> BuildVariants()
		{
			var result = new List<EFVariant>
			{
				new EFVariant(EFPlatform.MonoGameDirectX),
				new EFVariant(EFPlatform.MonoGameOpenGL),
				new EFVariant(EFPlatform.FNA)
			};

			result = Root.Apply(result);

			return result;
		}
	}
}
