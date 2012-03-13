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
//#define FORMAT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MK.CodeDom.Compiler;
using System.IO;
using MK.JavaScript.Css;
using System.Reflection;
using MK.JavaScript.Css.Selectors;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using System.Xml.Linq;

namespace MK.JavaScript.Compiler {

	public static class CssSupport {

		public class CssData {
			public HashSet<PageFrameCompiler> Pages = new HashSet<PageFrameCompiler>();
		}

		private static Dictionary<Type, CssData> styles = new Dictionary<Type, CssData>();

		public static CssData Resolve(Type cssType) { return styles[cssType]; }

		static void Register(//PageFrameCompiler page, 
			Type cssType) {
			CssData data;
			if (!styles.TryGetValue(cssType, out data)) {
				data = new CssData();
				styles[cssType] = data;
			}
		}

		private static List<MethodBase> methods = new List<MethodBase>();

		public static void Register(MethodBase methodBase, object[] attrs) {
			methods.Add(methodBase);
			foreach (ImportAttribute attr in attrs) {
				foreach (var type in attr.CssTypes) {
					Register(type);
				}
			}
		}
		public class CssFileData {
			private string guid = Guid.NewGuid().ToString();
			public readonly List<Type> StyleSheets = new List<Type>();
			public readonly HashSet<PageFrameCompiler> Pages;
			public CssFileData(HashSet<PageFrameCompiler> pages, Type cssType) {
				this.Pages = pages;
				this.AddStyleSheet(cssType);
			}
			public string CssFileName {
				get {
					return
#if !OBFUSCATE
 "Css=" + string.Join("_", this.StyleSheets.Select(ss => ss.GetType().Name).ToArray()) + "=" +
#endif
 this.guid;
				}
			}

			public void AddStyleSheet(Type cssType) {
				this.StyleSheets.Add(cssType);
			}

		}
		private static List<CssFileData> cssFiles = new List<CssFileData>();
		public static IEnumerable<CssFileData> CssFiles { get { return cssFiles; } }

		public static IEnumerable<XElement> CreateConfigurationElements() {
			
			foreach (var cssFile in cssFiles) {
				var css=new XElement("css",
					new XAttribute("name",cssFile.CssFileName)
				);
				foreach (var type in cssFile.StyleSheets) {
					css.Add(new XElement("type",
						new XAttribute("name", type.FullName),
						new XAttribute("assembly", type.Assembly.GetName().Name)
					));
				}
				yield return css;
			}

		}

		public static void Compute(PageFrameCompiler[] compilers) {
			foreach (var compiler in compilers) {
				foreach (var data in
									from method in methods
									where compiler.RuntimeFrameCompiler.Methods.ContainsKey(method)
										&& compiler.RuntimeFrameCompiler.Dependencies.IsPathFromTo(compiler.TypeData, compiler.RuntimeFrameCompiler.Methods[method])
									from MK.JavaScript.Css.ImportAttribute attr in method.GetCustomAttributes(typeof(MK.JavaScript.Css.ImportAttribute), false)
									from type in attr.CssTypes
									select CssSupport.Resolve(type)
				) {
					data.Pages.Add(compiler);
				}
			}
			foreach (var kv in styles) {
				foreach (var cssFile in cssFiles) {
					if (cssFile.Pages.SetEquals(kv.Value.Pages)) {
						cssFile.AddStyleSheet(kv.Key);
						goto end;
					}
				}
				cssFiles.Add(new CssFileData(kv.Value.Pages, kv.Key));
			end:
				;
			}
		}
	}
}
