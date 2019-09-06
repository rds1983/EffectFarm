using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace EffectFarm
{
	public class MultiVariantEffect
	{
		private readonly Func<Stream> _streamOpener;
		private readonly Dictionary<string, Effect> _effects = new Dictionary<string, Effect>();
		private readonly Dictionary<string, EFSource> _sources;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="streamOpener"></param>
		public MultiVariantEffect(Func<Stream> streamOpener)
		{
			if (streamOpener == null)
			{
				throw new ArgumentNullException("streamOpener");
			}

			_streamOpener = streamOpener;
			using (var stream = _streamOpener())
			{
				_sources = EFParser.LocateSources(stream);
			}
		}

		public Effect GetEffect(GraphicsDevice device, Dictionary<string, string> defines)
		{
			var key = EFVariant.BuildKey(EFUtils.CurrentPlatform, defines);
			Effect result;
			if (_effects.TryGetValue(key, out result))
			{
				return result;
			}

			EFSource source;
			if (!_sources.TryGetValue(key, out source))
			{
				return null;
			}

			var bytes = new byte[source.Size];
			using (var stream = _streamOpener())
			{
				stream.Read(bytes, source.Offset, source.Size);
			}

			result = new Effect(device, bytes);
			_effects[key] = result;

			return result;
		}
	}
}
