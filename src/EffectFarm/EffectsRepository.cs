using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace EffectFarm
{
	public class EffectsRepository
	{
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
#if MONOGAME
				var fileName = keyString + ".mgfxo";
#else
				var fileName = keyString + ".fxc";
#endif

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