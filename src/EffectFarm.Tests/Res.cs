using System;
using System.IO;

namespace EffectFarm.Tests
{
	/// <summary>
	/// Resource utility
	/// </summary>
	public static class Res
	{
		/// <summary>
		/// Open assembly resource stream by relative name
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static Stream OpenResourceStream(string path)
		{
			// Once you figure out the name, pass it in as the argument here.
			var assembly = typeof(Res).Assembly;
			var assemblyName = assembly.GetName().Name;
			var stream = assembly.GetManifestResourceStream($"{assemblyName}.Resources.{path}");
			if (stream == null)
			{
				throw new Exception($"Could not find resource at path '{path}'");
			}

			return stream;
		}

		/// <summary>
		/// Reads assembly resource as byte array by relative name
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static byte[] ReadResourceAsBytes(string path)
		{
			var ms = new MemoryStream();
			using (var input = OpenResourceStream(path))
			{
				input.CopyTo(ms);

				return ms.ToArray();
			}
		}

		/// <summary>
		/// Reads assembly resource as string by relative name
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string ReadResourceAsString(string path)
		{
			string result;
			using (var input = OpenResourceStream(path))
			{
				using (var textReader = new StreamReader(input))
				{
					result = textReader.ReadToEnd();
				}
			}

			return result;
		}
	}
}
