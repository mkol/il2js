/*
il2js Framework.dll - JavaScript VM for .NET
Copyright (C) 2011 Michael Kolarz

This library is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as
published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library.
If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Reflection;
using MK.JavaScript.Ajax.Serialization;
using System.Text.RegularExpressions;
using MK.JavaScript.Css;
using MK.JavaScript.Dom;
using System.Runtime.Serialization;

namespace MK.JavaScript.Compiler {
	/// <summary>
	/// Utility class for creating css.
	/// </summary>
	public static class CssWriter {
		private static FieldInfo fieldInfo = typeof(Style).GetField("_values", BindingFlags.Instance | BindingFlags.NonPublic);
		private static string dasherize(string str) {
			return new Regex(@"[A-Z]", RegexOptions.Compiled).Replace(str, m => {
				return "-" + char.ToLower(m.Value[0]);
			});
		}
		internal static void append(Style style, StringBuilder sb) {
			int tmp = sb.Length;
			foreach (var entry in (Dictionary<string, string>)fieldInfo.GetValue(style)) {
				sb.Append(dasherize(entry.Key)).Append(':').Append(entry.Value).Append(';');
#if FORMAT
					sb.Append('\n');
#endif
			}
			if (sb.Length != tmp)
				--sb.Length;
#if FORMAT
				sb.Append('\n');
#endif
		}
		private static void append(IEnumerable<Rule> rules, StringBuilder sb) {
			foreach (var rule in rules) {
				sb.Append(rule.Selector);
				sb.Append('{');
#if FORMAT
					sb.Append('\n');
#endif
				append(rule.Style, sb);
				sb.Append('}');
#if FORMAT
					sb.Append('\n');
#endif
			}
		}

		public static void Write(string outputPath, IEnumerable<Type> styleSheetTypes) {
			StringBuilder sb = new StringBuilder();
			foreach (var type in styleSheetTypes) {
				append(((StyleSheet)type.GetConstructor(Type.EmptyTypes).Invoke(null)).GetRules(), sb);
			}
			File.WriteAllText(outputPath, sb.ToString());
		}
	}
	/// <summary>
	/// Utility class fof creating html.
	/// </summary>
	public static class HtmlWriter {
		private const string idPrefix = "__";
		private const string classPrefix = "__";

		//private static string content(Element element) {
		//  StringBuilder sb = new StringBuilder();
		//  element.append(sb);
		//  return sb.ToString();
		//}


		private static void gather(Dictionary<Style, string> css, IEnumerable<Element> elements) {
			foreach (var element in elements) {
				if (element.Style != null) {
					string className;
					if (!css.TryGetValue(element.Style, out className)) {
						className = classPrefix + css.Count.ToString("x");
						css[element.Style] = className;
					}
					if (element.ClassName == null) {
						element.ClassName = className;
					} else {
						element.ClassName += " " + className;
					}
				}
				gather(css, element.elements);
			}
		}

		/// <summary>
		/// Creates page using Render method.
		/// </summary>
		/// <param name="path">Page path.</param>
		/// <param name="windowType">Page type.</param>
		public static void Write(string path, string prototypeJsPath, Type windowType) {
			var window = (Window)windowType.GetConstructor(Type.EmptyTypes).Invoke(null);

			var render = windowType.GetMethod("Render", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);

			Element[] elements;
			if (render.ReturnType == typeof(Element)) {
				elements = new Element[] { (Element)render.Invoke(window, null) };
			} else {
				elements = ((IEnumerable<Element>)render.Invoke(window, null)).ToArray();
			}
			#region css
			Dictionary<Style, string> css = new Dictionary<Style, string>();
			gather(css, elements);
			#endregion
			#region binding
			int bindingsCount = 0;
			Dictionary<char, string> bindings = new Dictionary<char, string>();
			var serializer = (ClassSerializer)Serializer.GetSerializer(windowType);
			foreach (var fieldInfo in windowType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Where(
				f => {
					var type = f.FieldType.IsArray ? f.FieldType.GetElementType() : f.FieldType;
					return type == typeof(Element) || type.IsSubclassOf(typeof(Element));
				})) {
				if (fieldInfo.FieldType.IsArray) {
					var array = (Element[])fieldInfo.GetValue(window);
					if (array != null) {
						var attributes = fieldInfo.GetCustomAttributes(typeof(DataMemberAttribute), false);
						if (attributes.Length != 1) {
							throw new Exception("Field " + windowType.FullName + "::" + fieldInfo.Name + " cannot be set in Render method because it is not marked as DataMember.");
						}
						bindings[serializer.GetToken(fieldInfo)] = "[" + string.Join(",", array.Select(element => {
							if (element == null) return "null";

							if (element.Id == null) {
								element.Id = idPrefix + (bindingsCount++).ToString("x");

							}
							return "\"" + element.Id + "\"";
						}).ToArray()) + "]";
					}
				} else {
					var element = (Element)fieldInfo.GetValue(window);
					if (element != null) {
						var attributes = fieldInfo.GetCustomAttributes(typeof(DataMemberAttribute), false);
						if (attributes.Length != 1) {
							throw new Exception("Field " + windowType.FullName + "::" + fieldInfo.Name + " cannot be set in Render method because it is not marked as DataMember.");
						}

						if (element.Id == null) {
							element.Id = idPrefix + (bindingsCount++).ToString("x");
						}
						bindings[serializer.GetToken(fieldInfo)] = "\"" + element.Id + "\"";
					}
				}
			}
			#endregion
			#region render
			var sb = new StringBuilder();
			sb.Append(
@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd""><html xmlns=""http://www.w3.org/1999/xhtml""><!--page generated by il2js--><head><title>").Append(window.Document.Title).Append(@"</title><script type=""text/javascript"" src=""").Append(prototypeJsPath).Append(@"""></script>");
			if (css.Count != 0) {
				sb.Append(@"<link type=""text/css"" rel=""stylesheet""  href=""").Append(Path.GetFileName(path)).Append(".css\" />");
				var sbCss = new StringBuilder();
				foreach (var item in css) {
					sbCss.Append('.').Append(item.Value).Append('{');
					CssWriter.append(item.Key, sbCss);
					sbCss.Append('}');
				}
				File.WriteAllText(path + ".css", sbCss.ToString());
			}
			if (bindings.Count != 0) {
				sb.Append(@"<script type=""text/javascript"">function __(){document.observe(""dom:loaded"",function(x,y){x={")
					.Append(string.Join(",", bindings.Select(b => "\"" + b.Key + "\":" + b.Value).ToArray()))
					.Append("};for(y in x)window[y]=$.apply(0,Object.isArray(x[y])?x[y]:[x[y]])})};if(!Prototype.Browser.IE)__()</script>");

			}
			sb.Append("<!--powered by il2js framework (http://smp.if.uj.edu.pl/~mkol/il2js)--><!--/il2js-->");
			if (bindings.Count != 0) {
				sb.Append(@"<script type=""text/javascript"">;if(Prototype.Browser.IE)__()</script>");

			}
			sb.Append("</head><body>");
			foreach (var element in elements) {
				element.append(sb);
			}
			sb.Append(@"</body></html>");
			File.WriteAllText(path, sb.ToString());
			#endregion
		}
	}

	/// <summary>
	/// Utility class for creating resources.
	/// </summary>
	public class ResourceWriter {
		private readonly IEnumerable<CultureInfo> cultures;
		private readonly List<KeyValuePair<Type, IEnumerable<string>>> resources = new List<KeyValuePair<Type, IEnumerable<string>>>();
		public ResourceWriter(IEnumerable<CultureInfo> cultures) {
			this.cultures = cultures;
		}
		/// <summary>
		/// Adds resource to list of resources.
		/// </summary>
		/// <param name="resource">Resource type</param>
		/// <param name="members">Members to serialize</param>
		public void AddResource(Type resource, IEnumerable<string> members) {
			this.resources.Add(new KeyValuePair<Type, IEnumerable<string>>(resource, members));
		}
		/// <summary>
		/// Writes resource files for web pages.
		/// </summary>
		/// <param name="outputPath">Output path for Resources.aspx.</param>
		/// <param name="fileName">Resources.aspx file name.</param>
		public void Write(string outputPath, string fileName) {
			File.WriteAllText(outputPath + Path.DirectorySeparatorChar + fileName,
@"<%@ Page Language=""C#"" ContentType=""text/plain"" %><%
	var a = this.Request.QueryString[0].Split(',');
	if (a.Length == 1) {
		this.Response.Write(this.GetLocalResourceObject(a[0]));
	} else {
		this.Response.Write('[');
		this.Response.Write(this.GetLocalResourceObject(a[0]));
		for (int i = 1; i < a.Length; ++i) {
			this.Response.Write(',');
			this.Response.Write(this.GetLocalResourceObject(a[i]));
		}
		this.Response.Write(']');
	}
%>");

			Directory.CreateDirectory(outputPath + Path.DirectorySeparatorChar + "App_LocalResources");

			this.write(outputPath + Path.DirectorySeparatorChar + "App_LocalResources" + Path.DirectorySeparatorChar + fileName + ".resx", CultureInfo.InvariantCulture);
			foreach (var culture in this.cultures) {
				this.write(outputPath + Path.DirectorySeparatorChar + "App_LocalResources" + Path.DirectorySeparatorChar + fileName + "." + culture.Name + ".resx", culture);
			}
		}

		private void write(string output, CultureInfo culture) {
			ResXResourceWriter writer = new ResXResourceWriter(output);
			for (int i = 0; i < this.resources.Count; ++i) {
				this.resources[i].Key
					.GetProperty("Culture", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
					.SetValue(null, culture, null);
				writer.AddResource(
					i.ToString(),
					Serializer.Serialize(
						this.resources[i].Value.Select(member => this.resources[i].Key.GetProperty(member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, null))));
			}
			writer.Close();
		}
	}
}
