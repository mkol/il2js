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
using System.Reflection;
using System.Reflection.Emit;
using MK.CodeDom.Compiler;
using System.IO;
using MK.JavaScript.Ajax;
using System.Text.RegularExpressions;

namespace MK.JavaScript.CodeDom.Compiler {

	static class CSharpType {

		public static string CSharpName(string name, ref int pos) {
			StringBuilder nameBuilder = new StringBuilder();
			List<string> genericTypes = null;
			do {
				switch (name[pos]) {
					case ',':
					case ']':
						int offset = 0;
						return Regex.Replace(nameBuilder.ToString(), @"`([0-9]+)", m => {
							int count = int.Parse(m.Groups[1].Value);
							var sb = new StringBuilder();
							sb.Append('<');
							for (int i = 0; i < count; ++i) {
								sb.Append(genericTypes[offset + i]).Append(',');
							}
							offset += count;
							--sb.Length;
							sb.Append('>');
							return sb.ToString();
						}, RegexOptions.Compiled);
					case '[':
						if (name[pos + 1] == ']') {
							nameBuilder.Append("[]");
							++pos;
						} else {
							genericTypes = new List<string>();
							do {
								++pos;
								genericTypes.Add(CSharpName(name, ref pos));
							} while (name[pos] != ']');
						}
						break;
					case '+':
						nameBuilder.Append('.');
						break;
					default:
						nameBuilder.Append(name[pos]);
						break;
				}
				++pos;
			} while (true);
		}
		public static string GetCSharpName(this Type type) {
			int pos = 0;
			return CSharpType.CSharpName(type.ToString() + ",", ref pos);
		}
	}


	class ServerManager {
		private static Dictionary<MethodInfo, int> used = new Dictionary<MethodInfo, int>();
		public static int Register(MethodInfo info, RunAtAttribute runAt) {
			int index;
			if (!used.TryGetValue(info, out index)) {
				index = used.Count;
				used[info] = index;
				onRegistered(info, runAt, index);
			}
			return index;
		}
		public static int IndexOf(MethodInfo info) {
			return used[info];
		}
		static string Name(Type type) {
			return type.GetCSharpName();
		}

		private static void onRegistered(MethodInfo method, RunAtAttribute runAt, int index) {
			if (method.IsConstructor) ThrowHelper.Throw("{0} cannot be called @sever since it is constructor.", method.GetSignature());
			if (method.IsVirtual) ThrowHelper.Throw("{0} cannot be called @sever since it is virtual.", method.GetSignature());
			sb.Append("case ").Append(index).Append(":try{");
			if (!method.IsStatic || method.GetParameters().Length > 0) {
				sb.Append("args=Serializer.Deserialize(context");
				if (!method.IsStatic)
					sb.Append(",typeof(").Append(Name(method.DeclaringType)).Append(')');
				foreach (var param in method.GetParameters()) {
					sb.Append(",typeof(").Append(Name(param.ParameterType)).Append(')');
				}
				sb.Append(");");
			}
			if (method.ReturnParameter.ParameterType != typeof(void))
				sb.Append("context.Response.Write(\"(\"+Serializer.Serialize(");
			if (method.IsSpecialName) {
				if (method.Name.StartsWith("get_")) {
					appendThisOrType(method);
					sb.Append('.').Append(method.Name.Substring(4));
				} else if (method.Name.StartsWith("set_")) {
					appendThisOrType(method);
					sb.Append('.').Append(method.Name.Substring(4)).Append("=args[");
					sb.Append(method.IsStatic ? 0 : 1);
					sb.Append("]");
				} else {
					ThrowHelper.Throw("Special name method {0} cannot be called @server.", method.GetSignature());
				}
			} else {
				appendThisOrType(method);
				sb.Append('.').Append(method.Name).Append('(');
				var parameters = method.GetParameters();
				if (parameters.Length > 0) {
					for (int j = 0; j < parameters.Length; ++j) {
						sb.Append('(').Append(Name(parameters[j].ParameterType)).Append(")args[").Append(method.IsStatic ? j : (j + 1)).Append("],");
					}
					--sb.Length;
				}
				sb.Append(")");
			}
			if (method.ReturnParameter.ParameterType != typeof(void)) {
				sb.Append(")+\")\")");
			} else {
				sb.Append(";context.Response.Write(\"0\")");
			}
			sb.Append(";}catch(Exception e){context.Response.Write(");

			if (runAt.HideExceptionMessage) {
				sb.Append("\"throw Error()\"");
			} else {
				sb.Append("\"throw Error(\"+Serializer.Serialize(e.Message)+\")\"");
			}
			sb.Append(");}break;");
		}

		private static void appendThisOrType(MethodInfo method) {
			if (!method.IsStatic)
				sb.Append("((");
			sb.Append(Name(method.DeclaringType));
			if (!method.IsStatic)
				sb.Append(")args[0])");
		}

		private const string header =
@"<%@ WebHandler Language=""C#"" Class=""JavascriptHandler"" %>
using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MK.JavaScript.Ajax.Serialization;

public class JavascriptHandler : IHttpHandler {
	public void ProcessRequest(HttpContext context) {
		context.Response.ContentType = ""text/plain"";
		List<object> args;
		switch(int.Parse(context.Request.QueryString[0])){
";

		private static StringBuilder sb;
		static ServerManager() {
			sb = new StringBuilder();
			sb.Append(header);
		}

		public static void Dump(string outputPath) {
			if (sb.Length == header.Length) return;

			sb.Append(@"
		}
	}
	public bool IsReusable { get { return true; } }
}
");
			File.WriteAllText(outputPath + Path.DirectorySeparatorChar + Settings.HandlerAshxFileName + ".ashx", sb.ToString());
		}
	}
}
