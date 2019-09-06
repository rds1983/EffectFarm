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

		public static EFSource[] LocateSources(Stream input)
		{
			var result = new List<EFSource>();

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

						source.Variant.Platform = (EFPlatform)reader.ReadInt32();
						source.Variant.Defines = reader.ReadString();

						var size = reader.ReadInt32();
						source.Start = (int)input.Position;
						source.Size = size;

						// Skip data
						input.Seek(size, SeekOrigin.Current);

						result.Add(source);
					}
					catch(EndOfStreamException)
					{
						break;
					}
				}
			}

			return result.ToArray();
		}
	}
}
