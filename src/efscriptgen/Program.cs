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

		static string BuildScript(string inputFolder, List<string> fxFiles, OutputType outputType)
		{
			var outputFolder = Path.Combine(inputFolder, OutputSubfolder(outputType));
			if (!Directory.Exists(outputFolder))
			{
				Directory.CreateDirectory(outputFolder);
			}

			var sb = new StringBuilder();
			foreach (var fx in fxFiles)
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
				}
				else
				{
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
						commandLine.Append($"mgfxc \"{fx}\" \"{outputFile}\"");
					}
					else
					{
						commandLine.Append($"fxc \"{fx}\" /Fo \"{outputFile}\"");
					}

					if (outputType != OutputType.FNA)
					{
						commandLine.Append(" /Profile:");
						switch (outputType)
						{
							case OutputType.MGDX11:
								commandLine.Append("DirectX_11");
								break;
							case OutputType.MGOGL:
								commandLine.Append("OpenGL");
								break;
						}

						if (!string.IsNullOrEmpty(variant))
						{
							commandLine.Append($" /Defines:{variant}");
						}
					}
					else
					{
						commandLine.Append(" /T:fx_2_0");

						if (!string.IsNullOrEmpty(variant))
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

			return sb.ToString();
		}

		static void Process(string[] args)
		{
			Log($"EffectFarm script generator {Version}.");

			if (args.Length < 1)
			{
				Log("Usage: efscriptgen <folder>");
				return;
			}

			var inputFolder = args[0];
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

			var script = BuildScript(inputFolder, fxFiles, OutputType.MGDX11);
			File.WriteAllText(Path.Combine(inputFolder, "compile_mgdx11.bat"), script);
			script = BuildScript(inputFolder, fxFiles, OutputType.MGOGL);
			File.WriteAllText(Path.Combine(inputFolder, "compile_mgogl.bat"), script);
			script = BuildScript(inputFolder, fxFiles, OutputType.FNA);
			File.WriteAllText(Path.Combine(inputFolder, "compile_fna.bat"), script);

			Log("The scripts generation was a success.");
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