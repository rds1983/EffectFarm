using System.Collections.Generic;
using System.IO;

namespace EffectFarm
{
	class EFParser
	{
		public static EFSource[] LocateSources(Stream input)
		{
			var result = new List<EFSource>();

			using (var reader = new BinaryReader(input))
			{
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
