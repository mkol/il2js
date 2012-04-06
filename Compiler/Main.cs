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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Globalization;


public partial class Program {

	static void Compile(string inputDll, string outputVirtualPath, string webAppDirectory, string outputDirectory, bool deleteOutputDirectory, bool ignoreFinallyClauses, bool inline, string prototypeJs) {

		var inputDirectory = Path.GetDirectoryName(inputDll);

		if (deleteOutputDirectory) {
			try {
				Directory.Delete(outputDirectory, true);
			} catch { }
		}

		Directory.CreateDirectory(outputDirectory);


		//dll mapping output culture resources
		Main0(new[] { inputDll, outputVirtualPath, outputDirectory, "IFC=" + ignoreFinallyClauses + "&IA=" + inline });

		var tmp = outputDirectory + Path.DirectorySeparatorChar + "error.txt";

		if (File.Exists(tmp)) {
			throw new Exception(File.ReadAllText(tmp));
		}

		//tmp = outputDirectory + Path.DirectorySeparatorChar + "generate.xml";

		#region process todo.xml

		//var doc = XDocument.Load(tmp);

		foreach (var node in doc.Root.Elements()) {
			switch (node.Name.LocalName) {
				case "page": {
						var pagePath = webAppDirectory + Path.DirectorySeparatorChar + node.Attribute("path").Value
							.Substring(1)//skip ~
							.Replace('/', Path.DirectorySeparatorChar);
						var stringToAdd = node.Attribute("script").Value;

						var render = node.Attribute("render");
						if (render != null) {
							MK.JavaScript.Compiler.HtmlWriter.Write(pagePath, prototypeJs, Assembly.LoadFrom(inputDll).GetType(render.Value));
						}

						bool replaced = false;
						File.WriteAllText(pagePath, new Regex(
							Regex.Escape("<!--powered by il2js framework (http://smp.if.uj.edu.pl/~mkol/il2js)-->") + ".*" +
							Regex.Escape("<!--/il2js-->"), RegexOptions.Compiled | RegexOptions.Singleline
							).Replace(File.ReadAllText(pagePath), m => {
								replaced = true;
								return stringToAdd;
							}));
						if (!replaced) {
							File.WriteAllText(pagePath, File.ReadAllText(pagePath).Replace("</head>", stringToAdd + "</head>"));
						}
					}
					break;
				case "resource": {
						var writer = new MK.JavaScript.Compiler.ResourceWriter(node.Attribute("cultures").Value.Split(',').Select(n => CultureInfo.GetCultureInfo(n)));
						foreach (var type in node.Elements()) {
							writer.AddResource(
								Assembly.LoadFrom(inputDirectory + Path.DirectorySeparatorChar + type.Attribute("assembly").Value + ".dll").GetType(type.Attribute("name").Value),
								type.Attribute("members").Value.Split(',')
							);
						}
						writer.Write(outputDirectory, node.Attribute("name").Value + ".aspx");
					} break;
				case "css":
					MK.JavaScript.Compiler.CssWriter.Write(
						outputDirectory + Path.DirectorySeparatorChar + node.Attribute("name").Value + ".css",
						node.Elements().Select(type => Assembly.LoadFrom(inputDirectory + Path.DirectorySeparatorChar + type.Attribute("assembly").Value + ".dll").GetType(type.Attribute("name").Value))
					);
					break;
			}
		}
		#endregion


		//File.Delete(tmp);
	}


	static void Main(string[] args) {
		if (args.Length == 0) {
			Console.WriteLine(
@"
USAGE: il2js <Input Dll Path> [options...]

                 -OUTPUT OPTIONS-

If following options are not provided, it is assumed that
  <Input Dll Path>
 is in form of
  <WebApp Parent Directory>\<WebApp Name>\Bin\<Input Dll File>.

/webdir=<path>   Directory containing WebApp.
                 DEFAULT=<WebApp Parent Directory>\<WebApp Name>

/output=<path>   Phisical path for generated files.
                 DEFAULT=<WebApp Parent Directory>\<WebApp Name>\_

/url=<path>      URL virtual path for generated files.
                 DEFAULT=/<WebApp Name>/_/

/clean           Output directory will be deleted.

                 -COMPILATION OPTIONS-

/ignorefinally   Compiler will ignore finally clauses.

/dontinline      Compiler will not inline methods not explicit marked
                 by Inline attribute.
                 (By default, all methods are inlined, if possible.)
           

                 -OTHER OPTIONS-

/prototypejs=<path>   Path to prototype.js file
                      (required when using [Window(Render=true,...)]).
                      DEFAULT=prototype.js														
"


///vs													Indicates if compiler is called from Visual Studio
				//                            as Post-build event command like
				//                            <path to il2js.exe> $(TargetPath) --VS
				//                            If present, error message will be opened in default
				//                            text editor rather then shown on console.
);
			return;
		}
		var il2js_js=AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "il2js.js";
		if(!File.Exists(il2js_js)){
			Console.Write("generating " + il2js_js+ " ... ");
			File.WriteAllText(il2js_js, PackCode(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Runtime.js"), false));
			Console.WriteLine("done");
		}

		switch (args[0]) {
			default: {
					string inputDll = args[0];
					string outputVirtualPath = "";
					string webAppDirectory = "";
					string outputDirectory = "";
					bool deleteOutputDirectory = false;
					bool ignoreFinallyClauses = false;
					bool inline = true;
					const bool vs = false;
					string prototypeJs = "prototype.js";

					try {
						foreach (var option in args.Skip(1)) {
							var kv = option.Split('=');
							switch (kv[0]) {
								case "/url": outputVirtualPath = kv[1]; break;
								case "/webdir": webAppDirectory = kv[1]; break;
								case "/output": outputDirectory = kv[1]; break;
								case "/prototypejs": prototypeJs = kv[1]; break;
								case "/clean": deleteOutputDirectory = true; break;
								case "/ignorefinally": ignoreFinallyClauses = true; break;
								//case "/vs": vs = true; break;
								case "/dontinline": inline = false; break;
								case "/define": Program.Runtime_jsSymbols.Add(kv[1]); break;
								case "/debug":
									Settings.DebugCompilation = true;
									Program.Runtime_jsSymbols.Add("debug");
									break;

								default: throw new ArgumentException("Option Error: unknown option " + kv[0]);
							}
						}
						if (webAppDirectory == "")
							webAppDirectory = Path.GetDirectoryName(Path.GetDirectoryName(inputDll));
						if (outputVirtualPath == "")
							outputVirtualPath = "/" + Path.GetFileName(webAppDirectory) + "/_/";
						if (outputDirectory == "")
							outputDirectory = webAppDirectory + Path.DirectorySeparatorChar + "_";

						Compile(inputDll, outputVirtualPath, webAppDirectory, outputDirectory, deleteOutputDirectory, ignoreFinallyClauses, inline, prototypeJs);

					} catch (Exception e) {
						if (vs) {
							var tmp = "IL2JSErrorLog.txt";
							File.WriteAllText(tmp, e.Message);
							Process.Start(tmp).WaitForExit();
							File.Delete(tmp);
						} else {
							Console.Error.WriteLine(e.Message);
						}
					}

				} break;
		}

	}

}
