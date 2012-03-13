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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using MK.CodeDom.Compiler;
using MK.JavaScript.Framework;
using MK.JavaScript.Reflection;
using System.Threading;
using MK.JavaScript.Framework.Mappings;
using System.Collections;

namespace MK.JavaScript.CodeDom.Compiler {
	public class NativesManagerBase {


		public MethodInfo GetMethodOfGenericTypeThatMatch(MethodInfo method) {
			try {
				return method.DeclaringType.GetGenericTypeDefinition().GetMethod(method.Name);
			} catch { }
			foreach (var baseMethod in method.DeclaringType.GetGenericTypeDefinition().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
				//if (method.DeclaringType.GetMethod(baseMethod.Name,
				//    baseMethod.GetParameters().Select(pi => {
				//      if (pi.ParameterType.IsGenericParameter) {
				//        return method.DeclaringType.GetGenericArguments()[pi.ParameterType.GenericParameterPosition];
				//      } else {
				//        return pi.ParameterType;
				//      }
				//    }).ToArray()

				//    ) == method) {
				if (baseMethod.MetadataToken == method.MetadataToken) {
					return baseMethod;
				}
			}
			return null;
		}
	}

	public class NativesManager : NativesManagerBase {
#warning zmienic
		//public static readonly NativesManager Instance = new NativesManager();
		public static NativesManager Instance {
			get;
			internal set;
		}

		#region MethodMappings
		public readonly Dictionary<MethodBase, MethodBase> MethodMappings = new Dictionary<MethodBase, MethodBase>();
		public void AddMethodMapping(MethodInfo method, Type targetType) {
			MethodMappings[method] = targetType.GetMethod(method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray());
		}
		public void AddTypeMapping(Type type, Type targetType) {
			foreach (var targetMethod in targetType.GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static)) {
				MethodMappings[type.GetMethod(
						targetMethod.Name,
						BindingFlags.Public | BindingFlags.DeclaredOnly | (targetMethod.IsStatic ? BindingFlags.Static : BindingFlags.Instance),
						null,
						targetMethod.CallingConvention,
						targetMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
						null
				)] = targetMethod;
			}
		}
		public void AddInterfaceMapping(Type type, Type targetType) {
			foreach (var interfaceType in targetType.GetInterfaces()) {
				if (interfaceType == typeof(IDisposable))
					continue;
				var targetMap = targetType.GetInterfaceMap(interfaceType);
				var tmp = type.GetInterfaces().FirstOrDefault(i => i.MetadataToken == interfaceType.MetadataToken);
				if (tmp != null) {
					var map = type.GetInterfaceMap(tmp);
					for (int i = 0; i < map.InterfaceMethods.Length; ++i) {
						if (targetMap.InterfaceMethods[i].Name == "Dispose")
							continue;
						MethodMappings[map.TargetMethods[i]] = targetMap.TargetMethods[i];
					}
				}
			}
		}
		public MethodBase ResolveMapping(MethodBase method) {
			MethodBase value;
			while (this.MethodMappings.TryGetValue(method, out value)) {
				method = value;
			}
			return method;
		}
		#endregion


		private readonly Dictionary<MemberInfo, JSNative> dotNetNatives = new Dictionary<MemberInfo, JSNative>();
		private readonly Dictionary<Type, List<MemberInfo>> dotNetNativeMembersForType = new Dictionary<Type, List<MemberInfo>>();
		private readonly HashSet<MemberInfo> dotNetNotNatives = new HashSet<MemberInfo>();


		private static string get_native_Part(JSNative Native, MemberInfo member) {
			if (Native.Name != null) return Native.Name;
			if (Native.Code != null) throw new Exception(Native.Code + " != null");
			switch (member.MemberType) {
				case MemberTypes.NestedType:
				case MemberTypes.TypeInfo:
					return member.Name;
				default: return GetNativeName(member.Name);
			}
		}
		public static string GetNativeName(string name) {
			return char.ToLower(name[0]) + name.Substring(1);
		}
		public string Get_native_Value(JSNative Native, MemberInfo member) {
			if (Native.Code != null) return Native.Code;
			switch (member.MemberType) {
				case MemberTypes.Field: {
						var field = (FieldInfo)member;
						if (field.IsStatic && IsNative(member.DeclaringType)) {
							return Get_native_Value(GetNative(member.DeclaringType), member.DeclaringType) + "." + get_native_Part(Native, member);
						} else {
							return get_native_Part(Native, member);
						}
					}
				case MemberTypes.TypeInfo:
				case MemberTypes.NestedType:
					if (member.DeclaringType != null && IsNative(member.DeclaringType)) {
						return Get_native_Value(GetNative(member.DeclaringType), member.DeclaringType) + "." + get_native_Part(Native, member);
					} else {
						return get_native_Part(Native, member);
					}
				case MemberTypes.Constructor:
					if (member.DeclaringType.DeclaringType != null && IsNative(member.DeclaringType.DeclaringType)) {
						return Get_native_Value(GetNative(member.DeclaringType.DeclaringMethod), member.DeclaringType.DeclaringMethod) + "." + get_native_Part(Native, member);
					} else {
						return get_native_Part(Native, member.DeclaringType);
					}
				case MemberTypes.Method: return get_native_Part(Native, member);
				default: throw new Exception(member.MemberType + "??");
			}

		}
		private bool isNativeNamespace(string s) {
			return s != null && (s.StartsWith("System") || s.StartsWith("Microsoft") || s.StartsWith("MS"));
		}
		public bool IsNative(MemberInfo member) {
			return
				(isNativeNamespace(member.MemberType == MemberTypes.TypeInfo ? ((Type)member).Namespace : member.DeclaringType.Namespace) && !_IsNotNative(member))

					|| member.GetCustomAttributes(typeof(JSNative), false).Length == 1
					|| _IsNative(member)

			;
		}
		private bool _IsNative(MemberInfo member) {
			if (dotNetNatives.ContainsKey(member)) return true;
			var method = member as MethodInfo;
			if (method == null || !member.DeclaringType.IsGenericType) return false;
			var genmethod = GetMethodOfGenericTypeThatMatch(method);
			if (genmethod != null && genmethod != method)
				return IsNative(genmethod);
			return dotNetNatives.ContainsKey(method);
		}
		private bool _IsNotNative(MemberInfo member) {
			if (dotNetNotNatives.Contains(member)
				|| (member.DeclaringType != null && dotNetNotNatives.Contains(member.DeclaringType))) {
				return true;
			}
			var method = member as MethodInfo;
			if (method == null || !member.DeclaringType.IsGenericType) return false;
			var genmethod = GetMethodOfGenericTypeThatMatch(method);
			if (genmethod != null && genmethod != method)
				return !IsNative(genmethod);
			return dotNetNotNatives.Contains(method)
				|| (member.DeclaringType != null && dotNetNotNatives.Contains(member.DeclaringType));
		}
		public JSNative GetNative(MemberInfo member) {
			var attribtes = member.GetCustomAttributes(typeof(JSNative), true);
			if (attribtes.Length == 1) {
				return (JSNative)attribtes[0];
			} else {
				JSNative attribute;
				if (dotNetNatives.TryGetValue(member, out attribute))
					return attribute;
				else if (member is MethodInfo) {
					var methodInfo = (MethodInfo)member;
					MethodInfo degenericMethod = null;
					if (methodInfo.IsGenericMethod && dotNetNatives.TryGetValue(methodInfo.GetGenericMethodDefinition(), out attribute)) {
						return attribute;
					} else if (methodInfo.DeclaringType.IsGenericType) {
						degenericMethod = GetMethodOfGenericTypeThatMatch(methodInfo);
						if (degenericMethod != null && dotNetNatives.TryGetValue(degenericMethod, out attribute)) {
							return attribute;
						}
					}
					foreach (var @interface in methodInfo.DeclaringType.GetInterfaces()) {
						var mapping = methodInfo.DeclaringType.GetInterfaceMap(@interface);
						for (int i = 0; i < mapping.InterfaceMethods.Length; ++i) {
							if (mapping.TargetMethods[i] == methodInfo || mapping.TargetMethods[i] == degenericMethod)
								try {
									return GetNative(mapping.InterfaceMethods[i]);
								} catch { }
						}
					}
					//zeby np nie natiwowac ciagle X.ToString : Object.ToString
					var baseDefinition = methodInfo.GetBaseDefinition();
					if (baseDefinition != methodInfo) {
						try {
							return GetNative(baseDefinition);
						} catch { }
					}
				} else if (member is ConstructorInfo) {
					var constructorInfo = (ConstructorInfo)member;
					if (constructorInfo.DeclaringType.IsGenericType &&
							dotNetNatives.TryGetValue(
									constructorInfo.DeclaringType.GetGenericTypeDefinition().GetConstructor(
											(from p in constructorInfo.GetParameters() select p.ParameterType).ToArray()
									),
									out attribute)
					) {
						return attribute;
					}
				}
				ThrowHelper.Throw("Cannot resolve native {0}.", member.GetSignature());
				return null;
			}
		}

		public void RegisterDotNetNative(Type member) {
			//tu nic nie ma bo nie jest ot potrzebne
		}
		public void RegisterDotNetNative(Type member, JSNative attribute) {
			dotNetNatives[member] = attribute;
		}
		public void RegisterDotNetNative(MemberInfo member, JSNative attribute) {
			List<MemberInfo> members;

#if VALIDATION
			if (attribute.CallType != NativeCallType.Default && member is MethodBase && ((MethodBase)member).IsVirtual) {
				ThrowHelper.Throw("JSNative::CallType cannot be set for {0} since it is virtual.", member.GetSignature());
			}
#endif

			if (member.DeclaringType.IsGenericTypeDefinition) {
				if (!dotNetNativeMembersForType.TryGetValue(member.DeclaringType, out members)) {
					members = new List<MemberInfo>();
					dotNetNativeMembersForType[member.DeclaringType] = members;
				}
				dotNetNativeMembersForType[member.DeclaringType].Add(member);
			}
			dotNetNatives[member] = attribute;
		}
		public NativesManager(string mappingXmlPath) {
			Type type;
			#region namespace System
			#region class Diagnostics.Debug
			type = typeof(System.Diagnostics.Debug);
			RegisterDotNetNative(type, new JSNative("console") {
				//AdditionalCode = @"if(!this.console){this.console={log:function(_){if(!_){alert}}}}" 
			});
			RegisterDotNetNative(type.GetMethod("WriteLine", new Type[] { typeof(string) }), new JSNative("log"));
			RegisterDotNetNative(type.GetMethod("WriteLine", new Type[] { typeof(object) }), new JSNative("log"));
			#endregion
			#region class Math -> Math
			type = typeof(System.Math);
			RegisterDotNetNative(type, new JSNative());
			#endregion
			#region struct System.Int32
			type = typeof(System.Int32);
			RegisterDotNetNative(type);
			RegisterDotNetNative(type.GetMethod("Parse", new Type[] { typeof(string) }), new JSNative("parseInt") { GlobalMethod = true });
			#endregion
			#region struct System.Byte
			type = typeof(System.Byte);
			RegisterDotNetNative(type);
			RegisterDotNetNative(type.GetMethod("Parse", new Type[] { typeof(string) }), new JSNative("parseInt") { GlobalMethod = true });
			#endregion
			#region struct System.SByte
			type = typeof(System.SByte);
			RegisterDotNetNative(type);
			RegisterDotNetNative(type.GetMethod("Parse", new Type[] { typeof(string) }), new JSNative("parseInt") { GlobalMethod = true });
			#endregion
			#region struct System.Double
			type = typeof(System.Double);
			RegisterDotNetNative(type);
			RegisterDotNetNative(type.GetMethod("Parse", new Type[] { typeof(string) }), new JSNative("parseFloat") { GlobalMethod = true });
			#endregion



			//tych sie nie da tak latwo oskryptowac...
			#region class System.Drawing.Color
			type = typeof(System.Drawing.Color);
			RegisterDotNetNative(type);
			foreach (var color in type.GetProperties(BindingFlags.Public | BindingFlags.Static)) {
				var c = (System.Drawing.Color)color.GetGetMethod().Invoke(null, null);

				RegisterDotNetNative(color.GetGetMethod(), new JSNative { Value = "#" + c.R.ToString("x2") + c.G.ToString("x2") + c.B.ToString("x2") });
			}
			RegisterDotNetNative(type.GetField("Empty"), new JSNative { Value = "" });
			#endregion
			#region class System.Linq.Enumerable -> Array
			type = typeof(System.Linq.Enumerable);
			RegisterDotNetNative(type);
			foreach (var method in type.GetMethods()) {
				switch (method.Name) {
#warning efektywnosc
					case "Range": RegisterDotNetNative(method, new PrototypeJS { Code = "$R($0,$0+$1-1).toArray()" }); break;
					case "ToDictionary":
						switch (method.GetParameters().Length) {
							case 2: RegisterDotNetNative(method, new PrototypeJS {
								Code = @"
function(a,b){
	b=$handler$(b);
	return a.inject({},function(x,y){
		x[b(y)]=y;
    return x;
	});
}
"
							}); break;
#warning nie.. bo jest jeszcze jedna z 3 parametrami i eq komperatorem
							case 3: RegisterDotNetNative(method, new PrototypeJS {
								Code = @"
function(a,b,c){
	b=$handler$(b);
	c=$handler$(c);
	return a.inject({},function(x,y){
		x[b(y)]=c(y);
    return x;
	});
}
"
							}); break;
						}
						break;
					case "Distinct":
						switch (method.GetParameters().Length) {
							case 1: RegisterDotNetNative(method, new PrototypeJS("uniq") { CallType = NativeCallType.Instance }); break;
							case 2: break;
						}
						break;
					//#error TO SIE TERAZ POSYPALO BO SA NIE ARAJOWE ENUMERABLE
					case "ToArray": RegisterDotNetNative(method, new PrototypeJS { Ignore = true }); break;
					case "ToList": RegisterDotNetNative(method, new PrototypeJS { Ignore = true }); break;
					case "Select": RegisterDotNetNative(method, new PrototypeJS { Code = "$0.map($handler$($1))" }); break;
					case "All": RegisterDotNetNative(method, new PrototypeJS { Code = "$0.all($handler$($1))" }); break;
					case "Any":
						switch (method.GetParameters().Length) {
							case 1: RegisterDotNetNative(method, new PrototypeJS { CallType = NativeCallType.Instance }); break;
							case 2: RegisterDotNetNative(method, new PrototypeJS { Code = "$0.any($handler$($1))", }); break;
						}
						break;
					case "Max":
						switch (method.GetParameters().Length) {
							case 1: RegisterDotNetNative(method, new PrototypeJS { CallType = NativeCallType.Instance }); break;
							case 2: RegisterDotNetNative(method, new PrototypeJS { Code = "$0.max($handler$($1))", }); break;
						}
						break;
					case "Min":
						switch (method.GetParameters().Length) {
							case 1: RegisterDotNetNative(method, new PrototypeJS { CallType = NativeCallType.Instance }); break;
							case 2: RegisterDotNetNative(method, new PrototypeJS { Code = "$0.min($handler$($1))", }); break;
						}
						break;
					case "OrderBy":
						switch (method.GetParameters().Length) {
							case 2: RegisterDotNetNative(method, new PrototypeJS { Code = "$0.sortBy($handler$($1))" }); break;
							case 3: break;
						}
						break;
					case "Where": RegisterDotNetNative(method, new PrototypeJS { Code = "$0.select($handler$($1))" }); break;
#warning znaczenie dla stosu jest inne niz oryginalne w .net'cie
					case "First":
						switch (method.GetParameters().Length) {
							case 1: RegisterDotNetNative(method, new PrototypeJS { CallType = NativeCallType.Instance }); break;
							case 2: RegisterDotNetNative(method, new PrototypeJS { Code = "$0.find($handler$($1))", }); break;
						}
						break;
#warning znaczenie dla stosu jest inne niz oryginalne w .net'cie
					case "Last":
						switch (method.GetParameters().Length) {
							case 1: RegisterDotNetNative(method, new PrototypeJS { CallType = NativeCallType.Instance }); break;
							case 2: break;
						}
						break;
				}
			}

			#endregion
			#endregion

			#region not natives
			dotNetNotNatives.Add(typeof(System.Collections.IEnumerable));
			dotNetNotNatives.Add(typeof(System.Collections.IEnumerator));
			dotNetNotNatives.Add(typeof(System.Collections.Generic.IEnumerable<>));
			dotNetNotNatives.Add(typeof(System.Collections.Generic.IEnumerator<>));
			#endregion

			LoadSystemMapping(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Mapping.xml");
			LoadSystemMapping(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Mapping2.xml");

			if (File.Exists(mappingXmlPath))
				LoadSystemMapping(mappingXmlPath);


			AddTypeMapping(typeof(System.Threading.Monitor), typeof(MK.JavaScript.Framework.Mappings.System.Threading.Monitor));
			AddTypeMapping(typeof(System.Threading.Thread), typeof(MK.JavaScript.Framework.Mappings.System.Threading.Thread));


			AddInterfaceMapping(typeof(System.Array),
				typeof(MK.JavaScript.Framework.Mappings.JSEnumerable<>));
			AddInterfaceMapping(typeof(System.Collections.Generic.Dictionary<,>.KeyCollection),
				typeof(MK.JavaScript.Framework.Mappings.JSEnumerable<>));
			AddInterfaceMapping(typeof(System.Collections.Generic.Dictionary<,>.ValueCollection),
				typeof(MK.JavaScript.Framework.Mappings.JSEnumerable<>));
			AddInterfaceMapping(typeof(System.Collections.Generic.List<>),
				typeof(MK.JavaScript.Framework.Mappings.JSEnumerable<>));

			#region GetEnumerator
			var GetEnumeratorMethodInfo = typeof(JSEnumerable<>).GetMethod("GetEnumerator");

			MethodMappings[typeof(System.Collections.Generic.Dictionary<,>.KeyCollection).GetMethod("GetEnumerator")] = GetEnumeratorMethodInfo;
			MethodMappings[typeof(System.Collections.Generic.Dictionary<,>.ValueCollection).GetMethod("GetEnumerator")] = GetEnumeratorMethodInfo;
			MethodMappings[typeof(System.Collections.Generic.List<>).GetMethod("GetEnumerator")] = GetEnumeratorMethodInfo;
			#endregion
			#region get_Current
			var get_CurrentMethodInfo = typeof(JSEnumerable<>).GetMethod("get_Current");

			MethodMappings[typeof(System.Collections.Generic.Dictionary<,>.KeyCollection.Enumerator).GetMethod("get_Current")] = get_CurrentMethodInfo;
			MethodMappings[typeof(System.Collections.Generic.Dictionary<,>.ValueCollection.Enumerator).GetMethod("get_Current")] = get_CurrentMethodInfo;
			MethodMappings[typeof(System.Collections.Generic.List<>.Enumerator).GetMethod("get_Current")] = get_CurrentMethodInfo;
			#endregion


			AddInterfaceMapping(typeof(System.Collections.Generic.List<>.Enumerator),
				typeof(MK.JavaScript.Framework.Mappings.JSArrayEnumerator<>));
			AddInterfaceMapping(typeof(System.Collections.Generic.Dictionary<,>.KeyCollection.Enumerator),
				typeof(MK.JavaScript.Framework.Mappings.JSArrayEnumerator<>));
			AddInterfaceMapping(typeof(System.Collections.Generic.Dictionary<,>.ValueCollection.Enumerator),
				typeof(MK.JavaScript.Framework.Mappings.JSArrayEnumerator<>));


			type = typeof(Delegate);
			RegisterDotNetNative(type);
			RegisterDotNetNative(type.GetMethod("op_Equality", new Type[] { typeof(Delegate), typeof(Delegate) }), new JSNative() {
				Code = "$0==null?$1==null:($1!=null&&$0.o==$1.o&&$0.m==$1.m)"
			});


			#region framework
			type = typeof(JSEventHandler);
			RegisterDotNetNative(type);
			RegisterDotNetNative(type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new Type[] { typeof(object), typeof(IntPtr) }, null), new PrototypeJS() {
				Code = "$handler$({o:$0,m:$1}).bindAsEventListener()"
			});

			type = typeof(FrameworkUtil);
			RegisterDotNetNative(type);
			RegisterDotNetNative(type.GetConstructors()[0], new JSNative { Code = "{o:$0,m:$1}" });

			//type = typeof(MK.JavaScript.Dom.SelectOptionsList);
			//RegisterDotNetNative(type);

#warning tu i inne getenumeratory... a generyczne?
			#endregion
		}

		#region Mapping.xml
		private static readonly XNamespace NS = "http://smp.if.uj.edu.pl/~mkol/il2js/Mapping.xsd";
		private static Type GetParameterType(XElement element) {
			var name = element.Attribute("name").Value;
			var type = Type.GetType(name);
			if (type == null) {
				var namespaces = element.Ancestors(NS + "namespace").Select(e => e.Attribute("name").Value).ToArray();
				for (int nc = 0; nc < namespaces.Length; ++nc) {
					StringBuilder sb = new StringBuilder();
					for (int i = namespaces.Length - 1; i >= nc; --i) {
						sb.Append(namespaces[i]).Append('.');
					}
					sb.Append(name);
					type = Type.GetType(sb.ToString());
					if (type != null)
						break;
				}
			}
			return type;
		}
		private static JSNative CreateJSNative(XElement element) {
			var value = new JSNative();
			foreach (var child in element.Elements()) {
				switch (child.Name.LocalName) {
					case "ignore": value.Ignore = true; break;
					case "opCode": value.OpCode = child.Attribute("value").Value; break;
					case "code":
						value.Code = child.Value;
						SetSyntax(value, child);
						break;
					case "additionalCode": value.AdditionalCode = child.Value; break;
					case "method": {
							var name = child.Attribute("name");
							if (name != null) {
								value.Name = name.Value;
							}
							SetSyntax(value, child);
						}
						break;
				}
			}
			return value;
		}
		private static void SetSyntax(JSNative value, XElement child) {
			var syntax = child.Attribute("syntax");
			if (syntax != null) {
				switch (syntax.Value) {
					case "instance": value.CallType = NativeCallType.Instance; break;
					case "static": value.CallType = NativeCallType.Static; break;
				}
			}
		}
		private void HandleProperty(Type type, XElement element) {
			var parameters = element.Element(NS + "parameters");
			var property = parameters == null ?
				type.GetProperty(element.Attribute("name").Value) :
				type.GetProperty(element.Attribute("name").Value, parameters.Elements().Select<XElement, Type>(GetParameterType).ToArray());
			XElement child;
			if ((child = element.Element(NS + "get")) != null) this.RegisterDotNetNative(property.GetGetMethod(), CreateJSNative(child.Element(NS + "javascript")));
			if ((child = element.Element(NS + "set")) != null) this.RegisterDotNetNative(property.GetSetMethod(), CreateJSNative(child.Element(NS + "javascript")));
		}
		private void HandleNamespace(string ns, string assembly, IEnumerable<XElement> elements) {
			foreach (var element in elements) {
				switch (element.Name.LocalName) {
					case "namespace": {
							var tmp = element.Attribute("assembly");
							this.HandleNamespace(ns + "." + element.Attribute("name").Value, tmp == null ? assembly : tmp.Value, element.Elements());
						} break;
					case "type": {
							var assemblyAttribute = element.Attribute("assembly");
							var assemblyValue =
								assemblyAttribute != null ? assemblyAttribute.Value :
								assembly != null ? assembly :
								null;
							string assemblyRealValue;
							if (assemblyValue == null) {
								assemblyRealValue = null;
							} else {
								if (!this.assemblyMapping.TryGetValue(assemblyValue, out assemblyRealValue)) {
									assemblyRealValue = assemblyValue;
								}
							}

							var type = Type.GetType(
								assemblyRealValue != null ?
									(ns + "." + element.Attribute("name").Value + ", " + assemblyRealValue) :
								(ns + "." + element.Attribute("name").Value)
							);
							this.RegisterDotNetNative(type);
							foreach (var memberElement in element.Elements()) {
								switch (memberElement.Name.LocalName) {
									case "constructor":
										this.RegisterDotNetNative(
											type.GetConstructor(memberElement.Element(NS + "parameters").Elements().Select<XElement, Type>(GetParameterType).ToArray()),
											CreateJSNative(memberElement.Element(NS + "javascript"))
											);
										break;
									case "property": this.HandleProperty(type, memberElement); break;
									case "method": this.HandleMethod(type, memberElement); break;
									case "operator": this.HandleOperator(type, memberElement); break;
								}
							}
						}
						break;
				}
			}
		}
		private Dictionary<string, string> assemblyMapping;

		private void HandleMethod(Type type, XElement element) {
			var parameters = element.Element(NS + "parameters");
			var method = parameters == null ?
				type.GetMethod(element.Attribute("name").Value) :
				type.GetMethod(element.Attribute("name").Value, parameters.Elements().Select<XElement, Type>(GetParameterType).ToArray());
			this.RegisterDotNetNative(method, CreateJSNative(element.Element(NS + "javascript")));
		}
		private void HandleOperator(Type type, XElement element) {
			var parameters = element.Element(NS + "parameters");
			var method = parameters == null ?
				type.GetMethod("op_" + element.Attribute("name").Value) :
				type.GetMethod("op_" + element.Attribute("name").Value, parameters.Elements().Select<XElement, Type>(GetParameterType).ToArray());
			this.RegisterDotNetNative(method, CreateJSNative(element.Element(NS + "javascript")));
		}
		private void LoadSystemMapping(string mappingXmlPath) {

			var document = XDocument.Load(mappingXmlPath);
			var schemaSet = new XmlSchemaSet();
			schemaSet.Add(NS.NamespaceName, AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Mapping.xsd");
			StringBuilder errorMessage = new StringBuilder();
			document.Validate(schemaSet, (o, e) => {
				errorMessage.Append(e.Message);
			});
			if (errorMessage.Length > 0) {
				ThrowHelper.Throw("Invalid Mapping.xml: " + errorMessage.ToString());
			}
			this.assemblyMapping = new Dictionary<string, string>();
			foreach (var element in document.Root.Elements()) {
				switch (element.Name.LocalName) {
					case "assembly": this.assemblyMapping[element.Attribute("name").Value] = element.Attribute("fullName").Value; break;
					case "namespace": {
							var tmp = element.Attribute("assembly");
							this.HandleNamespace(element.Attribute("name").Value, tmp == null ? null : tmp.Value, element.Elements());
						}
						break;
				}
			}
			this.assemblyMapping = null;
		}
		#endregion
	}

	public class JSPageCompiler : AssemblyCompiler {
		protected override MethodCompiler CreateMethodCompiler(MethodBaseData method) {
			return new MethodCompiler(this, method, method.Method);
		}
		protected override void AppendAssemblyHeader(StringBuilder sb) {
			sb.Append("//powered by http://smp.if.uj.edu.pl/il2js\n");
		}
	}
}
