using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

#if MONOGAME
using MonoGame.Framework.Utilities;
#endif

namespace EffectFarm
{
	public class EffectsRepository
	{
#if MONOGAME
		private static string _effectExtension = "mgfxo";
#else
		private static string _effectExtension = "fxo";
#endif

		public static string EffectExtension
		{
			get => _effectExtension;
			set => _effectExtension = value;
		}

		public static string EffectsSubfolder
		{
			get
			{
#if MONOGAME
				return PlatformInfo.GraphicsBackend == GraphicsBackend.OpenGL ? "MonoGameOGL" : "MonoGameDX11";
#else
				return "FNA";
#endif
			}
		}


		private Func<string, Stream> _assetOpener;
		private readonly Dictionary<string, Effect> _effectsCache = new Dictionary<string, Effect>();

		public EffectsRepository(Func<string, Stream> assetOpener)
		{
			_assetOpener = assetOpener ?? throw new ArgumentNullException(nameof(assetOpener));
		}

		public static EffectsRepository CreateFromFolder(string folder)
		{
			if (!Directory.Exists(folder))
			{
				throw new ArgumentException($"Couldn't find folder {folder}.");
			}

			return new EffectsRepository(s => File.OpenRead(Path.Combine(folder, s)));
		}

		public Effect Get(GraphicsDevice graphicsDevice, string name, Dictionary<string, string> defines = null)
		{
			var key = new StringBuilder();
			key.Append(name);

			if (defines != null && defines.Count > 0)
			{
				var keys = (from def in defines.Keys orderby def select def).ToArray();
				foreach (var k in keys)
				{
					key.Append("_");
					key.Append(k);
					var value = defines[k];
					if (value != "1")
					{
						key.Append("_");
						key.Append(value);
					}
				}
			}

			var keyString = key.ToString();
			Effect result;
			if (!_effectsCache.TryGetValue(keyString, out result))
			{
				var fileName = keyString + "." + EffectExtension;

				var ms = new MemoryStream();
				using (var stream = _assetOpener(fileName))
				{
					stream.CopyTo(ms);
				}

				result = new Effect(graphicsDevice, ms.ToArray());
				_effectsCache[keyString] = result;
			}

			return result;
		}
	}
}