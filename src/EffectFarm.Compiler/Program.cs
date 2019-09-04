using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EffectFarm
{
	class Program
	{
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

		static EFTarget[] ParseTargets(XDocument doc)
		{
			var targets = (from n in doc.Descendants("Targets") select n).FirstOrDefault();
			if (targets == null)
			{
				GenerateError("Could not find 'Targets' node");
			}

			var targetsList = new List<EFTarget>();
			foreach (var target in targets.Elements())
			{
				EFTarget targetValue;
				if (!Enum.TryParse(target.Name.ToString(), out targetValue))
				{
					GenerateError("Target '{0}' isn't supported", target.ToString());
				}

				targetsList.Add(targetValue);
			}

			return targetsList.ToArray();
		}

		static Dictionary<string, EFVariant> ParseVariants(XDocument doc)
		{
			var result = new Dictionary<string, EFVariant>();

			var variants = (from n in doc.Descendants("Variants") select n).FirstOrDefault();
			if (variants == null)
			{
				GenerateError("Could not find 'Variants' node");
			}

			foreach (var variantNode in variants.Elements())
			{
				var variant = new EFVariant();
				if (variantNode.Attribute("Name") == null)
				{
					GenerateError("Variant lacks essential 'Name' attribute");
				}

				variant.Name = variantNode.Attribute("Name").ToString();
				if (string.IsNullOrEmpty(variant.Name))
				{
					GenerateError("Variant name couldn't be empty");
				}

				if (!variantNode.HasElements)
				{
					GenerateError("Variant {0} lacks children", variant.Name);
				}

				var isBinaryAttr = variantNode.Attribute("IsBinary");
				if (isBinaryAttr != null && 
					isBinaryAttr.Value != null &&
					isBinaryAttr.Value.ToLower() == "true")
				{
					variant.IsBinary = true;
				}

				var variantsList = new List<string>();
				foreach(var value in variantNode.Elements())
				{
					if (value.Name != "Value")
					{
						GenerateError("Unrecognized node '{0}'", value.Name);
					}

					variantsList.Add(value.Value);
				}

				variant.Values = variantsList.ToArray();

				result[variant.Name] = variant;
			}

			return result;
		}

		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Log("Usage: efc <input_file> <config_file>");
				return;
			}

			// Parse config
			var config = new EFConfig();
			try
			{
				var doc = XDocument.Parse(File.ReadAllText(args[1]));

				config.Targets = ParseTargets(doc);
				config.Variants = ParseVariants(doc);
			}
			catch (Exception ex)
			{
				Log(ex.ToString());
				return;
			}

			try
			{
				var result = D3DCompiler.Compile(@"D:\Projects\Nursia\src\Nursia\EffectsSource\DefaultEffect.fx",
					new Dictionary<string, string>
					{
						["BONES"] = "2",
						["LIGHTNING"] = "1"
					},
					"fx_2_0");
			}
			catch (Exception ex)
			{
				Log(ex.ToString());
			}


			Console.ReadKey();
		}
	}
}
