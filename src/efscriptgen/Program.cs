using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EffectFarm
{
	enum OutputType
	{
		MGDX11,
		MGOGL
	}

	class Program
	{
		public static string Version
		{
			get
			{
				var assembly = typeof(Program).Assembly;
				var name = new AssemblyName(assembly.FullName);

				return name.Version.ToString();
			}
		}

		static void Log(string message)
		{
			Console.WriteLine(message);
		}

		static string OutputSubfolder(OutputType outputType)
		{
			switch (outputType)
			{
				case OutputType.MGDX11:
					return "MonogameDX11";
				case OutputType.MGOGL:
					return "MonoGameOGL";
			}

			return "FNA";
		}

		static void Process(string[] args)
		{
			Log($"EffectFarm script generator {Version}.");

			if (args.Length < 2)
			{
				Log("Usage: efscriptgen mgdx11|mgogl <folder>");
				return;
			}

			var outputTypeArg = args[0];
			OutputType outputType;
			switch(outputTypeArg)
			{
				case "mgdx11":
					outputType = OutputType.MGDX11;
					break;
				case "mgogl":
					outputType = OutputType.MGOGL;
					break;
				default:
					Log($"First argument should be either 'mgfx11' or 'mgogl'. Actual value passed '{outputTypeArg}'.");
					return;
			}

			var inputFolder = args[1];
			if (!Directory.Exists(inputFolder))
			{
				Log($"Could not find '{inputFolder}'.");
				return;
			}

			var fxFiles = Directory.EnumerateFiles(inputFolder, "*.fx").ToList();
			if (fxFiles.Count == 0)
			{
				Log($"No '.fx' found at folder '{inputFolder}'.");
				return;
			}

			var outputFolder = Path.Combine(inputFolder, OutputSubfolder(outputType));
			var sb = new StringBuilder();
			foreach(var fx in fxFiles)
			{
				var outputFile = Path.Combine(outputFolder, Path.ChangeExtension(Path.GetFileName(fx), "mgfxo"));

				switch (outputType)
				{
					case OutputType.MGDX11:
						sb.AppendLine($"mgfxc \"{fx}\" \"{outputFile}\" /profile:DirectX_11");
						break;
					case OutputType.MGOGL:
						sb.AppendLine($"mgfxc \"{fx}\" \"{outputFile}\" /profile:OpenGL");
						break;
				}

			}

			var scriptFile = Path.Combine(inputFolder, "compile.bat");

			File.WriteAllText(scriptFile, sb.ToString());

			Log($"Generated script '{scriptFile}'.");
		}

		static void Main(string[] args)
		{
			try
			{
				Process(args);
			}
			catch (Exception ex)
			{
				Log(ex.ToString());
			}
		}
	}
}