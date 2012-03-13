/*
il2js Compiler - JavaScript VM for .NET
Copyright (C) 2012 Michael Kolarz

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/
#define TEST
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using MK.CodeDom.Compiler;
using MK.JavaScript;
using MK.JavaScript.CodeDom.Compiler;
using MK.JavaScript.Compiler;
using MK.JavaScript.Dom;
using System.Drawing;
using MK.JavaScript.Css;

public class Settings {
	public static string OutputDirectory;

	internal static string Guid;

	public static string ResourcesAspxFileName { get { return Guid; } }
	public static string HandlerAshxFileName { get { return Guid; } }
	public static string RuntimeJsFileName { get { return Guid; } }

	//public static string RuntimeJsFileName = Guid.NewGuid().ToString();
	//public static string ResourcesAspxFileName = "_";
	//public static string HandlerAshxFileName = "_";

	public static bool IgnoreFinally = false;
	public static bool InlineAll = true;

	public static bool DebugCompilation = false;

	public const bool IgnoreClause = false;
}

public partial class Program {




	public class Names {
		internal static Dictionary<string, string> names = new Dictionary<string, string> {
			//$ref$
			{"ref_container","a"},
			{"ref_index","b"},
			//$leaveStackElement$
			{"Handling_Type","a"},
			{"Handling_Argument","b"},
			//$execution$
			{"local","a"},
			{"position","b"},
			{"args","c"},
			{"il","d"},
			{"tryStack","e"},
			{"handlingStack","f"},
			//przerzucone jako globalne dla przyspieszenia i kompresji
			//{"eatInt","e"},
			//{"eatUInt","f"},
			//{"eatChar","g"},
			//$call$:
			{"callStack","e"},
			{"stack","f"},
			{"pop","g"},
			{"pops","h"},
			{"push","i"},
			{"operator","j"},
			{"br2","k"},
			{"handleException","l"},

			{"path","A"},
			{"guid","B"},
			{"method","C"},
			{"methodLength","D"},
			{"field","E"},
			{"ctor","F"},
			{"string","G"},
			{"code","H"},
			{"loaded","I"},
			{"cctor_executed","J"},
			{"module","K"},
			{"apply","L"},
			{"getNativeMethod","M"},
			{"handler","N"},
			{"call","O"},
			{"execution","P"},
			{"eatInt","Q"},
			{"eatUInt","R"},
			{"eatChar","S"},
			{"ref","T"},
			{"set","U"},
			{"init","V"},

		};
#if OBFUSCATE
		private static char Current = 'W';
#endif
		public static string GetName(string originalName) {
#if OBFUSCATE

			//return names[originalName];

			string ret;
			if (!names.TryGetValue(originalName, out ret)) {
				ret = Current.ToString();
				if (Current++ > 'Z') throw new Exception("GetNames");
				names[originalName] = ret;

				//Console.WriteLine("{\"" + originalName + "\",\"" + ret + "\"},");

			}
			return ret;
#else
			return "$" + originalName + "$";
#endif
		}
		public static string PathValue;
		public static string _move_ { get { return GetName("move"); } }
		public static string _get_ { get { return GetName("get"); } }
		public static string _init_ { get { return GetName("init"); } }
		public static string _resources_ { get { return GetName("resources"); } }
		public static string _path_ { get { return GetName("path"); } }
	}


	static HashSet<string> Runtime_jsSymbols = new HashSet<string> {
		//TO MUSI BYC ZBIOR PREFIKSOWY
//#if TEST
//    "debug",
//#endif
		"guid",
		"release",
		"IE",
		//"developer",
	};
	static void CreateRuntime_js() {
		File.WriteAllText(Settings.OutputDirectory + Path.DirectorySeparatorChar + Settings.RuntimeJsFileName + ".js",
			"//http://smp.if.uj.edu.pl/il2js framework (c) mkol@smp.if.uj.edu.pl\n" + PackCode(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "il2js.js"), false));
	}

	public static string PackCode(string value, bool forceLinePack) {
		#region PreProcessor #if .. #else .. #endif
		value = Regex.Replace(
			value,
			@"^\s*//#if\s+([a-zA-Z0-9]+)(.*?)((^\s*//#else)(.*?))?//\s*#endif",
			m => {
				if (Runtime_jsSymbols.Contains(m.Groups[1].Value)) {
					return m.Groups[2].Value;
				} else {
					if (m.Groups[3].Success) {
						return m.Groups[5].Value;
					} else {
						return "";
					}
				}
			},
			RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline
		);
		#endregion
		#region Replacements $...
		value = Regex.Replace(
			value,
			@"\$([a-zA-Z_][a-zA-Z0-9_]*)(\.([a-zA-Z_][a-zA-Z0-9_]*))?\$",
			m => {
				switch (m.Groups[1].Value) {
					case "resourcesFileName": return Settings.ResourcesAspxFileName == null ? "$resourcesFileName$" : Settings.ResourcesAspxFileName;
					case "handlerFileName": return Settings.HandlerAshxFileName == null ? "$handlerFileName$" : Settings.HandlerAshxFileName;
					case "guidValue": return Settings.Guid == null ? "$guidValue$" : MK.Convert.ToJSString(Settings.Guid);
					case "OpCode": return "\"" + typeof(MK.CodeDom.Compiler.MethodCompiler.JSOpCode).GetField(m.Groups[3].Value).GetValue(null) + "\""
#if !OBFUSCATE
 + "/*" + m.Groups[3].Value + "*/"
#endif
;
					case "HandlingType":
						switch (m.Groups[3].Value) {
							case "Leave": return "0";
							case "Throw": return "1";
							default: throw new Exception("HandlingType." + m.Groups[3].Value);
						}
					case "debug": return m.Groups[0].Value;
					case "eatToken": return Names.GetName("eatChar");
					//case "eatUInt": return Names.GetName("eatInt");
					case "native": return Names.GetName("string");
					case "pathValue": return Names.PathValue ?? "$pathValue$";
					default:
						return Names.GetName(m.Groups[1].Value);
				}

			},
			RegexOptions.Compiled
		);
		#endregion
		if (forceLinePack
#if OBFUSCATE
 || true
#endif
)
			value = string.Join("", (from l in value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
															 let line = l.Trim()
															 where !line.StartsWith("//")
															 select line).ToArray());
#if OBFUSCATE
		value = value
			.Replace(";}", "}")
			.Replace("case \"", "case\"")
		;
#endif
		return value;
	}





	private static XDocument doc;


	//nawet nie mysl o pozbyciu sie tego i wsadzeniu do dllki bo cie zjedza zmienne statyczne...
	static void Main0(string[] args) {

		Settings.Guid = global::System.Guid.NewGuid().ToString();

#if !TEST
		try {
#endif


		Settings.OutputDirectory = args[2];



		if (Directory.Exists(Settings.OutputDirectory)) {
			try {
				Directory.Delete(Settings.OutputDirectory, true);
			} catch (IOException) { }
		}
		while (!Directory.Exists(Settings.OutputDirectory)) {
			try {
				Directory.CreateDirectory(Settings.OutputDirectory);
			} catch (IOException) { }
		}

		var assembly = Assembly.LoadFrom(args[0]);
		string pathValue = args[1];
		Names.PathValue = MK.Convert.ToJSString(pathValue);
		LogPath = Settings.OutputDirectory + @"\log.xml";
		var inputDirectory = Path.GetDirectoryName(args[0]);

		NativesManager.Instance = new NativesManager(inputDirectory + @"\Mapping.xml");

		foreach (var option in args[3].Split('&')) {
			var kv = option.Split('=');
			switch (kv[0]) {
				case "IFC": Settings.IgnoreFinally = bool.Parse(kv[1]); break;
				case "IA": Settings.InlineAll = bool.Parse(kv[1]); break;
			}
		}


		CreateRuntime_js();


		XmlLog = XmlWriter.Create(LogPath);
		XmlLog.WriteStartElement("compilation");

		if (Settings.DebugCompilation) {
			XmlLog.WriteAttributeString("dll", args[0]);
			XmlLog.WriteAttributeString("guid", Settings.RuntimeJsFileName);

		}
		var compilers =/*
			new PageFrameCompiler(assembly.GetType("MK.JavaScript.Tests",true), new WindowAttribute("~/_test/dump.aspx"))
			/*/
			(
				from type in assembly.GetTypes()
				where type.IsSubclassOf(typeof(Window))
				let attrs = type.GetCustomAttributes(typeof(WindowAttribute), false)
				where attrs.Length == 1
				select new PageFrameCompiler(type, (WindowAttribute)attrs[0])
			).ToArray()
			//*/
			;
		PageFrameCompiler.CompileAll(compilers);

		CssSupport.Compute(compilers);

		#region todo.xml

		var root = new XElement("generate");

		doc = new XDocument(root);

		foreach (var compiler in compilers) {


			var sb = new StringBuilder();
			sb.Append("<!--powered by il2js framework (http://smp.if.uj.edu.pl/~mkol/il2js)-->");
			foreach (var fileName in
				from file in CssSupport.CssFiles
				where file.Pages.Contains(compiler)
				select file.CssFileName
			) {
				sb.Append(@"<link type=""text/css"" rel=""stylesheet""  href=""" + pathValue + fileName + @".css"" />");
			}
			if (compiler.IsRuntimePage)
				sb.Append(@"<script type=""text/javascript"" src=""" + pathValue + Settings.RuntimeJsFileName + @".js""></script>");
			sb.Append(@"<script type=""text/javascript"" src=""" + pathValue + compiler.JsFileName + @".js""></script>");
			sb.Append(@"<!--/il2js-->");

			XElement page = new XElement("page",
				new XAttribute("path", compiler.WindowAttribute.Path),
				new XAttribute("script", sb.ToString())
			);

			if (compiler.WindowAttribute.Render) {
				page.Add(new XAttribute("render", compiler.WindowType.FullName));
			}

			root.Add(page);


		}
		root.Add(Resources.CreateConfigurationElement(Settings.ResourcesAspxFileName, GetCultures(inputDirectory)));
		foreach (var element in CssSupport.CreateConfigurationElements()) {
			root.Add(element);
		}

		doc.Save(Settings.OutputDirectory + "\\generate.xml");

		#endregion
		XmlLog.WriteEndElement();
		XmlLog.Close();


		ServerManager.Dump(Settings.OutputDirectory);

#if !TEST
		} catch (CompilationException e) {
			File.WriteAllText(Settings.OutputDirectory + Path.DirectorySeparatorChar + "error.txt", e.Message);
		} catch (Exception e) {
			File.WriteAllText(Settings.OutputDirectory + Path.DirectorySeparatorChar + "error.txt", "Internal compiler error. Sending input directory to mkol@smp.if.uj.edu.pl will help to identify and to fix this problem in new version of compiler.");
			//File.WriteAllText(Settings.OutputDirectory + Path.DirectorySeparatorChar + "error.txt", e.StackTrace);
		}
#endif
	}
	static IEnumerable<CultureInfo> GetCultures(string inputDirectory) {
		foreach (var name in Directory.GetDirectories(inputDirectory).Select(a => Path.GetFileName(a))) {
			CultureInfo culture;
			try {
				culture = CultureInfo.GetCultureInfo(name);
			} catch (ArgumentException) {
				continue;
			}
			yield return culture;
		}
	}

	#region IL
	private static void GenerateOpCodeValueEnum() {
		StringBuilder output = new StringBuilder();
		foreach (var fieldInfo in typeof(OpCodes).GetFields()) {
			output.Append(fieldInfo.Name).Append('=').Append(((OpCode)fieldInfo.GetValue(null)).Value).Append(',');
		}
		File.WriteAllText("output", output.ToString());
	}
	private static void GenerateOpCodeValueEnumSwitch() {
		StringBuilder output = new StringBuilder();
		foreach (var fieldInfo in typeof(OpCodes).GetFields()) {
			output.Append("case OpCodeValue.").Append(fieldInfo.Name).Append(":\n");
		}
		File.WriteAllText("output", output.ToString());
	}
	private static void GenerateOpCodes() {
		StringBuilder output = new StringBuilder();
		var fields = typeof(OpCodes).GetFields();
		var i = 0;
		var index = 0;
		var nop = fields[i].Name;
		while (true) {
			var opCode = (OpCode)fields[i].GetValue(null);
			var opCodeName = fields[i].Name;
			if (opCode.Value < 0) {
				break;
			}
			while (index < opCode.Value) {
				output.Append("OpCodes.").Append(nop).Append(",\n");
				++index;
			}
			output.Append("//").Append(opCode.Value).Append("\nOpCodes.").Append(opCodeName).Append(",\n");
			++i;
			++index;
		}
		output.Append("\n--------------------------------------------\n");
		index = 0;
		while (i < fields.Length) {
			var opCode = (OpCode)fields[i].GetValue(null);
			var opCodeName = fields[i].Name;
			while (index < 512 + opCode.Value) {
				output.Append("OpCodes.").Append(nop).Append(",\n");
				++index;
			}
			output.Append("//").Append(opCode.Value).Append("\nOpCodes.").Append(opCodeName).Append(",\n");
			++i;
			++index;
		}
		output.Append('\n');
		File.WriteAllText("output", output.ToString());
	}
	#endregion


	public static XmlWriter XmlLog;
	private static string LogPath;
}
