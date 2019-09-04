using System;
using System.Collections.Generic;

namespace EffectFarm
{
	class Program
	{
		static void Main(string[] args)
		{
			var result = D3DCompiler.Compile(@"D:\Projects\Nursia\src\Nursia\EffectsSource\DefaultEffect.fx",
				new Dictionary<string, string> {
					["BONES"] = "2",
					["LIGHTNING"] = "1"
				},
				"fx_2_0");


			Console.ReadKey();
		}
	}
}
