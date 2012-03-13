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
//#define COMPILATION_LOG
#define DEGEN
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using MK.JavaScript.CodeDom.Compiler;
using MK.JavaScript.Reflection;
using MK.JavaScript.Framework.Mappings;

namespace MK.CodeDom.Compiler {
	internal static class TypeExtensions {
		public static IEnumerable<Type> GetDeclaringTypesWithType(this Type type) {
			yield return type;
			if (type.DeclaringType != null) {
				foreach (var t in type.DeclaringType.GetDeclaringTypesWithType()) {
					yield return t;
				}
			}
		}

	}

	internal static class Validation {
		[Conditional("VALIDATION")]
		public static void Validate(Func<string> validator) {
			var errorMessage = validator();
			if (errorMessage != null)
				Error(errorMessage);
		}
		[Conditional("VALIDATION")]
		public static void Error(string format, params object[] args) {
			ThrowHelper.Throw(format, args);
		}
	}
	public class MemberInfoData {
		public int Index;
		public int HitCount = 1;
		public readonly MemberInfo Member;

		public override string ToString() {
			return this.GetType().Name + "[" + (this.Member == null ? "" : this.Member.GetSignature()) + "]";
		}

		public MemberInfoData(MemberInfo member, int index) {
			this.Member = member;
			this.Index = index;
		}

	}
	public class VirtualMethodCallData {
		public bool WasCalledAsVirtual = false;
	}
	public class TypeData : MemberInfoData {
		public IEnumerable<TypeData> GetBaseTypes() {
			if (this.BaseTypeData == null) return new TypeData[] { };
			else return this.BaseTypeData.GetBaseTypes().Concat(new[] { this.BaseTypeData });
		}
		public IEnumerable<TypeData> GetSubTypesTree() {
			foreach (var type in this.SubTypes) {
				yield return type;
				foreach (var item in type.GetSubTypesTree()) {
					yield return item;
				}
			}
		}

		public MethodBaseData StaticConstructor;
		public Dictionary<MethodBaseData, VirtualMethodCallData> VirtualMethods = new Dictionary<MethodBaseData, VirtualMethodCallData>();
		public Dictionary<int, int> Interface2VirtualMap = new Dictionary<int, int>();
		public HashSet<TypeData> SubTypes = new HashSet<TypeData>();
		public TypeData BaseTypeData;

		public string Name { get { return this.Index.Encode(); } }

		public TypeMeta Meta { get { return TypeMeta.Get(this.Type); } }

		public bool IsinstUsed = false;


		public Type Type { get { return (Type)this.Member; } }
		public TypeData(Type type, int index) : base(type, index) { }
	}
	public class MethodBaseData : MemberInfoData {
		public MethodCompiler Compiler;
		public MethodBase Method { get { return (MethodBase)Member; } }

		public MethodBaseData(MethodBase method, int index) : base(method, index) { }

	}
	public class Static_FieldInfoData : MemberInfoData {
		public FieldInfo Field { get { return (FieldInfo)this.Member; } }
		public Static_FieldInfoData(FieldInfo Field, int index) : base(Field, index) { }
	}
	public class NativeData : MemberInfoData {
		public readonly string AdditionalCode;
		public NativeData(int index, string additionalCode)
			: base(null, index) {
			this.AdditionalCode = additionalCode;
		}
	}
	public class StringData : MemberInfoData {
		public readonly string Value;
		public StringData(int index, string value)
			: base(null, index) {
			this.Value = value;
		}
	}
	/// <summary>
	/// <para>tu bedzie wykonywany kod</para>
	/// <para>jest to z zalozenia wszystko co jest kompilowane dla jakiegos topa i jego podramek</para>
	/// </summary>
	public abstract partial class AssemblyCompiler {

		protected readonly Dictionary<Type, TypeData> typeData = new Dictionary<Type, TypeData>();

		/// <param name="type">Non native type.</param>
		public TypeData Resolve(Type type) {
			//if (type.IsGenericType)
			//  type = type.GetGenericTypeDefinition();
			return this.typeData[type];
		}

		internal readonly Dictionary<MethodBase, MethodBaseData> Methods = new Dictionary<MethodBase, MethodBaseData>();
		protected readonly Dictionary<MemberInfo, NativeData> nativeMembers = new Dictionary<MemberInfo, NativeData>();
		protected readonly Dictionary<FieldInfo, Static_FieldInfoData> staticFields = new Dictionary<FieldInfo, Static_FieldInfoData>();
		protected readonly Dictionary<string, StringData> strings = new Dictionary<string, StringData>();
		protected readonly Dictionary<string, StringData> methodCodes = new Dictionary<string, StringData>();

		private int staticConstructorsCount = 0;

		protected readonly Queue<MethodBaseData> methodsToCompile = new Queue<MethodBaseData>();


		protected abstract MethodCompiler CreateMethodCompiler(MethodBaseData method);
		protected virtual void AppendAssemblyHeader(StringBuilder sb) { }
		protected virtual void AppendAssemblyFooter(StringBuilder sb) { }

		internal readonly MK.Collections.Graph<MemberInfoData> Dependencies = new MK.Collections.Graph<MemberInfoData>();

		#region Register

		private void addDependency(MemberInfoData calee, MemberInfoData data) {
			this.Dependencies.AddEdge(calee, data);
			//if (data.Member != null && data.Member.MemberType != MemberTypes.TypeInfo && data.Member.DeclaringType!=null) {
			//  try {
			//    //this.dependencies.AddEdge(this.types[data.Member.DeclaringType], data);
			//  } catch { }
			//}
		}

		public int Register(MemberInfoData callee, FieldInfo member) {
			if (NativesManager.Instance.IsNative(member)) {
				return this.RegisterNative(callee, member);
			} else if (member.IsStatic) {
				Static_FieldInfoData data;
				if (this.staticFields.TryGetValue(member, out data)) {
					++data.HitCount;
				} else {
					data = new Static_FieldInfoData(member, this.staticFields.Count);
					this.staticFields[member] = data;
				}
				this.addDependency(callee, data);
				return data.Index;
			} else {
				return 0;
			}
		}

		public MemberInfoData RegisterFrameType(Type type) {
#warning .. to bylo poto by css'y sie dobrze dodawaly
			var value = new MemberInfoData(null, 0);
			this.Dependencies.AddVertice(value);
			this.Dependencies.AddEdge(value, doRegister(type));
			return value;


			//TypeData data = doRegister(type);
			//this.Dependencies.AddVertice(data);
			//return data;
		}

		public int Register(MemberInfoData callee, MethodBase member) {
			if (NativesManager.Instance.IsNative(member)) {
				return this.RegisterNative(callee, member);
			} else {
				if ((member.IsVirtual || member.IsAbstract) && member.DeclaringType.IsSubclassOf(typeof(MK.JavaScript.Window))) {
					ThrowHelper.Throw("{0} cannot be called as it is virtual or abstract method of subclass of Window.", member.GetSignature());
				}
				MethodBaseData data;
				if (this.Methods.TryGetValue(member, out data)) {
					++data.HitCount;
				} else {
					//zaczynam numeracje metod od 1 a nie od zera... ciekawe kiedy to spowoduje problemy
					//dlaczego tak numeruje? bo if(0) zwraca false w .js
					data = new MethodBaseData(member, this.Methods.Count + 1);
					this.Methods[member] = data;

					if ((member.IsVirtual || member.IsAbstract)) {// && ((MethodInfo)member).GetBaseDefinition() == member) {
						var typeData = this.Resolve(member.DeclaringType);
						VirtualMethodCallData vmcd;
						if (!typeData.VirtualMethods.TryGetValue(data, out vmcd)) {
							typeData.VirtualMethods[data] = new VirtualMethodCallData();
						}
					}
					if (!member.IsAbstract)
						this.methodsToCompile.Enqueue(data);
				}
				if (this.PageCompiler.WindowAttribute.MethodsOnTop
					&& member.DeclaringType.GetDeclaringTypesWithType().Any(t => t == this.PageCompiler.WindowType)) {
					this.addDependency(this.PageCompiler.TypeData, data);
				} if (this.PageCompiler.WindowAttribute.RunAtThis.Any(t => member.DeclaringType.GetDeclaringTypesWithType().Contains(t))) {
					this.addDependency(this.PageCompiler.TypeData, data);
				} else {
					var moduleCompiler = this.GetModuleCompiler(member.DeclaringType);
					if (moduleCompiler != null) {
						this.addDependency(moduleCompiler.TypeData, data);
					} else {
						this.addDependency(callee, data);
					}
				}
				return data.Index;
			}
		}
		public TypeData Register(MemberInfoData callee, Type type) {
			if (NativesManager.Instance.IsNative(type)) {
				//nie potrzebne bo nigdzie sie do tego nie odwoluje
				//this.RegisterNative(type);
				//try {
				//  var native = NativesManager.Instance.GetNative(type);
				//  Console.WriteLine(native.Name ?? type.Name);
				//} catch { }
				return null;
			} else {
				TypeData data = doRegister(type);
				var moduleCompiler = this.GetModuleCompiler(type);
				if (moduleCompiler == null) {
					this.addDependency(callee, data);
				}
				return data;
			}
		}
		public int Register(MemberInfoData callee, string value) {
			StringData data;
			if (!this.strings.TryGetValue(value, out data)) {
				data = new StringData(this.strings.Count, value);
				this.strings[value] = data;
			}
			this.addDependency(callee, data);
			return data.Index;
		}
		public int RegisterCode(MemberInfoData callee, string value) {
			StringData data;
			if (!this.methodCodes.TryGetValue(value, out data)) {
				data = new StringData(this.methodCodes.Count, value);
				this.methodCodes[value] = data;
			}
			this.addDependency(callee, data);
			return data.Index;
		}


		private TypeData doRegister(Type type) {
			if (type.IsGenericType) type = type.GetGenericTypeDefinition();

			TypeData data;
			if (this.typeData.TryGetValue(type, out data)) {
				++data.HitCount;
			} else {
				TypeData baseTypeData;
				if (type.BaseType != null && type.BaseType != typeof(object) && !NativesManager.Instance.IsNative(type.BaseType)) {
					baseTypeData = doRegister(type.BaseType);
				} else {
					baseTypeData = null;
				}

				//typy numerowane od 1. ciekawe kiedy sie to wysypie
				data = new TypeData(type, this.typeData.Count + 1);
				data.BaseTypeData = baseTypeData;
				this.typeData[type] = data;

				if (baseTypeData != null) {
					this.addDependency(data, baseTypeData);
					baseTypeData.SubTypes.Add(data);
				}

				// statyczne konstruktory tworza te enumy. z zalozenia one sa stringami tylko!
				if (!type.IsSubclassOf(typeof(MK.JavaScript.Framework.EnumString))) {
					ConstructorInfo cctor = type.GetConstructor(BindingFlags.Static | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
					if (cctor != null) {
						var native = cctor.GetCustomAttribute<JSNative>();
						if (native == null || !native.Ignore) {
							data.StaticConstructor = new MethodBaseData(cctor, this.staticConstructorsCount++);
							this.addDependency(data, data.StaticConstructor);
							this.methodsToCompile.Enqueue(data.StaticConstructor);
						}
					}
				}
			}
			return data;
		}


		public int RegisterNative(MemberInfoData callee, MemberInfo member) {
			NativeData data;
			if (!this.nativeMembers.TryGetValue(member, out data)) {
				var native = NativesManager.Instance.GetNative(member);
				//w tych przypadkach nie rejestrujemy bo nie ma takiej potrzeby
				if (
					native.Value == null &&
					native.OpCode == null &&
					native.CommandType == NativeCommandType.None &&
					!native.Ignore
				) {
					data = new NativeData(0, native.AdditionalCode == null ? null : Program.PackCode(native.AdditionalCode, true));
					data.Index = native.Code == null ?
						this.Register(data, NativesManager.Instance.Get_native_Value(native, member)) :
						this.RegisterCode(data, Program.PackCode(native.Code, true));
					this.nativeMembers[member] = data;
				} else {
					return 0;
				}
			}
			this.addDependency(callee, data);
			return data.Index;
		}

		#endregion
		#region Resolve

		public string ResolveName(FieldInfo member) {
			if (NativesManager.Instance.IsNative(member)) {
				return this.resolveNative(member);
			} if (member.IsStatic) {
				return this.staticFields[member].Index.Encode();
			} else {
				return this.Resolve(member.DeclaringType).Meta.Fields[member].Token;
			}

		}
		public string ResolveNormalCallName(MethodBase member) {
			return this.Methods[member].Index.Encode();
		}
		private int getVirtualMethodIndex(MethodBase member) {
			return this.Methods[member].Index;
		}
		public string ResolveVirtualCallName(MethodBase member) {
			return this.getVirtualMethodIndex(member).Encode();
		}
		public string ResolveNativeName(MethodBase member) {
			return this.resolveNative(member);
		}
		//public string ResolveName(MethodBase member) {
		//  if (NativesManager.Instance.IsNative(member)) {
		//    return resolveNative(member);
		//  } else if (member.IsAbstract || member.IsVirtual) {
		//    return this.types[member.DeclaringType].Methods[(MethodInfo)member].Index.Encode();
		//  } else {
		//    return this.methods[member].Index.Encode();
		//  }
		//}

		public string ResolveName(Type type) {
			if (NativesManager.Instance.IsNative(type)) {
				return this.resolveNative(type);
			} else {
				return this.Resolve(type).Index.Encode();
			}
		}

		private string resolveNative(MemberInfo member) {
			return this.nativeMembers[member].Index.Encode();
		}
		#endregion

		#region Modules
		private readonly Dictionary<Type, ModuleCompiler> moduleCompilers = new Dictionary<Type, ModuleCompiler>();

		public ModuleCompiler GetModuleCompiler(Type type0) {
			foreach (var type in type0.GetDeclaringTypesWithType()) {
				if (type.IsSubclassOf(typeof(MK.JavaScript.Ajax.Module))) {
					ModuleCompiler value;
					if (!this.moduleCompilers.TryGetValue(type, out value)) {
						value = new ModuleCompiler(this.moduleCompilers.Count, this.Resolve(type));
						value.RuntimeFrameCompiler = this;
						this.moduleCompilers[type] = value;
					}
					return value;
				}
			}
			return null;
		}
		#endregion

		protected void Compile(MethodBaseData data) {
			if (data.Compiler == null) {
				data.Compiler = this.CreateMethodCompiler(data);
				Program.XmlLog.WriteStartElement("method");
				Program.XmlLog.WriteAttributeString("signature", data.Method.GetSignature());
				if (Settings.DebugCompilation) {
					Program.XmlLog.WriteAttributeString("name", data.Method.DeclaringType.FullName + "." + data.Method.Name);
					Program.XmlLog.WriteAttributeString("metadataToken", "0x" + data.Member.MetadataToken.ToString("X"));
					Program.XmlLog.WriteAttributeString("index", data.Index.ToString());
				}
				data.Compiler.Compile();
#if !OBFUSCATE
				Program.XmlLog.WriteStartElement("il");
				Program.XmlLog.WriteAttributeString("value", data.Compiler.Body);
				Program.XmlLog.WriteEndElement();
#endif
				data.Compiler.DumpLogs();
				Program.XmlLog.WriteEndElement();
			}
		}
		/// <summary>
		/// poniewaz frameworkframe jest jakims pageframe wiec (nie chce dziedziczenia) trza wiedziec ktora to by nie dublowac kodu
		/// </summary>
		public PageFrameCompiler PageCompiler;

		public void Compile() {
			Program.XmlLog.WriteStartElement("page");
			Program.XmlLog.WriteAttributeString("path", this.PageCompiler.WindowAttribute.Path);
			Program.XmlLog.WriteAttributeString("class", this.PageCompiler.WindowType.FullName);
			if (Settings.DebugCompilation) {
				Program.XmlLog.WriteAttributeString("metadataToken", "0x" + this.PageCompiler.WindowType.MetadataToken.ToString("X"));
				Program.XmlLog.WriteAttributeString("guid", this.PageCompiler.JsFileName);
			}
			Program.XmlLog.WriteStartElement("methods");
			do {
				while (this.methodsToCompile.Count > 0) {
					this.Compile(this.methodsToCompile.Dequeue());
				}
				foreach (var type in this.typeData.Values) {
					foreach (var _anInterface in type.Type.GetInterfaces()) {
						var anInterface =
#if DEGEN
 _anInterface.IsGenericType ? _anInterface.GetGenericTypeDefinition() :
#endif
 _anInterface;

						TypeData interfaceData;
						if (this.typeData.TryGetValue(anInterface, out interfaceData)) {
							interfaceData.SubTypes.Add(type);
						}
					}
				}
				//dla interfejsow to wyglada bardzo ciekawie...
				foreach (var type in this.typeData.Values) {
					if (type.Type.IsInterface) {
						foreach (var subType in type.GetSubTypesTree().Where(t => !t.Type.IsInterface)) {
							var map = subType.Type.GetInterfaceMap(
#if DEGEN
subType.Type.GetInterfaces().Single(i => i.MetadataToken == type.Type.MetadataToken)
#else
	type.Type
#endif
);
							for (int i = 0; i < map.InterfaceMethods.Length; ++i) {
								MethodBaseData data;
								var interfaceMethod = map.InterfaceMethods[i]
#if DEGEN
.GetGenericIfNecessary()
#endif
;
								if (this.Methods.TryGetValue(interfaceMethod, out data)) {
									if (
										type.VirtualMethods.ContainsKey(data)
										&& map.TargetMethods[i].DeclaringType
#if DEGEN
.MetadataToken
#endif
 == subType.Type
#if DEGEN
.MetadataToken
#endif
										/*
										 * to tez jest niepoprawne poniewaz w sytuacji
										 * 
										 * interface I{
										 * void F();
										 * }
										 * 
										 * class A{
										 * public void F(){}
										 * }
										 * 
										 * class B:A,I{}
										 * 
										 * to nie bedzie dzialac... ale puki co
										 */
									) {
										this.Register(data, map.TargetMethods[i]);
#warning hm... to raczej niepoprawne
										subType.Interface2VirtualMap[this.getVirtualMethodIndex(interfaceMethod)] = this.getVirtualMethodIndex(map.TargetMethods[i].GetBaseDefinition());
									}
								}
							}
						}
					} else {
						foreach (var method in from m in type.VirtualMethods
																	 let mi = (MethodInfo)m.Key.Method
																	 where mi == mi.GetBaseDefinition() && !mi.IsFinal
																	 select m
						) {
							foreach (var subType in type.GetSubTypesTree()) {
								foreach (var subMethod in subType.Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
									if (subMethod.GetBaseDefinition().MetadataToken == method.Key.Method.MetadataToken) {
										this.Register(method.Key, subMethod);
									}
								}
							}
						}
					}
				}
			} while (this.methodsToCompile.Count > 0);
			//this.dependencies.DFSFromSources();
			//this.dependencies.FloydWarshall();
			foreach (var type in this.PageCompiler.WindowAttribute.RunAtThis) {
				TypeData data;
				if (this.typeData.TryGetValue(type, out data)) {
					this.addDependency(this.PageCompiler.TypeData, data);
				}
			}

			Program.XmlLog.WriteEndElement();//methods

			this.dumpLogs();
			
			Program.XmlLog.WriteEndElement();//page
		}

		public const char StringSeparator = '`';
		public const string StringSeparatorString = "`";
		public const string StringSeparatorEscapedString = "\\140";

		private static string IntArray(IEnumerable<int> values) {
			if (values == null) return "0";
			var sb = new StringBuilder();
			sb.Append('"');

			foreach (var item in values) {
				sb.Append(item.Encode()).Append('`');
			}

			if (sb.Length > 1) --sb.Length;
			sb.Append('"');
			return sb.ToString();
		}
		private static string IntArrayAsArray(IEnumerable<int> values) {
			if (values == null) return "0";
			var sb = new StringBuilder();
			sb.Append('[');

			foreach (var item in values) {
				sb.Append(item).Append(',');
			}

			if (sb.Length > 1) --sb.Length;
			sb.Append(']');
			return sb.ToString();
		}
		private static string StringArray(IEnumerable<string> values) {
			var strings = new StringBuilder();
			foreach (var @string in values) {
				if (@string.Contains(StringSeparator)) ThrowHelper.Throw("{0} contains reserved character: {1} .", @string, StringSeparator);
				strings.Append(
					@string//.Replace(StringSeparatorString, StringSeparatorEscapedString)
				).Append(StringSeparator);
			}
			if (strings.Length > 0) {
				--strings.Length;
				return MK.Convert.ToJSString(strings.ToString());
			} else {
				return "\"\"";
			}
		}
		public void GenerateFor(JSFileCompiler jsFileCompiler) {
			var pageCompiler = jsFileCompiler as PageFrameCompiler;

#warning jak mamy modul to mozna by brac mniej w zaleznosci od tego cvo juz jest loadowane przez strony loadujace modul
			/**
			 * 
			 */
			Predicate<MemberInfoData> takeTest = //chodzi oto by brac te i tylko te ktore trza dodac dla danej ramki, jak i tak jest na topie to nie dodaje
				member =>
					(
						this.Dependencies.IsPathFromTo(jsFileCompiler.TypeData, member)
					)
					&& (this.PageCompiler == jsFileCompiler ? true : !this.Dependencies.IsPathFromTo(this.PageCompiler.TypeData, member));

			StringBuilder sb = new StringBuilder();
			this.AppendAssemblyHeader(sb);
			if (jsFileCompiler != this.PageCompiler) {
				sb.Append("top.");
			}
			sb.Append(Program.Names._init_).Append('(').Append(pageCompiler == null ? "0" : "window").Append(',');

			#region $method$ $methodLength$
			//methodDereference nie jest potrzebne poniewaz to jest frameworkowe. methodDereference jest potrzebne tylko jak wolamy natywne!
			var methods = from member in this.Methods.Values
										where takeTest(member)
										//where !member.Method.IsAbstract
										let method = member.Method
										select new {
											member.Index,
											Body = method.IsAbstract ? "" : member.Compiler.Body,
											ParametersLength = method.IsStatic ? method.GetParameters().Length : (method.GetParameters().Length + 1),
											Signature = method.GetSignature(),
										};
			if (methods.Any()) {
				sb
					.Append(IntArray(methods.Select(m => m.Index)))
					.Append(',')
					.Append(StringArray(methods.Select(m => m.Body)))
					.Append(',')
					.Append(IntArrayAsArray(methods.Select(m => m.ParametersLength)))
				;
#if COMPILATION_LOG
				jsFileCompiler.CompilationLog("methods:");
				foreach (var method in methods) {
					jsFileCompiler.CompilationLog(method.Index, method.Signature + "\t" + method.Body);
				}
#endif
			} else {
				sb.Append("0,0,0");
			}
			#endregion
			sb.Append(',');
			#region $string$
			var strings = from member in this.strings.Values
										where takeTest(member)
										select new { member.Index, member.Value };
			if (strings.Any()) {
				sb
					.Append(IntArray(strings.Select(s => s.Index)))
					.Append(',')
					.Append(StringArray(strings.Select(s => s.Value)));
			} else {
				sb.Append("0,0");
			}
			#endregion
			sb.Append(',');
			#region $code$
			var codes = from member in this.methodCodes.Values
									where takeTest(member)
									select new { member.Index, member.Value };
			if (codes.Any()) {
				sb
					.Append(IntArray(codes.Select(s => s.Index)))
					.Append(',')
					.Append(StringArray(codes.Select(s => s.Value)));
			} else {
				sb.Append("0,0");
			}
			#endregion
			sb.Append(',');
			#region $native$.additionalCode
			var additionalCodes = from member in this.nativeMembers.Values
														where takeTest(member)
														where member.AdditionalCode != null
														select member.AdditionalCode
					;
			if (additionalCodes.Any())
				sb.Append(MK.Convert.ToJSString(
					string.Join("",
						(additionalCodes).ToArray())
				));
			else
				sb.Append('0');
			#endregion
			sb.Append(',');
			#region $field$
			var groupedFields = from field in
														(
															from member in this.staticFields.Values
															where takeTest(member)
															select new { member.Index, member.Field.FieldType }
															)
													group field.Index by field.FieldType.IsPrimitive into g
													select g;
			sb
				.Append(IntArray((IEnumerable<int>)(groupedFields.FirstOrDefault(g => g.Key))))
				.Append(',')
				.Append(IntArray((IEnumerable<int>)(groupedFields.FirstOrDefault(g => !g.Key))));
			#endregion
			sb.Append(',');
			#region $ctor$ .ctor
			/*
			 * 
			 * 
			 * kiedy potrzebujemy brac jakis typ?
			 * 
			 * uwagi
			 * *) jak bierzemy jakis typ co go musimy brac to musimy brac jego ancestor'ow by stworzyc caly lancuch
			 * *) jednak nie musimy brac wszystkich ancestorow tylko do najstarszego istotnego (czyli takiego co sam spelnia ponizsze warunki)
			 *
			 * kiedy nie bierzemy [alternatywy]
			 * *) type.IsInterface
			 * 
			 * [alternatywy]
			 * *) jesli ma wolane BAZOWE metody wirtualne
			 * *) jesli jego ancestor ma wolane BAZOWE metody wirtualne
			 * *) jesli jest w wyrazeniu `varible is type`; by dzialalo `varible instanceof type` w JS
			 * *) jesli jakis jego ancestor jest w wyrazeniu `varible is ancestor` (subtype mnie tutaj nie interesuje, poniewaz zeby `varible instanceof ancestor` dzialalo musi byc lancuch w gore do tego ancestora)
			 * 
			 * reasumujac: powinnismy wyjsc od typow co je uzywamy w danej ramce i sprawdzic dla nich pwyzsze warunki
			 * potem brac wszystkich ancestor`ow do najstarszego co spelnia warunki
			 */
#warning czy jest brane to co trza? czy dla interfejsowych rzeczy to cos wystarcza?
			var ctors = (from member in this.typeData.Values
									 /*
									  * ciezko tu optymalizowac - patrz komentarz dla testu ponizej
									  */
									 //where takeTest(member)
									 where this.Dependencies.IsPathFromTo(jsFileCompiler.TypeData, member)
									 where !member.Type.IsInterface
									 where member.VirtualMethods.Count > 0
										 || member.GetBaseTypes().Any(m => m.VirtualMethods.Count > 0)
										 || member.IsinstUsed
										 || member.GetBaseTypes().Any(m => m.IsinstUsed)
									 orderby member.Index
									 select new {
										 member.Index,
										 Signature = member.Type.GetSignature(),
										 BaseTypeIndex = member.BaseTypeData == null ? 0 : member.BaseTypeData.Index,
										 Methods = (from m in member.VirtualMethods.Keys
																let baseDefinition = ((MethodInfo)m.Method).GetBaseDefinition().GetGenericIfNecessary()
																let baseDefinitionData = this.Methods[baseDefinition]
																/* 
																 * to moze byc za malo, bo np na topie jest bray typ bez podtypow i tam jest wolane baza
																 * a na podframie bedzie ten typ ale nie bedzie wolania
																 * i w tedy lipa. zatem ponizsza linijka NIE MOZE byc odkomentowana
																 */
																//where takeTest(this.methods[baseDefinition])
																/*
																 * ale zamiast tego mozna wziasc to:
																 */
																where this.Dependencies.IsPathFromTo(jsFileCompiler.TypeData, baseDefinitionData)
																 || this.Dependencies.IsPathFromTo(this.PageCompiler.TypeData, baseDefinitionData)
																where !m.Method.IsAbstract
																select new {
																	VirtualIndex = this.getVirtualMethodIndex(baseDefinition),
																	MethodIndex = m.Index
																}).Concat(
																from m in member.Interface2VirtualMap
																select new {
																	VirtualIndex = m.Key,
																	MethodIndex = m.Value
																}
															 )
									 });
			if (ctors.Any()) {
				//tu nie ma konstruktorow bo one sa wolane inaczej. w sensie jak jest newobj to on ma konstruktor, a jak jest jsowy konstruktor to zamiast dostac {} do konstruktora dostaje konkretny obiekt
				jsFileCompiler.CompilationLog("types:");
				sb.Append('[');
				foreach (var ctor in ctors) {
					sb.Append(ctor.Index).Append(',').Append(ctor.BaseTypeIndex).Append(',');
					jsFileCompiler.CompilationLog(ctor.Index, ctor.Signature);
				}
				--sb.Length;
				sb.Append("],[");
				foreach (var ctor in ctors) {
					if (ctor.Methods.Any()) {
						sb.Append('{');
						foreach (var method in ctor.Methods) {
							sb.Append(method.VirtualIndex).Append(':').Append(method.MethodIndex).Append(',');
						}
						--sb.Length;
						sb.Append("},");
					} else {
						sb.Append("0,");
					}
				}
				--sb.Length;
				sb.Append(']');
			} else {
				/*
				 * format wyglada nastepujaco
				 * [(index typu,index base(0=>brak)),...],[{index virtuala:index metody},...]
				 * 
				 */
				sb.Append("0,0");
			}
			#endregion
			sb.Append(',');
			#region $cctor$ static constructors .cctor
			var cctors = from member in
										 this.Dependencies.TopologicalInverseOrder((
											 from member in this.typeData.Values
											 where takeTest(member)
											 where member.StaticConstructor != null
											 select member
											).OfType<MemberInfoData>()).OfType<TypeData>()
									 select new {
										 member.Index,
										 member.StaticConstructor.Compiler.Body,
										 member.StaticConstructor.Method
									 };
			if (cctors.Any()) {
				sb
					.Append(IntArray(cctors.Select(c => c.Index)))
					.Append(',')
					.Append(StringArray(cctors.Select(c => c.Body)));
			} else {
				sb.Append("0,0");
			}
			#endregion
			sb.Append(',');
			#region
			if (pageCompiler == null) {
				//module
				sb.Append("0,0");
			} else {
				//page
				sb
					.Append(pageCompiler.CtorMethodIndex)
					.Append(',')
					.Append(pageCompiler.Page_LoadMethodIndex);
			}
			#endregion
			sb.Append(',');
			#region $module$
			var modules = from module in this.moduleCompilers.Values
#warning !!!!!!!!!!!!!!!!!!!! critical
										//where takeTest(module.TypeData)
										where jsFileCompiler == this.PageCompiler
										select new { module.Index, module.JsFileName };
			if (modules.Any()) {
				sb
					.Append(IntArray(modules.Select(s => s.Index)))
					.Append(',')
					.Append(StringArray(modules.Select(s => s.JsFileName)));
			} else {
				sb.Append("0,0");
			}
			#endregion
			sb.Append(");");

			this.AppendAssemblyFooter(sb);

			File.WriteAllText(jsFileCompiler.OutputPath, sb.ToString());
		}
		public void WriteOutputs() {
			foreach (var compiler in this.moduleCompilers.Values) {
				this.GenerateFor(compiler);
			}
		}
	}

	public class JSFileCompiler {
		[Conditional("COMPILATION_LOG")]
		public void CompilationLog(string line) {
			File.AppendAllText(this.MapPath, line + "\n");
		}
		[Conditional("COMPILATION_LOG")]
		public void CompilationLog(int index, string signature) {
			File.AppendAllText(this.MapPath, index + "\t0x" + index.ToString("X") + "\t" + signature + "\n");
		}


		public AssemblyCompiler RuntimeFrameCompiler { get; internal set; }

		/// <summary>
		/// Data of Window or Module type.
		/// </summary>
		public MemberInfoData TypeData { get; protected set; }


		public readonly string JsFileName;
		public string OutputPath { get { return Settings.OutputDirectory + Path.DirectorySeparatorChar + this.JsFileName + ".js"; } }
		public string MapPath { get { return this.OutputPath + ".map"; } }

		public JSFileCompiler(string fileName) {
			this.JsFileName = fileName;
		}
	}
	/// <summary>
	/// odpowiada modulowi
	/// </summary>
	public class ModuleCompiler : JSFileCompiler {
		public readonly int Index;
		public ModuleCompiler(int index, TypeData typeData)
			: base(
#if !OBFUSCATE
"Module=" + new Regex(@"[^a-zA-Z0-9_]", RegexOptions.Compiled).Replace(typeData.Type.Name, "_") + "=" +
#endif
Guid.NewGuid().ToString()
			) {
			this.Index = index;
			this.TypeData = typeData;
		}
	}
	/// <summary>
	/// to odpowiada stronie
	/// </summary>
	public class PageFrameCompiler : JSFileCompiler {

		public readonly Type WindowType;
		public readonly MK.JavaScript.WindowAttribute WindowAttribute;


		public PageFrameCompiler(Type windowType, MK.JavaScript.WindowAttribute windowAttribute)
			: base(
#if !OBFUSCATE
"Page=" + windowAttribute.Path.Replace('/', '_').Replace('~', '_') + "=" +
#endif
Guid.NewGuid().ToString()
			) {
			this.WindowType = windowType;
			this.WindowAttribute = windowAttribute;
		}


		#region Compile utils
		private static IEnumerable<Type> GetAncestors(Type type) {
			if (type.BaseType != typeof(object)) {
				yield return type.BaseType;
				foreach (var item in GetAncestors(type.BaseType)) {
					yield return item;
				}
			}
		}
		private static AssemblyCompiler getAssemblyCompiler(Type type, Dictionary<Type, AssemblyCompiler> assemblyCompilers) {
			AssemblyCompiler ac;
			if (!assemblyCompilers.TryGetValue(type, out ac)) {
				ac = new JSPageCompiler();
				assemblyCompilers[type] = ac;
			}
			return ac;
		}
		#endregion

		public int Page_LoadMethodIndex { get; private set; }
		public int CtorMethodIndex { get; private set; }


		private void compile() {
			//TODO czemu to musi byc tak zakomentowane??
			//if (this.IsRuntimePage) {
			this.RuntimeFrameCompiler.RegisterFrameType(typeof(JSEnumerable<>));
			//}

			this.CompilationLog(this.WindowAttribute.Path);

			this.TypeData = this.RuntimeFrameCompiler.RegisterFrameType(this.WindowType);




			MethodBase methodBase;
			methodBase = this.WindowType.GetConstructor(Type.EmptyTypes);
			if (methodBase != null && methodBase.GetMethodBody().GetILAsByteArray().Length != 7)
				this.CtorMethodIndex = this.RuntimeFrameCompiler.Register(this.TypeData, methodBase);
			methodBase = this.WindowType.GetMethod("Page_Load", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
			if (methodBase != null)
				this.Page_LoadMethodIndex = this.RuntimeFrameCompiler.Register(this.TypeData, methodBase);

		}
		private void WriteOutput() {
			this.RuntimeFrameCompiler.GenerateFor(this);
		}

		public bool IsRuntimePage { get; private set; }

		public static void CompileAll(params PageFrameCompiler[] compilers) {
			//Dictionary<Type, PageFrameCompiler> pageCompilers = compilers.ToDictionary(value => value.WindowType);
			Dictionary<Type, AssemblyCompiler> assemblyCompilers = new Dictionary<Type, AssemblyCompiler>();


#warning jak to zrobic??
			foreach (var compiler in compilers) {
				foreach (var ancestor in GetAncestors(compiler.WindowType)) {
					if (ancestor.FullName.StartsWith("MK.JavaScript.Window`1")) {//czyli jest to cos co ma top'a (pewnie roznego od samego siebie, ale niekoniecznie)
						var topPageType = ancestor.GetGenericArguments()[0];
						compiler.RuntimeFrameCompiler = getAssemblyCompiler(topPageType, assemblyCompilers);
						compiler.IsRuntimePage = compiler.WindowType == topPageType;
						goto done;
					}
				}
				var ac = getAssemblyCompiler(compiler.WindowType, assemblyCompilers);
				compiler.IsRuntimePage = true;
				compiler.RuntimeFrameCompiler = ac;
				ac.PageCompiler = compiler;
			done: ;
			}
			foreach (var compiler in compilers) { compiler.compile(); }
			foreach (var compiler in assemblyCompilers.Values) { compiler.Compile(); }
			foreach (var compiler in assemblyCompilers.Values) { compiler.WriteOutputs(); }
			foreach (var compiler in compilers) { compiler.WriteOutput(); }

		}


	}
}
