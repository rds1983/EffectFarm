using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace EffectFarm.Tests
{
	[TestFixture]
	internal class Tests
	{
		[Test]
		public void Load()
		{
#if MONOGAME_WINDOWSDX
			Assert.IsTrue(EFUtils.CurrentPlatform == EFPlatform.MonoGameDirectX);
#elif MONOGAME_DESKTOPGL
			Assert.IsTrue(EFUtils.CurrentPlatform == EFPlatform.MonoGameOpenGL);
#else
			Assert.IsTrue(EFUtils.CurrentPlatform == EFPlatform.FNA);
#endif

			var efb = Res.ReadResourceAsBytes("DefaultEffect.efb");

			var mvEffect = new MultiVariantEffect(() => new MemoryStream(efb));

			string[] defines1 = new string[] { "", "LIGHTNING=1" };
			string[] defines2 = new string[] { "", "BONES=1", "BONES=2", "BONES=4" };

			for(var i = 0; i < defines1.Length; i++)
			{
				var define1 = defines1[i];
				for(var j = 0; j < defines2.Length; j++)
				{
					var define2 = defines2[j];

					var defines = new Dictionary<string, string>();
					if (!string.IsNullOrEmpty(define1))
					{
						var parts = define1.Split('=');
						defines[parts[0]] = parts[1];
					}

					if (!string.IsNullOrEmpty(define2))
					{
						var parts = define2.Split('=');
						defines[parts[0]] = parts[1];
					}

					var effect = mvEffect.GetEffect(TestsEnvironment.GraphicsDevice, defines);

					Assert.IsNotNull(effect);
					Assert.IsNotNull(effect.Parameters);
					Assert.IsTrue(effect.Parameters.Count > 0);
				}
			}
		}
	}
}
