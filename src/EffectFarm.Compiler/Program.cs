using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TwoMGFX;

namespace EffectFarm
{
	class Program
	{
		class IncludeFX : Include
		{
			private readonly string _folder;

			public IncludeFX(string folder)
			{
				_folder = folder;
			}

			public IDisposable Shadow { get; set; }

			public void Close(Stream stream)
			{
				stream.Close();
				stream.Dispose();
			}

			public void Dispose()
			{
			}

			public Stream Open(IncludeType type, string fileName, Stream parentStream)
			{
				return new FileStream(Path.Combine(_folder, fileName), FileMode.Open);
			}
		}

		class ConsoleEffectCompilerOutput : IEffectCompilerOutput
		{
			public void WriteWarning(string file, int line, int column, string message)
			{
				Log("Warning: {0}({1},{2}): {3}", file, line, column, message);
			}

			public void WriteError(string file, int line, int column, string message)
			{
				GenerateError("Error: {0}({1},{2}): {3}", file, line, column, message);
			}
		}

		class ProcessorLogger : ContentBuildLogger
		{
			public override void LogImportantMessage(string message, params object[] messageArgs)
			{
				Log(message, messageArgs);
			}

			public override void LogMessage(string message, params object[] messageArgs)
			{
				Log(message, messageArgs);
			}

			public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
			{
				Log(message, messageArgs);
			}
		}

		private static ProcessorLogger _processorLogger = new ProcessorLogger();
		private static string _intermediateDirectory;
		private static string _outputDirectory;

		class ImporterContext : ContentImporterContext
		{
			private readonly List<string> _dependencies = new List<string>();

			public override string IntermediateDirectory => _intermediateDirectory;

			public override ContentBuildLogger Logger => _processorLogger;

			public override string OutputDirectory => _outputDirectory;

			public override void AddDependency(string filename)
			{
				_dependencies.Add(filename);
			}
		}

		class ProcessorContext : ContentProcessorContext
		{
			public TargetPlatform _targetPlatform;
			public string _outputFile;

			private readonly List<string> _dependencies = new List<string>();

			public override string BuildConfiguration => throw new NotImplementedException();

			public override string IntermediateDirectory => _intermediateDirectory;

			public override ContentBuildLogger Logger => _processorLogger;

			public override ContentIdentity SourceIdentity => throw new NotImplementedException();

			public override string OutputDirectory => _outputDirectory;

			public override string OutputFilename => _outputFile;

			public override OpaqueDataDictionary Parameters => throw new NotImplementedException();

			public override TargetPlatform TargetPlatform => _targetPlatform;

			public override GraphicsProfile TargetProfile => throw new NotImplementedException();

			public override void AddDependency(string filename)
			{
				_dependencies.Add(filename);
			}

			public override void AddOutputFile(string filename)
			{
				throw new NotImplementedException();
			}

			public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
			{
				throw new NotImplementedException();
			}

			public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
			{
				throw new NotImplementedException();
			}

			public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
			{
				throw new NotImplementedException();
			}
		}

		private static string FormatMessage(string message, params object[] args)
		{
			string str;
			try
			{
				if (args != null && args.Length > 0)
				{
					str = string.Format(message, args);
				}
				else
				{
					str = message;
				}
			}
			catch (FormatException)
			{
				str = message;
			}

			return str;
		}

		static void Log(string message, params object[] args)
		{
			Console.WriteLine(FormatMessage(message, args));
		}

		static void GenerateError(string message, params object[] args)
		{
			throw new Exception(FormatMessage(message, args));
		}

		static EFPlatform[] ParseTargets(XDocument doc)
		{
			var targets = (from n in doc.Descendants("Targets") select n).FirstOrDefault();
			if (targets == null)
			{
				GenerateError("Could not find 'Targets' node");
			}

			var targetsList = new List<EFPlatform>();
			foreach (var target in targets.Elements())
			{
				EFPlatform targetValue;
				if (!Enum.TryParse(target.Name.ToString(), out targetValue))
				{
					GenerateError("Target '{0}' isn't supported", target.ToString());
				}

				targetsList.Add(targetValue);
			}

			return targetsList.ToArray();
		}

		static Entry ParseEntry(XElement root)
		{
			var result = new Entry();
			var defines = new List<Define>();
			var children = new List<Entry>();
			foreach (var element in root.Elements())
			{
				if (element.Name == "Define")
				{
					// Define
					var define = new Define();
					if (element.Attribute("Name") != null)
					{
						define.Name = element.Attribute("Name").Value;
						define.Value = "1";
						if (element.Attribute("Value") != null)
						{
							define.Value = element.Attribute("Value").Value;
						}
					}

					defines.Add(define);
				}
				else if (element.Name == "Entry")
				{
					children.Add(ParseEntry(element));
				}
				else
				{
					GenerateError("Unknown node {0}", element.Name);
				}
			}

			result.Defines = defines.ToArray();
			result.Children = children.ToArray();

			return result;
		}

		static ShaderMacro[] ToMacroses(EFVariant variant)
		{
			var result = new List<ShaderMacro>();

			if (!string.IsNullOrEmpty(variant.Defines))
			{
				var parts = variant.Defines.Split(';');
				foreach (var part in parts)
				{
					var parts2 = part.Split('=');

					result.Add(new ShaderMacro(parts2[0], parts2[1]));
				}
			}

			return result.ToArray();
		}

		static void WriteOutput(BinaryWriter writer, EFVariant variant, byte[] data)
		{
			writer.Write((int)variant.Platform);
			writer.Write(variant.Defines);
			writer.Write(data.Length);
			writer.Write(data);
		}

		static EFVariant[] Substract(EFVariant[] variants, string path)
		{
			var variantsList = variants.ToList();

			try
			{
				var sources = new EFSource[0];
				using (var input = File.OpenRead(path))
				{
					sources = EFParser.LocateSources(input);
					foreach(var source in sources)
					{
						foreach(var variant in variants)
						{
							if (source.Variant == variant)
							{
								variantsList.Remove(variant);
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}

			return variantsList.ToArray();
		}

		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Log("Usage: efc <input_file> <config_file>");
				return;
			}

			try
			{
				var inputFile = args[0];
				if (!File.Exists(inputFile))
				{
					Log("Could not find '0'", inputFile);
					return;
				}

				var inputDt = File.GetLastWriteTime(inputFile);

				var configFile = args[1];
				if (!File.Exists(inputFile))
				{
					Log("Could not find '0'", inputFile);
					return;
				}
				var doc = XDocument.Parse(File.ReadAllText(configFile));

				// Parse config
				var config = new Config
				{
					Targets = ParseTargets(doc)
				};
				if (config.Targets.Length == 0)
				{
					GenerateError("No target platforms");
				}

				var rootEntry = (from n in doc.Descendants("RootEntry") select n).FirstOrDefault();
				if (rootEntry == null)
				{
					GenerateError("Could not find 'RootEntry' node");
				}

				config.Root = ParseEntry(rootEntry);

				var variants = config.BuildVariants().ToArray();

				var outputFile = Path.ChangeExtension(inputFile, "efb");
				if (File.Exists(outputFile) && File.GetLastWriteTime(outputFile) > inputDt)
				{
					var resultVariants = Substract(variants, outputFile);
					if (resultVariants.Length == 0)
					{
						Log("{0} is up to date", Path.GetFileName(outputFile));
						return;
					}
				}

				var workingFolder = Path.GetDirectoryName(inputFile);
				var includeFx = new IncludeFX(workingFolder);
				var consoseEffectCompilerOutput = new ConsoleEffectCompilerOutput();

				var importerContext = new ImporterContext();
				var processorContext = new ProcessorContext();

				var effectImporter = new EffectImporter();
				var effectProcesor = new EffectProcessor();

				using (var stream = File.OpenWrite(outputFile))
				using (var writer = new BinaryWriter(stream))
				{
					writer.Write(Encoding.UTF8.GetBytes(EFParser.EfbSignature));
					writer.Write(EFParser.EfbVersion);

					foreach (var variant in variants)
					{
						Log(variant.ToString());

						switch (variant.Platform)
						{
							case EFPlatform.MonoGameDirectX:
							case EFPlatform.MonoGameOpenGL:
							{
								var content = effectImporter.Import(inputFile, importerContext);

								processorContext._targetPlatform = variant.Platform == EFPlatform.MonoGameDirectX ?
									TargetPlatform.Windows : TargetPlatform.DesktopGL;
								effectProcesor.Defines = variant.Defines;

								var result = effectProcesor.Process(content, processorContext);

								WriteOutput(writer, variant, result.GetEffectCode());
							}
							break;
							case EFPlatform.FNA:
							{
								var result = ShaderBytecode.CompileFromFile(inputFile,
									"fx_2_0",
									ShaderFlags.OptimizationLevel3,
									EffectFlags.None,
									ToMacroses(variant),
									includeFx);

								if (result.ResultCode != Result.Ok)
								{
									GenerateError(result.Message);
								}

								WriteOutput(writer, variant, result.Bytecode);
							}
							break;
						}
					}
				}

				Log("Compilation to {0} was succesful", Path.GetFileName(outputFile));
			}
			catch (Exception ex)
			{
				Log(ex.ToString());
			}
		}
	}
}