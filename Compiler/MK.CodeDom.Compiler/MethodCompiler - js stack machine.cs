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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using MK.JavaScript;
using MK.JavaScript.Ajax;
using MK.JavaScript.CodeDom.Compiler;
using MK.JavaScript.Framework;
using MK.JavaScript.Reflection;


namespace MK.CodeDom.Compiler {

	internal static class UIntExtension {
		public static string Encode(this int x) {
			return Encode((long)x);
		}
		public static string Encode(this uint x) {
			return Encode((long)x);
		}
		public static string Encode(this long x) {
			return x >= 0 ? x.ToString("x") : ("-" + (-x).ToString("x"));
			//niestety inne jest odczuwalnie wolniejsze
			//var value = x.ToString("x");
			//var terminal = value[value.Length - 1];
			//if (terminal < 'a') {
			//  terminal = (char)(terminal - '0' + 'G');
			//} else {
			//  terminal = (char)(terminal - 'a' + 'A');
			//}
			//return value.Length > 1 ? (value.Substring(0, value.Length - 1) + terminal.ToString()) : terminal.ToString();
		}
	}

	public static class CustomAttributeProvider {
		public static T GetCustomAttribute<T>(this ICustomAttributeProvider self) where T : Attribute {
			var array = self.GetCustomAttributes(typeof(T), false);
			return array.Length == 1 ? (T)array[0] : null;

		}

	}

	[global::System.Serializable]
	public class CompilationException : Exception {
		public CompilationException() { }
		public CompilationException(string message) : base(message) { }
		public CompilationException(string message, Exception inner) : base(message, inner) { }
		protected CompilationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
	internal static class ThrowHelper {
		internal static class Method {
			internal static void Throw(MethodBase method, int position, string message) {
				ThrowHelper.Throw("Error in {2} position {1}: {0}.", message, position, method.GetSignature());
			}
			internal static void Throw(MethodBase method, int position, string format, params object[] args) {
				Throw(method, position, string.Format(format, args));
			}
		}
		internal static void Throw(string errorMessage, params object[] args) {
			Throw(string.Format(errorMessage, args));
		}
		internal static void Throw(string errorMessage) {
			Console.Error.WriteLine(errorMessage);
			throw new CompilationException(errorMessage);
		}

	}

	partial class MethodCompiler {

		private static MethodInfo Thread_CurrentThread = typeof(System.Threading.Thread).GetProperty("CurrentThread").GetGetMethod();
		private static ConstructorInfo Object_ctor = typeof(object).GetConstructors()[0];

		protected StringBuilder output = new StringBuilder();

		protected virtual void OnToken(OpCodeValue opCode) { }


		private void HandleOpCode(OpCode opCode) {
			var opCodeValue = (OpCodeValue)opCode.Value;
			this.OnToken(opCodeValue);
			switch (opCodeValue) {
				#region Ldc
				case OpCodeValue.Ldc_I4: Ldc_I((int)this.EatToken().Value); break;
				case OpCodeValue.Ldc_I8: Ldc_I((long)this.EatToken().Value); break;
				case OpCodeValue.Ldc_I4_S: Ldc_I((long)(sbyte)this.EatToken().Value); break;
				case OpCodeValue.Ldc_I4_M1: Ldc_I(-1); break;
				case OpCodeValue.Ldc_I4_0: Ldc_I(0); break;
				case OpCodeValue.Ldc_I4_1: Ldc_I(1); break;
				case OpCodeValue.Ldc_I4_2: Ldc_I(2); break;
				case OpCodeValue.Ldc_I4_3: Ldc_I(3); break;
				case OpCodeValue.Ldc_I4_4: Ldc_I(4); break;
				case OpCodeValue.Ldc_I4_5: Ldc_I(5); break;
				case OpCodeValue.Ldc_I4_6: Ldc_I(6); break;
				case OpCodeValue.Ldc_I4_7: Ldc_I(7); break;
				case OpCodeValue.Ldc_I4_8: Ldc_I(8); break;
				#endregion
				#region op_
				case OpCodeValue.Endfinally: Endfinally(); break;
				case OpCodeValue.Throw:
				case OpCodeValue.Rethrow:
					Throw();
					break;

				case OpCodeValue.Add: Add(); break;
				case OpCodeValue.And: And(); break;
				case OpCodeValue.Or: Or(); break;
				case OpCodeValue.Mul: Mul(); break;
				case OpCodeValue.Div: Div(); break;
				case OpCodeValue.Cgt: Cgt(); break;
				case OpCodeValue.Clt: Clt(); break;
				case OpCodeValue.Ceq: Ceq(); break;
				case OpCodeValue.Sub: Sub(); break;
				case OpCodeValue.Xor: Xor(); break;
				#endregion
				#region Stloc
				case OpCodeValue.Stloc: this.Stloc((ushort)this.EatToken().Value); break;
				case OpCodeValue.Stloc_S: this.Stloc((byte)this.EatToken().Value); break;
				case OpCodeValue.Stloc_0: this.Stloc(0); break;
				case OpCodeValue.Stloc_1: this.Stloc(1); break;
				case OpCodeValue.Stloc_2: this.Stloc(2); break;
				case OpCodeValue.Stloc_3: this.Stloc(3); break;
				#endregion
				#region Ldloc
				case OpCodeValue.Ldloca:
				case OpCodeValue.Ldloca_S: this.Ldloca(int.Parse(this.EatToken().Value.ToString())); break;

				case OpCodeValue.Ldloc: this.Ldloc(int.Parse(this.EatToken().Value.ToString())); break;
				case OpCodeValue.Ldloc_S: this.Ldloc((byte)this.EatToken().Value); break;
				case OpCodeValue.Ldloc_0: this.Ldloc(0); break;
				case OpCodeValue.Ldloc_1: this.Ldloc(1); break;
				case OpCodeValue.Ldloc_2: this.Ldloc(2); break;
				case OpCodeValue.Ldloc_3: this.Ldloc(3); break;
				#endregion
				#region Ldarg
				case OpCodeValue.Ldarga:
				case OpCodeValue.Ldarga_S: this.Ldarga(int.Parse(this.EatToken().Value.ToString())); break;

				case OpCodeValue.Ldarg: Ldarg((int)this.EatToken().Value); break;
				case OpCodeValue.Ldarg_S: Ldarg((int)(byte)this.EatToken().Value); break;
				case OpCodeValue.Ldarg_0: Ldarg(0); break;
				case OpCodeValue.Ldarg_1: Ldarg(1); break;
				case OpCodeValue.Ldarg_2: Ldarg(2); break;
				case OpCodeValue.Ldarg_3: Ldarg(3); break;
				#endregion
				#region Starg
				case OpCodeValue.Starg: Starg((int)this.EatToken().Value); break;
				case OpCodeValue.Starg_S: Starg((int)(byte)this.EatToken().Value); break;
				#endregion
				#region Stind
				case OpCodeValue.Stind_Ref: this.Stind(typeof(void*)); break;
				case OpCodeValue.Stind_I: this.Stind(typeof(int)); break;
				case OpCodeValue.Stind_I1: this.Stind(typeof(SByte)); break;
				case OpCodeValue.Stind_I2: this.Stind(typeof(Int16)); break;
				case OpCodeValue.Stind_I4: this.Stind(typeof(Int32)); break;
				case OpCodeValue.Stind_I8: this.Stind(typeof(Int64)); break;
				case OpCodeValue.Stind_R4: this.Stind(typeof(Single)); break;
				case OpCodeValue.Stind_R8: this.Stind(typeof(Double)); break;
				#endregion
				#region Ldind
				case OpCodeValue.Ldind_I: this.Ldind(typeof(int)); break;
				case OpCodeValue.Ldind_I1: this.Ldind(typeof(SByte)); break;
				case OpCodeValue.Ldind_I2: this.Ldind(typeof(Int16)); break;
				case OpCodeValue.Ldind_I4: this.Ldind(typeof(Int32)); break;
				case OpCodeValue.Ldind_I8: this.Ldind(typeof(Int64)); break;
				case OpCodeValue.Ldind_R4: this.Ldind(typeof(Single)); break;
				case OpCodeValue.Ldind_R8: this.Ldind(typeof(Double)); break;
				case OpCodeValue.Ldind_Ref: this.Ldind(typeof(void*)); break;
				case OpCodeValue.Ldind_U1: this.Ldind(typeof(Byte)); break;
				case OpCodeValue.Ldind_U2: this.Ldind(typeof(UInt16)); break;

				case OpCodeValue.Ldobj: this.Ldind((Type)this.EatToken().Value); break;
				#endregion
				#region Ldelem
				case OpCodeValue.Ldelema: this.Ldelema((Type)this.EatToken().Value); break;

				case OpCodeValue.Ldelem: this.Ldelem((Type)this.EatToken().Value); break;
				case OpCodeValue.Ldelem_U1: this.Ldelem(typeof(Byte)); break;
				case OpCodeValue.Ldelem_U2: this.Ldelem(typeof(UInt16)); break;
				case OpCodeValue.Ldelem_U4: this.Ldelem(typeof(UInt32)); break;
				case OpCodeValue.Ldelem_I: this.Ldelem(typeof(/*native*/int)); break;
				case OpCodeValue.Ldelem_I1: this.Ldelem(typeof(Byte)); break;
				case OpCodeValue.Ldelem_I2: this.Ldelem(typeof(Int16)); break;
				case OpCodeValue.Ldelem_I4: this.Ldelem(typeof(Int32)); break;
				case OpCodeValue.Ldelem_I8: this.Ldelem(typeof(Int64)); break;
				case OpCodeValue.Ldelem_R4: this.Ldelem(typeof(Single)); break;
				case OpCodeValue.Ldelem_R8: this.Ldelem(typeof(Double)); break;
				case OpCodeValue.Ldelem_Ref: this.Ldelem(typeof(/*O*/void)); break;
				#endregion
				#region Stelem
				case OpCodeValue.Stelem: Stelem((Type)this.EatToken().Value); break;
				case OpCodeValue.Stelem_I: Stelem(typeof(int)); break;
				case OpCodeValue.Stelem_I1: Stelem(typeof(sbyte)); break;
				case OpCodeValue.Stelem_I2: Stelem(typeof(short)); break;
				case OpCodeValue.Stelem_I4: Stelem(typeof(int)); break;
				case OpCodeValue.Stelem_I8: Stelem(typeof(long)); break;
				case OpCodeValue.Stelem_R4: Stelem(typeof(float)); break;
				case OpCodeValue.Stelem_R8: Stelem(typeof(double)); break;
				case OpCodeValue.Stelem_Ref: Stelem(typeof(IntPtr)); break;
				#endregion
				case OpCodeValue.Pop: Pop(); break;
				case OpCodeValue.Ldnull: Ldnull(); break;
				case OpCodeValue.Newarr: Newarr((Type)this.EatToken().Value); break;
				case OpCodeValue.Dup: this.Dup(); break;
				case OpCodeValue.Ldlen: Ldlen(); break;
				case OpCodeValue.Ldstr: Ldstr(this.AssemblyCompiler.Register(this.CalleeData, (string)this.EatToken().Value)); break;
				case OpCodeValue.Ldc_R4: Ldc_R((float)this.EatToken().Value); break;
				case OpCodeValue.Ldc_R8: Ldc_R((double)this.EatToken().Value); break;
				#region Branch
				case OpCodeValue.Brtrue:
				case OpCodeValue.Brtrue_S:
					this.Brtrue(int.Parse(this.EatToken().Value.ToString()));
					break;
				case OpCodeValue.Brfalse:
				case OpCodeValue.Brfalse_S:
					this.Brfalse(int.Parse(this.EatToken().Value.ToString()));
					break;
				case OpCodeValue.Br:
				case OpCodeValue.Br_S:
					this.Br(int.Parse(this.EatToken().Value.ToString()));
					break;
#warning #error w try'u moze byc kilka leaveow, leavey moga byc od tak, poza wszelkim blokiem....
				case OpCodeValue.Leave:
				case OpCodeValue.Leave_S:
					this.Leave(int.Parse(this.EatToken().Value.ToString()));
					break;
				case OpCodeValue.Ble:
				case OpCodeValue.Ble_S:
					this.Ble(int.Parse(this.EatToken().Value.ToString()));
					break;
				case OpCodeValue.Bge:
				case OpCodeValue.Bge_S:
					this.Bge(int.Parse(this.EatToken().Value.ToString()));
					break;
				case OpCodeValue.Bgt:
				case OpCodeValue.Bgt_S:
					this.Bgt(int.Parse(this.EatToken().Value.ToString()));
					break;
				case OpCodeValue.Beq:
				case OpCodeValue.Beq_S:
					this.Beq(int.Parse(this.EatToken().Value.ToString()));
					break;
				case OpCodeValue.Blt:
				case OpCodeValue.Blt_S:
					this.Blt(int.Parse(this.EatToken().Value.ToString()));
					break;
				case OpCodeValue.Bne_Un:
				case OpCodeValue.Bne_Un_S:
					this.Bne(int.Parse(this.EatToken().Value.ToString()));
					break;
				#endregion
				case OpCodeValue.Switch: this.Switch(); break;
				case OpCodeValue.Stfld: this.onStfld((FieldInfo)this.EatToken().Value); break;
				case OpCodeValue.Ldflda: {
						var info = (FieldInfo)this.EatToken().Value;
						this.AssemblyCompiler.Register(this.CalleeData, info.DeclaringType);
						this.AssemblyCompiler.Register(this.CalleeData, info);
						if (NativesManager.Instance.IsNative(info))
							this.Ldflda_native(info);
						else
							this.Ldflda(info);

					}
					break;
				case OpCodeValue.Initobj: {
						var type = (Type)this.EatToken().Value;
						if (type.IsValueType) ThrowHelper.Throw("OpCodes.Initobj is supported only for reference types");
						this.Ldnull();
						this.Stind(typeof(void*));
					}
					break;

				//case OpCodeValue.Ldflda: {
				//    var position = this.Position;
				//    var info = (FieldInfo)this.EatToken().Value;
				//    if (info.FieldType.IsValueType) ThrowHelper.Throw_UnsupportedCode(opCode.Name, position);
				//    var next = this.EatToken().Value;//OpCode.Initobj
				//    if (!(next is OpCode)) ThrowHelper.Throw_UnsupportedCode(opCode.Name, position);
				//    if (((OpCode)next).Value != (short)OpCodeValue.Initobj) ThrowHelper.Throw_UnsupportedCode(opCode.Name, position);
				//    this.EatToken();//typeof(info.GetValue(?))
				//    this.Ldnull();
				//    this.onStfld(info);
				//  } break;
				case OpCodeValue.Ldfld: {
						var info = (FieldInfo)this.EatToken().Value;
						this.AssemblyCompiler.Register(this.CalleeData, info.DeclaringType);
						this.AssemblyCompiler.Register(this.CalleeData, info);

						if (NativesManager.Instance.IsNative(info))
							Ldfld_native(info);
						else
							Ldfld(info);
					}
					break;
				case OpCodeValue.Stsfld: {
						var info = (FieldInfo)this.EatToken().Value;
						this.AssemblyCompiler.Register(this.CalleeData, info.DeclaringType);
						this.AssemblyCompiler.Register(this.CalleeData, info);
						if (NativesManager.Instance.IsNative(info))
							Stsfld_native(info);
						else
							Stsfld(info);
					}
					break;
				//case OpCodeValue.Ldsflda:
				case OpCodeValue.Ldsfld: {
						var info = (FieldInfo)this.EatToken().Value;
						if (info.DeclaringType.IsSubclassOf(typeof(EnumString))) {
							this.Ldstr(this.AssemblyCompiler.Register(this.CalleeData, (string)typeof(EnumString).GetField("Value", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.GetValue(null))));
							break;
						}


						this.AssemblyCompiler.Register(this.CalleeData, info.DeclaringType);
						this.AssemblyCompiler.Register(this.CalleeData, info);
						if (NativesManager.Instance.IsNative(info)) {
							var native = NativesManager.Instance.GetNative(info);
							if (native.Value != null) {
								if (native.Value.GetType() == typeof(string)) {
									Ldstr(this.AssemblyCompiler.Register(this.CalleeData, (string)native.Value));
								} else if (native.Value.GetType() == typeof(int)) {
									Ldc_I((int)native.Value);
								} else {
									this.Throw("Unsupported value type: " + native.Value.GetType());
								}
							} else {
								Ldsfld_native(info);
							}
						} else
							Ldsfld(info);
					} break;
				case OpCodeValue.Call: {
						var info = NativesManager.Instance.ResolveMapping((MethodBase)this.EatToken().Value);

						if (info == Thread_CurrentThread) {
							_eval("t");
							break;
						} else if (info == Object_ctor) {
							break;
						}

						if (Resources.IsResourceClass(info.DeclaringType)) {
							int resourcesIndex, memberIndex;
							Resources.Resolve(info, out resourcesIndex, out memberIndex);
							this.Ldc_I(resourcesIndex);
							this.Ldc_I(memberIndex);
							this.OnCall(Resources.LoadResource);
						} else if (info.DeclaringType == typeof(Server)) {
							var method = (MethodInfo)info;
							switch (method.Name) {
								case "AsyncCall":
									if (method.IsGenericMethod) {
										if (method.GetParameters().Last().ParameterType == typeof(Action<>).MakeGenericType(new[] { method.GetGenericArguments().Last() })) {
											_callServerAsynchronous(method.GetParameters().Length - 2, true);
										} else {
											_callServerAsynchronous(method.GetParameters().Length - 1, false);
										}
									} else {
										_callServerAsynchronous(0, false);
									}
									break;
								case "Call":
									_callServerSynchronous(method.GetParameters().Length - 1, method.ReturnParameter.ParameterType != typeof(void));
									break;
								default:
									this.OnCall(info);
									break;
							}

						} else {
							this.OnCall(info);
						}
					} break;
				case OpCodeValue.Callvirt: {
						var info = NativesManager.Instance.ResolveMapping((MethodBase)this.EatToken().Value);

						if (info.Name == "Invoke" && info.DeclaringType.IsSubclassOf(typeof(Delegate))) {
							this._cl(info.GetParameters().Length);
							break;
						}


						if (this.Call_common_(ref info)) break;

#warning co jest??
						this.AssemblyCompiler.Register(this.CalleeData, info);

						if (NativesManager.Instance.IsNative(info)) {
							if (!this.handleNativeCall(info)) {
								if (info.IsVirtual)
									Callvirt_native((MethodInfo)info);
								else
									Call_native(info);
							}
						} else {
							if (info.IsVirtual)
								Callvirt((MethodInfo)info);
							else {
								Call(info);
							}
						}
					}
					break;
				case OpCodeValue.Ldftn: {
						var info = (MethodInfo)this.EatToken().Value;
						RunAtAttribute runAt = info.GetCustomAttribute<RunAtAttribute>();
						if (runAt != null && (runAt.RunAt & RunAt.Server) == RunAt.Server) {
							this.Ldc_I(ServerManager.Register(info, runAt));
							break;
						}
						//#warning wczesniej tu nie bylo ponizszej linijki
						//            this.AssemblyCompiler.Register(this.CalleeData, info.DeclaringType);

						this.AssemblyCompiler.Register(this.CalleeData, info);
						if (NativesManager.Instance.IsNative(info))
							Ldftn_native(info);
						else
							Ldftn(info);
					} break;
				case OpCodeValue.Newobj: {
						var info = (ConstructorInfo)this.EatToken().Value;

						//Inline inline = info.GetCustomAttribute<Inline>();
						//if (!NativesManager.Instance.IsNative(info) && (inline == null ? InlineAll : inline.Value)) {
						//  try {
						//    var compiler = new JSILInlineMethodCompiler(info);
						//    compiler.Compile();
						//    this.output.Append(compiler.output.ToString());
						//    Program.Log("inlined[method=\"{0}\"]",info.GetSignature());
						//  } catch (InlineException e) {
						//  }
						//  break;
						//}

						if (info.DeclaringType.IsSubclassOf(typeof(Delegate)) && info.DeclaringType != typeof(JSEventHandler)) {
							if (info.GetParameters()[0].ParameterType == typeof(object)
								&& info.GetParameters()[1].ParameterType == typeof(IntPtr)) {
								this.AssemblyCompiler.Register(this.CalleeData, FrameworkUtil._delegate_);
								Newobj_native(FrameworkUtil._delegate_);
							} else {
								this.Throw("Unsupported delegate constructor: {0}.", info.GetSignature());
							}
						} else {
							//var body = info.GetMethodBody();
							//if (body == null) {
							//  //Console.WriteLine(info.GetSignature() + " has no body.");
							//} else {
							//  var il = body.GetILAsByteArray();
							//  if (il.Length == 2 && il[0] == (byte)OpCodeValue.Ldarg_0 && il[1] == (byte)OpCodeValue.Ret) {
							//    Console.WriteLine(info.GetSignature() + " has empty body.");
							//  }
							//}


							//nie mozna zrobic call'a na to poniewaz nie wiadomo gdzie wsadzic w stos new'a... znaczy wiadomo ale to jest niepotrzebne

							//this.OnCall(info);
							this.AssemblyCompiler.Register(this.CalleeData, info.DeclaringType);

							if (NativesManager.Instance.IsNative(info)) {
								if (!NativesManager.Instance.GetNative(info).Ignore) {
									this.AssemblyCompiler.Register(this.CalleeData, info);
									this.Newobj_native(info);
								}
							} else {
								//ponizsza linijka nie moze byc poniewaz konstruktor moze brac argumenty dla sciemy.. i trza je popnac
								//if (IsEmptyConstructor(info)) break;

								this.AssemblyCompiler.Register(this.CalleeData, info);
								var moduleCompiler = this.AssemblyCompiler.GetModuleCompiler(info.DeclaringType);
								if (moduleCompiler != null) {
									this.LoadModule(moduleCompiler.Index);
								}
								this.Newobj(info);
							}
						}
					} break;


				case OpCodeValue.Ret:
					if (this.Position + 1 < this.ilTokens.Length) {
						this.Ret();
					}
					break;
				case OpCodeValue.Rem:
					this.Rem();
					break;
				case OpCodeValue.Isinst: {
						var type = (Type)this.EatToken().Value;
						this.AssemblyCompiler.Register(this.CalleeData, type);
						this.Isinst(type);
					}
					break;
				#region Conv_I:
				case OpCodeValue.Conv_I1: this.Conv_I(sizeof(sbyte)); break;
				case OpCodeValue.Conv_I2: this.Conv_I(sizeof(short)); break;
				case OpCodeValue.Conv_I:
				case OpCodeValue.Conv_I4: this.Conv_I(sizeof(int)); break;
				case OpCodeValue.Conv_I8: this.Conv_I(sizeof(long)); break;
				#endregion
				#region Conv_U:
				case OpCodeValue.Conv_U1: this.Conv_U(sizeof(byte)); break;
				case OpCodeValue.Conv_U2: this.Conv_U(sizeof(ushort)); break;
				case OpCodeValue.Conv_U:
				case OpCodeValue.Conv_U4: this.Conv_U(sizeof(uint)); break;
				case OpCodeValue.Conv_U8: this.Conv_U(sizeof(ulong)); break;
				#endregion
				case OpCodeValue.Volatile:// No code required since read/write operation in JS are not optimized for non volatile varibles.
				case OpCodeValue.Nop:// No code required.
				case OpCodeValue.Break:// No debugger provided that will use this.
					break;
				case OpCodeValue.Constrained: this.Constrained((Type)this.EatToken().Value); break;


				#region ...Ignored...
				case OpCodeValue.Box: {
						var type = (Type)this.EatToken().Value;
						//*
						if (type.IsPrimitive) {/*/
						if (type == typeof(char)) {//*/
							this.Box(type);
						} else {
							Program.XmlLog.WriteElementString("warning", string.Format("Ignored OpCode: {0} <{1}>.", opCodeValue, type));
						}
					}
					break;
				case OpCodeValue.Unbox_Any: {
						var type = (Type)this.EatToken().Value;
						//*
						if (type.IsPrimitive) {/*/
						if (type == typeof(char)) {//*/
							this.Unbox_Any(type);
						} else {
							Program.XmlLog.WriteElementString("warning", string.Format("Ignored OpCode: {0} <{1}>.", opCodeValue, type));
						}
					}
					break;
				case OpCodeValue.Unbox:
				case OpCodeValue.Castclass: {
						var type = (Type)this.EatToken().Value;
						Program.XmlLog.WriteElementString("warning", string.Format("Ignored OpCode: {0} <{1}>.", opCodeValue, type));
					}
					break;
				case OpCodeValue.Conv_R4:
				case OpCodeValue.Conv_R8:
					Program.XmlLog.WriteElementString("warning", string.Format("Ignored OpCode: {0}.", opCodeValue));
					break;
				#endregion

#warning Shl,Shr - treats as integer
				case OpCodeValue.Shl:
				case OpCodeValue.Shr:

				#region TODO
#warning not supportes opcode
				case OpCodeValue.Jmp:
				case OpCodeValue.Ldind_U4:
				case OpCodeValue.Neg:
				case OpCodeValue.Not:
				case OpCodeValue.Cpobj:
				case OpCodeValue.Ldsflda:
				case OpCodeValue.Stobj:
				case OpCodeValue.Refanyval:
				case OpCodeValue.Ckfinite:
				case OpCodeValue.Mkrefany:
				case OpCodeValue.Ldtoken:
				case OpCodeValue.Arglist:
				case OpCodeValue.Endfilter:
				case OpCodeValue.Tailcall:
				case OpCodeValue.Refanytype:

				#endregion

				#region Not supported codes - won't be supported! (jest na wiki)
				// No JavaScript equivalent
				case OpCodeValue.Ldvirtftn:
				case OpCodeValue.Sizeof:
				case OpCodeValue.Calli:
				case OpCodeValue.Readonly:
				case OpCodeValue.Cpblk:
				case OpCodeValue.Initblk:
				case OpCodeValue.Localloc:
				case OpCodeValue.Unaligned:
				#region ..., when comparing unsigned integer values or unordered float values.
				//poniewaz musialbym umiec konwertowac na unordered float'a a nie rozumiem oco chodzi. pomijam kwestje konwertowanie do uint i byte bo jak nie wiem czy to byl int czy sbyte to jak moge skonwertowac do unsigned'a ??
				//tak naprawde problem jest w tym ze nieznajomosc typu powoduje niemozliwosc przekonwertowania do postaci unsigned tego typu
				//Here and in other Un OpCodes: In JavaScript there is only one Number type. Taking care for unsigned opeartion is relevant ant would cost both execution time and memory usage.
				case OpCodeValue.Bge_Un:
				case OpCodeValue.Bgt_Un:
				case OpCodeValue.Ble_Un:
				case OpCodeValue.Blt_Un:
				case OpCodeValue.Cgt_Un:
				case OpCodeValue.Clt_Un:
				case OpCodeValue.Bge_Un_S:
				case OpCodeValue.Bgt_Un_S:
				case OpCodeValue.Ble_Un_S:
				case OpCodeValue.Blt_Un_S:
				#endregion
				#region Other unsigned things
				case OpCodeValue.Rem_Un:
				case OpCodeValue.Div_Un:
				case OpCodeValue.Shr_Un:
				case OpCodeValue.Conv_R_Un:
				#endregion
				#region ..., throwing OverflowException on overflow.
				//Here and in other Ovf OpCodes: No JavaScript support.
				case OpCodeValue.Conv_Ovf_I1_Un:
				case OpCodeValue.Conv_Ovf_I2_Un:
				case OpCodeValue.Conv_Ovf_I4_Un:
				case OpCodeValue.Conv_Ovf_I8_Un:
				case OpCodeValue.Conv_Ovf_U1_Un:
				case OpCodeValue.Conv_Ovf_U2_Un:
				case OpCodeValue.Conv_Ovf_U4_Un:
				case OpCodeValue.Conv_Ovf_U8_Un:
				case OpCodeValue.Conv_Ovf_I_Un:
				case OpCodeValue.Conv_Ovf_U_Un:
				case OpCodeValue.Conv_Ovf_I:
				case OpCodeValue.Conv_Ovf_U:
				case OpCodeValue.Add_Ovf:
				case OpCodeValue.Add_Ovf_Un:
				case OpCodeValue.Mul_Ovf:
				case OpCodeValue.Mul_Ovf_Un:
				case OpCodeValue.Sub_Ovf:
				case OpCodeValue.Sub_Ovf_Un:
				case OpCodeValue.Conv_Ovf_I1:
				case OpCodeValue.Conv_Ovf_U1:
				case OpCodeValue.Conv_Ovf_I2:
				case OpCodeValue.Conv_Ovf_U2:
				case OpCodeValue.Conv_Ovf_I4:
				case OpCodeValue.Conv_Ovf_U4:
				case OpCodeValue.Conv_Ovf_I8:
				case OpCodeValue.Conv_Ovf_U8:
				#endregion
				#endregion
				default:
					this.Throw("Unsupported OpCode.{0}.", opCode.Name);
					break;
			}

		}


		protected void Throw(string message) { ThrowHelper.Method.Throw(this.Method, this.Position, message); }
		protected void Throw(string format, params object[] args) { ThrowHelper.Method.Throw(this.Method, this.Position, format, args); }

		//private bool IsEmptyConstructor(ConstructorInfo info) {
		//  var body = info.GetMethodBody().GetILAsByteArray();
		//  return body.Length == 1 && body[0] == (byte)(short)OpCodeValue.Ldarg_0;
		//}

		private void onStfld(FieldInfo info) {
			this.AssemblyCompiler.Register(this.CalleeData, info.DeclaringType);
			this.AssemblyCompiler.Register(this.CalleeData, info);
			if (NativesManager.Instance.IsNative(info))
				Stfld_native(info);
			else
				Stfld(info);
		}
		private bool handleNativeCall(MethodBase info) {
			var native = NativesManager.Instance.GetNative(info);
			if (native.Value != null) {
				if (native.Value.GetType() == typeof(string)) {
					Ldstr(this.AssemblyCompiler.Register(this.CalleeData, (string)native.Value));
				} else if (native.Value.GetType() == typeof(int)) {
					Ldc_I((int)native.Value);
				} else {
					this.Throw("Unsupported value type: {0}.", native.Value.GetType().GetSignature());
				}
				return true;
				//wczesniej tego tutaj nei bylo dla wirtualnych. moze cos sie knoci? 
				//ale skoro to jest i tak natywne wiec nie widze problemu
			} else if (native.OpCode != null) {
				this._emit(native.OpCode);
				return true;
			} else if (native.Ignore) {
				return true;
			} else {
				switch (native.CommandType) {
					case NativeCommandType.None: return false;
					case NativeCommandType.Ldfld: {
							var fieldName = native.CommandArgument;
							if (fieldName == null) {
								if (info.Name.StartsWith("get_")) {
									fieldName = NativesManager.GetNativeName(info.Name.Substring(4));
								} else {
									fieldName = NativesManager.GetNativeName(info.Name);
								}
							}
							this.output.Append(JSOpCode.NLdf).Append(this.AssemblyCompiler.Register(this.CalleeData, fieldName).Encode());
							return true;
						}
					case NativeCommandType.Stfld: {
							var fieldName = native.CommandArgument;
							if (fieldName == null) {
								if (info.Name.StartsWith("set_")) {
									fieldName = NativesManager.GetNativeName(info.Name.Substring(4));
								} else {
									fieldName = NativesManager.GetNativeName(info.Name);
								}
							}
							this.output.Append(JSOpCode.NStf).Append(this.AssemblyCompiler.Register(this.CalleeData, fieldName).Encode());
							return true;
						}
					default:
						throw new ArgumentException();
				}
			}
		}
		private void OnCall(MethodBase info) {
			if (this.Call_common_(ref info)) return;

			this.AssemblyCompiler.Register(this.CalleeData, info);

			if (NativesManager.Instance.IsNative(info)) {
				if (!handleNativeCall(info))
					Call_native(info);
			} else {

				//ponizsza linijka nie moze byc poniewaz konstruktor moze brac argumenty dla sciemy.. i trza je popnac
				//if (info.IsConstructor && IsEmptyConstructor((ConstructorInfo)info)) return;

				ModuleCompiler moduleCompiler;
				if (info.IsStatic &&
					info.IsPublic &&
					(moduleCompiler = this.AssemblyCompiler.GetModuleCompiler(info.DeclaringType)) != null
				) {
					this.LoadModule(moduleCompiler.Index);
				}
				Call(info);
			}
		}
		private bool Call_common_(ref MethodBase info) {
			this.AssemblyCompiler.Register(this.CalleeData, info.DeclaringType);

			var runAt = info.GetCustomAttribute<RunAtAttribute>();
			if (runAt != null) {
				switch (runAt.RunAt) {
					case RunAt.Server:
						var methodInfo = (MethodInfo)info;
						//Release.Assert(methodInfo.GetParameters().All(p =>
						//  p.ParameterType == typeof(string) ||
						//  p.ParameterType.IsPrimitive ||
						//  p.ParameterType.GetCustomAttribute<DataContractAttribute>() != null
						//), info.GetSignature() + " must have DataContract or primitive type arguments to be called at server");
						ServerManager.Register(methodInfo, runAt);
						this._callServerSynchronous(methodInfo);
						return true;
				}
			}
			return TryInline(info);
		}

		#region Inline
#warning czemu nie mozna cacheowac inlineow?
		//private static Dictionary<MethodBase, string> inline = new Dictionary<MethodBase, string>();
		//private static string getInline(MethodBase method) {
		//  string ret;
		//  if (!inline.TryGetValue(method, out ret)) {
		//    try {
		//      var compiler = new JSILInlineMethodCompiler(this.aaamethod);
		//      compiler.Compile();
		//      ret = compiler.output.ToString();
		//    } catch (InlineException) {
		//      ret = null;
		//    } finally {
		//      inline[method] = ret;
		//    }
		//  }
		//  return ret;
		//}

		private bool TryInline(MethodBase info) {
			Inline inline = info.GetCustomAttribute<Inline>();
			if (!NativesManager.Instance.IsNative(info) && (inline == null ? Settings.InlineAll : inline.Value)) {
				try {
					var compiler = new JSILInlineMethodCompiler(this.AssemblyCompiler, this.CalleeData, info);
					compiler.Compile();
					this.output.Append(compiler.output.ToString());
					return true;
				} catch (InlineException) {
					return false;
				}
				//string inlineed = getInline(info);
				//if (inlineed == null) return false;
				//else {
				//  this.output.Append(inlineed);
				//  return true;
				//}
			} else
				return false;
		}
		#endregion
	}
	public partial class MethodCompiler {

		#region
		internal class JSOpCode {
			#region True/False
			public const char Tr = '~';
			public const char Fl = '!';
			#endregion
			#region Box
			public const char Box_Boolean = '~';
			public const char Box_Char = '!';
			public const char Box_Number = '@';
			public const char Unbox_Any_Boolean = '#';
			public const char Unbox_Any_Char = '$';
			public const char Unbox_Any_Number = '%';
			#endregion
			#region Ld_a
			/*
			 * kod podwojny bo brakuje
			 * to czesto wykorzystywane nie jest bo tyczy sie w zasadzie glownie w sytuacji
			 * ValueType a;a.F();
			 */
			public const char Ld_a_loc = '~';
			public const char Ld_a_arg = '!';
			public const char Ld_a_elem = '@';
			public const char Ld_a_fld = '#';
			public const char Ld_a_fld_native = '$';
			#endregion
			#region ExceptionHandlingClause
			public const char BeginCatch = '~';
			public const char BeginFinally = '!';
			#endregion
			public const char BeginTry = 'w';
			public const char Box_Unbox_Any = ' ';
			public const char Lds = '=';
			public const char LdI = 'y';
			public const char Ceq = 'o';
			public const char Leq = 'p';
			public const char Geq = '[';
			public const char StL = ']';
			public const char LdL = 's';
			public const char StA = 'g';
			public const char LdA = 'h';
			public const char Stelem = 'j';
			public const char Ldelem = 'k';
			public const char Pop = 'l';
			public const char Ldnull = ';';
			public const char Arr = '\'';
			public const char Dup = 'z';
			public const char Ldlen = 'x';
			public const char Brt = 'v';
			public const char Brf = 'n';
			public const char Br = 'm';
			public const char Sw = ',';
			public const char Ldsfld = '.';
			public const char Stsfld = 'r';
			public const char Ldfld = '~';
			public const char Stfld = '!';
			public const char Ldsfld_native = '@';
			public const char Stsfld_native = '#';
			public const char NLdf = '$';
			public const char NStf = 'U';
			public const char Callvirt = ':';
			public const char Call = 'S';
			public const char NCs = 't';
			public const char NCv = '(';
			public const char New = ')';
			public const char Ldfun = '_';
			public const char NLdfun = 'q';
			public const char NCvc = 'Q';
			public const char NativeCode = 'W';
			public const char NNew = 'E';
			public const char Blt = 'R';
			public const char Bgt = 'T';
			public const char Beq = 'Y';
			public const char Bge = 'I';
			public const char Ble = 'O';
			public const char Ret = '{';
			public const char Cl = '}';
			public const char Ca = 'F';
			public const char Css = 'G';
			public const char Csa = 'H';
			public const char Isinst_native = 'J';
			public const char Isinst = 'K';
			public const char Bne = 'L';
			public const char Ldc_R = 'Z';
			public const char Conv_I = 'X';
			public const char Conv_U = 'C';
			public const char Ld_a = 'V';
			public const char Ldind_or_Constrained = 'B';
			public const char Stind = 'N';
			public const char Throw = 'M';
			public const char LoadModule = 'i';
			public const char Sleep = '?';
			#region
			public const char And = '&';
			public const char Or = '|';
			public const char Add = '+';
			public const char Sub = '-';
			public const char Div = '/';
			public const char Mul = '*';
			public const char Xor = '^';
			public const char Clt = '<';
			public const char Cgt = '>';
			public const char Rem = '%';
			#endregion
#warning z tego mozna zrezygnowac bo ',' mozna na true/false...
			public const char CallNativeCtor = 'u';
			public const char Endfinally = 'D';
			public const char Leave = 'A';
			//public const char = 'P';

			//SPRAWDZONE: inne zajete z wyjatkiem '"' i '`' i '\\' ktore sa nieuzywalne
		}
		#endregion

		protected void _eval(string what) {
			this.output
							 .Append(JSOpCode.Ldsfld_native)
							 .Append(this.AssemblyCompiler.Register(this.CalleeData, what).Encode());
		}


		#region

		#region
		protected void Or() { this.output.Append(JSOpCode.Or); }
		protected void And() { this.output.Append(JSOpCode.And); }
		protected void Ret() { this.output.Append(JSOpCode.Ret); }
		protected void Rem() { this.output.Append(JSOpCode.Rem); }
		protected void Add() { this.output.Append(JSOpCode.Add); }
		protected void Div() { this.output.Append(JSOpCode.Div); }
		protected void Mul() { this.output.Append(JSOpCode.Mul); }
		protected void Sub() { this.output.Append(JSOpCode.Sub); }
		protected void Xor() { this.output.Append(JSOpCode.Xor); }
		protected void Ldlen() { this.output.Append(JSOpCode.Ldlen); }
		protected void Pop() { this.output.Append(JSOpCode.Pop); }
		protected void Dup() { this.output.Append(JSOpCode.Dup); }
		protected void Ceq() { this.output.Append(JSOpCode.Ceq); }
		protected void Clt() { this.output.Append(JSOpCode.Clt); }
		protected void Cgt() { this.output.Append(JSOpCode.Cgt); }
		protected void Ldnull() { this.output.Append(JSOpCode.Ldnull); }
		protected void Throw() { this.output.Append(JSOpCode.Throw); }
#warning ...there is no requirement that the block end with an endfinally instruction, and there can be as many endfinally instructions within the block as required...
		protected void Endfinally() { this.output.Append(JSOpCode.Endfinally); }

		protected void Ldelem(Type type) { this.output.Append(JSOpCode.Ldelem); }
		protected void Stelem(Type type) { this.output.Append(JSOpCode.Stelem); }
		protected void Stind(Type type) { this.output.Append(JSOpCode.Stind); }

		#endregion
		protected void LoadModule(int index) { this.output.Append(JSOpCode.LoadModule).Append(index.Encode()); }

		protected void Unbox_Any(Type type) {
			this.output.Append(JSOpCode.Box_Unbox_Any);
			if (type == typeof(bool)) this.output.Append(JSOpCode.Unbox_Any_Boolean);
			else if (type == typeof(char)) this.output.Append(JSOpCode.Unbox_Any_Char);
			//bo wiem ze prymitywny typ
			else this.output.Append(JSOpCode.Unbox_Any_Number);
		}
		protected void Box(Type type) {
			this.output.Append(JSOpCode.Box_Unbox_Any);
			if (type == typeof(bool)) this.output.Append(JSOpCode.Box_Boolean);
			else if (type == typeof(char)) this.output.Append(JSOpCode.Box_Char);
			//bo wiem ze prymitywny typ
			else this.output.Append(JSOpCode.Box_Number);

			/*
			 * po strinie js nie mozna sprawdzac typeof bo "number"->char|number
			 */
		}


		protected void Ldind(Type type) { this.output.Append(JSOpCode.Ldind_or_Constrained); }
#warning !!!!!!!!!!!!!!!!!!! to jest ewidentnie zle bo dereferencjuje ostatni argument zamiast `this`'a
		protected void Constrained(Type type) { this.output.Append(JSOpCode.Ldind_or_Constrained); }

		protected void Ldelema(Type type) { this.output.Append(JSOpCode.Ld_a).Append(JSOpCode.Ld_a_elem); }


		protected void Ldc_I(long i) {
			this.output.Append(JSOpCode.LdI).Append(i.Encode());
		}
		protected void Ldc_R(double value) {
			this.output.Append(JSOpCode.Ldc_R).Append(this.AssemblyCompiler.Register(this.CalleeData, value.ToString().Replace(',', '.')).Encode());
		}
		protected void _emit(string opCodeName) {
			this.output.Append((char)typeof(JSOpCode).GetField(opCodeName).GetValue(null));
		}
		protected void _cl(int args) { this.output.Append(JSOpCode.Cl).Append(args.Encode()); }
		protected void _callServerSynchronous(MethodInfo info) {
			this.output.Append(JSOpCode.Ca)
				.Append(ServerManager.IndexOf(info).Encode())
				.Append(info.ReturnParameter.ParameterType == typeof(void) ? JSOpCode.Fl : JSOpCode.Tr)
				.Append((info.GetParameters().Length + (info.IsStatic ? 0 : 1)).Encode());
		}
		/// <param name="args"><code>this</code> parameter doesn't count.</param>
		protected void _callServerAsynchronous(int args, bool returnsValue) {
			this.output.Append(JSOpCode.Csa).Append(args.Encode()).Append(returnsValue ? JSOpCode.Tr : JSOpCode.Fl);
		}
		/// <param name="args"><code>this</code> parameter doesn't count.</param>
		protected void _callServerSynchronous(int args, bool returnsValue) {
			this.output.Append(JSOpCode.Css).Append(args.Encode()).Append(returnsValue ? JSOpCode.Tr : JSOpCode.Fl);
		}

		#region {Ld|St}(s)?fld(a)?(_native)?
		#region Ldflda.*
		protected void Ldflda(FieldInfo info) {
			this.output
				.Append(JSOpCode.Ld_a)
				.Append(JSOpCode.Ld_a_fld)
				.Append(this.AssemblyCompiler.ResolveName(info));
		}
		protected void Ldflda_native(FieldInfo info) {
			this.output
				.Append(JSOpCode.Ld_a)
				.Append(JSOpCode.Ld_a_fld_native)
				.Append(this.AssemblyCompiler.ResolveName(info));
		}
		#endregion

		protected void Ldfld_native(FieldInfo info) { this.output.Append(JSOpCode.NLdf).Append(this.AssemblyCompiler.ResolveName(info)); }
		protected void Stfld_native(FieldInfo info) { this.output.Append(JSOpCode.NStf).Append(this.AssemblyCompiler.ResolveName(info)); }

		protected void Ldsfld_native(FieldInfo info) { this.output.Append(JSOpCode.Ldsfld_native).Append(this.AssemblyCompiler.ResolveName(info)); }
		protected void Stsfld_native(FieldInfo info) { this.output.Append(JSOpCode.Stsfld_native).Append(this.AssemblyCompiler.ResolveName(info)); }


		protected void Ldfld(FieldInfo info) { this.output.Append(JSOpCode.Ldfld).Append(this.AssemblyCompiler.ResolveName(info)); }
		protected void Stfld(FieldInfo info) { this.output.Append(JSOpCode.Stfld).Append(this.AssemblyCompiler.ResolveName(info)); }


		protected void Ldsfld(FieldInfo info) { this.output.Append(JSOpCode.Ldsfld).Append(this.AssemblyCompiler.ResolveName(info)); }
		protected void Stsfld(FieldInfo info) { this.output.Append(JSOpCode.Stsfld).Append(this.AssemblyCompiler.ResolveName(info)); }
		#endregion

		protected void Newarr(Type type) { this.output.Append(JSOpCode.Arr); }
		protected void Stloc(int index) { this.output.Append(JSOpCode.StL).Append(index.Encode()); }
		protected void Ldloc(int index) { this.output.Append(JSOpCode.LdL).Append(index.Encode()); }
		protected void Ldloca(int index) { this.output.Append(JSOpCode.Ld_a).Append(JSOpCode.Ld_a_loc).Append(index.Encode()); }
		protected void Starg(int index) { this.output.Append(JSOpCode.StA).Append(index.Encode()); }
		protected virtual void Ldarg(int index) { this.output.Append(JSOpCode.LdA).Append(index.Encode()); }
		protected void Ldarga(int index) { this.output.Append(JSOpCode.Ld_a).Append(JSOpCode.Ld_a_arg).Append(index.Encode()); }
		protected void Ldstr(int index) { this.output.Append(JSOpCode.Lds).Append(index.Encode()); }


		#region Protected regions
		protected void BeginTry(int offset) {
			this.output.Append(JSOpCode.BeginTry);
			this.AddJump(offset);
		}
		protected void BeginCath(int offset, Type catchType) {
			this.output.Append(JSOpCode.BeginCatch);
			//this.AddJump(offset);
		}
		protected void BeginFinally() {
			this.output.Append(JSOpCode.BeginFinally);
		}
		#endregion

		#region Branch
		protected virtual void Leave(int offset) {
			this.output.Append(JSOpCode.Leave);
			this.AddJump(offset);
		}

		protected virtual void Br(int offset) {
			this.output.Append(JSOpCode.Br);
			this.AddJump(offset);
		}
		protected virtual void Bne(int offset) {
			this.output.Append(JSOpCode.Bne);
			this.AddJump(offset);
		}
		protected virtual void Bgt(int offset) {
			this.output.Append(JSOpCode.Bgt);
			this.AddJump(offset);
		}
		protected virtual void Blt(int offset) {
			this.output.Append(JSOpCode.Blt);
			this.AddJump(offset);
		}
		protected virtual void Bge(int offset) {
			this.output.Append(JSOpCode.Bge);
			this.AddJump(offset);
		}
		protected virtual void Ble(int offset) {
			this.output.Append(JSOpCode.Ble);
			this.AddJump(offset);
		}
		protected virtual void Beq(int offset) {
			this.output.Append(JSOpCode.Beq);
			this.AddJump(offset);
		}
		protected virtual void Brfalse(int offset) {
			this.output.Append(JSOpCode.Brf);
			this.AddJump(offset);
		}
		protected virtual void Brtrue(int offset) {
			this.output.Append(JSOpCode.Brt);
			this.AddJump(offset);
		}
		#endregion

#warning te konwersje moga nie dzialac... np jak konwertujemy uint->int to sie nie zmieni mi na minus. z drugiej strony na stosie nie ma chyba rzeczy ktore nie maja znaku??
		protected void Conv_I(int sizeOf) { this.output.Append(JSOpCode.Conv_I).Append(sizeOf.Encode()); }
		protected void Conv_U(int sizeOf) { this.output.Append(JSOpCode.Conv_U).Append(sizeOf.Encode()); }

		protected void Switch() {
			var count = (uint)(this.EatToken().Value);
			int offsetBase = (int)(this.Position + 4 * count);
			this.output.Append(JSOpCode.Sw);
			this.output.Append(count.Encode());
			//z punktu widzeje javascriptu wygodniej mi skakac od tej pozycji....

			int basePosition = this.output.Length + checked((int)count);
			for (int i = 0; i < count; ++i) {
				this.output.Append(',');
				this.AddJump(offsetBase + (int)this.EatToken().Value, basePosition);
			}
		}
#warning czemu przy konstruktorach nie sprawdzam .Append(method.DeclaringType.IsValueType ? JSOpCode.Tr : JSOpCode.Fl)//czy dereferencjowac this'a

		protected void Newobj(ConstructorInfo ctor) {
			this.output
				.Append(JSOpCode.New)
				.Append(this.AssemblyCompiler.ResolveName(ctor.DeclaringType))
				.Append(',')
				.Append(this.AssemblyCompiler.ResolveNormalCallName(ctor))
				;
		}
		protected void Newobj_native(ConstructorInfo ctor) {
			this.output
				.Append(NativesManager.Instance.GetNative(ctor).Code == null ? JSOpCode.NNew : JSOpCode.NativeCode)
				.Append(this.AssemblyCompiler.ResolveNativeName(ctor))
				.Append(JSOpCode.Tr)//wykorzystywane w NativeCode, w NNew ignorowane
				.Append(ctor.GetParameters().Length.Encode());
		}
		#region Call.*
		protected void Call(MethodBase method) {
			this.output
				.Append(JSOpCode.Call)
				.Append(this.AssemblyCompiler.ResolveNormalCallName(method))
				.Append(method.DeclaringType.IsValueType && !method.IsStatic ? JSOpCode.Tr : JSOpCode.Fl);
			//czy dereferencjowac this'a - to ma znaczenie dla enumeratorow ktore sa structami... niestety
			;
		}
		protected void Callvirt(MethodInfo method) {
			this.output
				.Append(JSOpCode.Callvirt)
				.Append(this.AssemblyCompiler.ResolveVirtualCallName(method))
				.Append(method.DeclaringType.IsInterface ? JSOpCode.Tr : JSOpCode.Fl)
				;
		}
		protected void Call_native(MethodBase method) {
			if (method.IsConstructor) {
				this.output
					.Append(JSOpCode.CallNativeCtor)
					.Append(this.AssemblyCompiler.ResolveNativeName(method))
					.Append(',')
					.Append(method.GetParameters().Length.Encode())
					;
				return;
			}

			var native = NativesManager.Instance.GetNative(method);
			bool callAsStatic =
				native.CallType == NativeCallType.Static ||
				native.CallType == NativeCallType.Default && method.IsStatic
				//method.IsConstructor
 ;
			if (callAsStatic) {
				if (native.Code == null) {
					//method.IsStatic && IsNative(member.DeclaringType)
					this.output.Append(JSOpCode.NCs);
					this.output.Append((
						native.GlobalMethod ?
							this.AssemblyCompiler.Register(this.CalleeData, "window") :
							this.AssemblyCompiler.RegisterNative(this.CalleeData, method.DeclaringType)
					).Encode()).Append(',');
				} else {
					this.output.Append(JSOpCode.NativeCode);
				}
			} else {
				this.output.Append(native.Code == null ? JSOpCode.NCv : JSOpCode.NCvc);
			}
			int realParametersCount = method.GetParameters().Length;
			if (method.IsStatic) {
				if (native.CallType == NativeCallType.Instance) {
					--realParametersCount;
				}
			} else {
				if (native.CallType == NativeCallType.Static) {
					++realParametersCount;
				}
			}
			this.output
				.Append(this.AssemblyCompiler.ResolveNativeName(method))
				.Append(method.MemberType == MemberTypes.Method && ((MethodInfo)method).ReturnParameter.ParameterType != typeof(void) ? JSOpCode.Tr : JSOpCode.Fl)
				.Append(realParametersCount.Encode());
			if (!callAsStatic) {
				this.output.Append(method.DeclaringType.IsValueType ? JSOpCode.Tr : JSOpCode.Fl);//czy dereferencjowac this'a
			}

		}
		protected void Callvirt_native(MethodInfo method) {
			this.output
				.Append(NativesManager.Instance.GetNative(method).Code == null ? JSOpCode.NCv : JSOpCode.NCvc)
				.Append(this.AssemblyCompiler.ResolveNativeName(method))
				.Append(method.ReturnParameter.ParameterType != typeof(void) ? JSOpCode.Tr : JSOpCode.Fl)
				.Append(method.GetParameters().Length.Encode())
				.Append(method.DeclaringType.IsValueType ? JSOpCode.Tr : JSOpCode.Fl)//czy dereferencjowac this'a
				;
		}
		#endregion
		protected void Ldftn(MethodInfo method) {
			this.output
				.Append(JSOpCode.Ldfun)
				.Append(this.AssemblyCompiler.ResolveNormalCallName(method))
				;
#warning czy na pewno tu powinien byc taki resolve??
		}
		protected void Ldftn_native(MethodInfo method) {
			this.Throw("Cannot Ldftn(" + method.GetSignature() + ")");
		}
		protected void Isinst(Type type) {
			if (type == typeof(string)) {
				this.Ldstr(this.AssemblyCompiler.Register(this.CalleeData, "string"));
				this.output.Append(JSOpCode.Isinst_native);
			} else if (type == typeof(int)) {
				this.Ldstr(this.AssemblyCompiler.Register(this.CalleeData, "number"));
				this.output.Append(JSOpCode.Isinst_native);
			} else if (type == typeof(bool)) {
				this.Ldstr(this.AssemblyCompiler.Register(this.CalleeData, "boolean"));
				this.output.Append(JSOpCode.Isinst_native);
			} else {
				if (NativesManager.Instance.IsNative(type))
					this.Throw("OpCodes.Isinst (probably in expression: [varible] is/as [type]) is not supported for native type {0}.", type.GetSignature());
				if (type.IsInterface)
					this.Throw("OpCodes.Isinst (probably in expression: [varible] is/as [type]) is not supported for interface type {0}.", type.GetSignature());

				var data = this.AssemblyCompiler.Resolve(type);
				data.IsinstUsed = true;
				this.output.Append(JSOpCode.Isinst).Append(data.Name);

			}
		}

		#endregion


	}
	[global::System.Serializable]
	public class InlineException : Exception {
		public InlineException() { }
		public InlineException(string message) : base(message) { }
		public InlineException(string message, Exception inner) : base(message, inner) { }
		protected InlineException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
	public class JSILInlineMethodCompiler : MethodCompiler {
		public JSILInlineMethodCompiler(AssemblyCompiler compiler, MethodBaseData calee, MethodBase method)
			: base(compiler, calee, method) {
			if (method.IsAbstract)
				throw new InlineException("Method is abstract.");
			if (method.IsVirtual)
				throw new InlineException("Method is virtual.");
			if (method.GetMethodBody().LocalVariables.Count != 0)
				throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot have local varibles.");
		}

		int outputTokenPosition = -1;
		protected override void OnToken(OpCodeValue opCode) {
			++this.outputTokenPosition;
		}
		private int args = 0;
		#region { throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot have branches."); }
		protected override void Beq(int offset) { throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot have branches."); }
		protected override void Bge(int offset) { throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot have branches."); }
		protected override void Bgt(int offset) { throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot have branches."); }
		protected override void Ble(int offset) { throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot have branches."); }
		protected override void Blt(int offset) { throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot have branches."); }
		protected override void Bne(int offset) { throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot have branches."); }
		protected override void Br(int offset) { throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot have branches."); }
		protected override void Brfalse(int offset) { throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot have branches."); }
		protected override void Brtrue(int offset) { throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot have branches."); }
		protected override void Leave(int offset) { throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot have branches."); }
		#endregion
		protected override void Ldarg(int index) {
			if (outputTokenPosition != index)
				throw new InlineException("Inline method " + this.Method.GetSignature() + " cannot use arguments more then once and in not stack order.");
			++args;
		}

		protected override void OnCompiled() {
			if (this.args != (
				this.Method.IsConstructor ? this.Method.GetParameters().Length + 1 :
				this.Method.IsStatic ? this.Method.GetParameters().Length :
				this.Method.GetParameters().Length + 1))
				throw new InlineException("Inline method " + this.Method.GetSignature() + " must use all arguments.");
		}
	}
}


