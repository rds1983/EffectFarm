using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace EffectFarm
{
	enum OutputType
	{
		MGDX11,
		MGOGL,
		FNA
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
				Log("Usage: efscriptgen mgdx11|mgogl|fna <folder>");
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
				case "fna":
					outputType = OutputType.FNA;
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

			if (!Directory.Exists(outputFolder))
			{
				Directory.CreateDirectory(outputFolder);
			}

			var sb = new StringBuilder();
			foreach(var fx in fxFiles)
			{
				var xmlFile = Path.ChangeExtension(fx, "xml");
				var variants = new List<string>();
				if (File.Exists(xmlFile))
				{
					var xDoc = XDocument.Load(xmlFile);
					foreach (var defineTag in xDoc.Root.Elements())
					{
						var defineValue = defineTag.Attribute("Value").Value;
						variants.Add(defineValue);
					}
				} else {
					variants.Add(string.Empty);
				}

				foreach (var variant in variants)
				{
					var postFix = string.Empty;
					if (!string.IsNullOrEmpty(variant))
					{
						var defines = (from d in variant.Split(";") orderby d select d.Trim()).ToArray();
						foreach (var def in defines)
						{
							var defineParts = (from d in def.Split("=") select d.Trim()).ToArray();
							foreach (var part in defineParts)
							{
								postFix += "_";
								postFix += part;
							}
						}
					}

					var outputFile = Path.GetFileNameWithoutExtension(fx) + postFix;
					var outputExt = outputType != OutputType.FNA ? "mgfxo" : "fxc";
					outputFile = Path.Combine(outputFolder, Path.ChangeExtension(outputFile, outputExt));

					var commandLine = new StringBuilder();

					if (outputType != OutputType.FNA)
					{
						commandLine.Append("mgfxc /profile:");
						switch (outputType)
						{
							case OutputType.MGDX11:
								commandLine.Append("DirectX_11");
								break;
							case OutputType.MGOGL:
								commandLine.Append("OpenGL");
								break;
						}
						commandLine.Append($" \"{fx}\" \"{outputFile}\"");
					}
					else
					{
						commandLine.Append($"fxc /T:fx_2_0 /Fo \"{outputFile}\" \"{fx}\"");
					}

					if (!string.IsNullOrEmpty(variant))
					{
						if (outputType != OutputType.FNA)
						{
							commandLine.Append($" /defines:{variant}");
						}
						else
						{
							var defines = (from d in variant.Split(";") orderby d select d.Trim()).ToArray();
							foreach (var def in defines)
							{
								var defineParts = (from d in def.Split("=") select d.Trim()).ToArray();
								if (defineParts.Length == 1)
								{
									commandLine.Append($" /D {defineParts[0]}=1");
								}
								else
								{
									commandLine.Append($" /D {defineParts[0]}={defineParts[1]}");
								}
							}
						}
					}

					sb.AppendLine(commandLine.ToString());
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