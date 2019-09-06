using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EffectFarm
{
	public class EFParser
	{
		public const string EfbSignature = "EFB";
		public const int EfbVersion = 1;

		public static Dictionary<string, EFSource> LocateSources(Stream input)
		{
			var result = new Dictionary<string, EFSource>();
			using (var reader = new BinaryReader(input))
			{
				// Signature
				var sig = Encoding.UTF8.GetString(reader.ReadBytes(3));
				if (sig != EfbSignature)
				{
					throw new Exception("Wrong signature.");
				}

				var version = reader.ReadInt32();
				if (version != EfbVersion)
				{
					throw new Exception(string.Format("Wrong efb version. File version: {0}, supported version: {1}.",
						version, EfbVersion));
				}

				while (true)
				{
					try
					{
						var source = new EFSource();

						var data = reader.ReadString();
						source.Variant = EFVariant.FromString(data);

						var size = reader.ReadInt32();
						source.Offset = (int)input.Position;
						source.Size = size;

						// Skip data
						input.Seek(size, SeekOrigin.Current);

						result[data] = source;
					}
					catch (EndOfStreamException)
					{
						break;
					}
				}
			}

			return result;
		}
	}
}