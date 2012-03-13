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
#define DEGEN
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace MK.CodeDom.Compiler {

	public static class MemberInfoExtensions {
		public static MemberInfo GenericDefinitionOf(MemberInfo info) {
			return info.DeclaringType.GetGenericTypeDefinition().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Single(
				i => i.MetadataToken == info.MetadataToken
			);
		}


		public static MethodBase GetGenericIfNecessary(this MethodBase self) {
			if (self.DeclaringType.IsGenericType) {
				return (MethodBase)GenericDefinitionOf(self);
			} else {
				return self;
			}
		}
	}

	public class ILToken {
		public bool IsEaten = false;
		public int Position;
		public int Size;
		public object Value;
		public ILToken(int position, int size, object value) {
			this.Position = position;
			this.Size = size;
			this.Value = value;
		}
		public override string ToString() {
			var sb = new StringBuilder().Append('[').Append(this.Position.ToString().PadLeft(3)).Append('+').Append(this.Size).Append("] ");
			if (this.Value is MemberInfo) {
				var member = (MemberInfo)this.Value;
				if (member.DeclaringType != null)
					sb.Append(member.DeclaringType.Name).Append("::");
				sb.Append(member.Name).Append(' ');
				var f = member as FieldInfo;
				if (f != null) {
					if (f.IsStatic) sb.Append(".static ");
					sb.Append(": ").Append(f.FieldType.Name);
				}
				var m = member as MethodBase;
				if (m != null) {
					var mi = m as MethodInfo;
					if (m.IsStatic) sb.Append(".static ");
					foreach (var p in m.GetParameters()) {
						sb.Append(p.ParameterType).Append(" ");
					}
					if (mi != null) sb.Append(": ").Append(mi.ReturnType.Name);
				}
			} else {
				sb.Append(this.Value.GetType().Name + ": " + this.Value.ToString()).ToString();
			}
			return sb.ToString();
		}
	}
	public partial class ILParser {
		private int EatInt32() {
			var value = BitConverter.ToInt32(il, position);
			position += 4;
			return value;
		}
		private uint EatUInt32() {
			var value = BitConverter.ToUInt32(il, position);
			position += 4;
			return value;
		}
		private float EatFloat32() {
			var value = BitConverter.ToSingle(il, position);
			position += 4;
			return value;
		}
		private double EatFloat64() {
			var value = BitConverter.ToDouble(il, position);
			position += 8;
			return value;
		}
		private Module Module;


		private FieldInfo parseInlineField() {
			var ret = Module.ResolveField(EatInt32(),
					this.method.DeclaringType.IsGenericType ? this.method.DeclaringType.GetGenericArguments() : null,
					this.method.IsGenericMethod ? this.method.GetGenericArguments() : null);
#if DEGEN
			if (ret.DeclaringType.IsGenericType) {
				ret = (FieldInfo)MemberInfoExtensions.GenericDefinitionOf(ret);
			}
#endif
			return ret;
		}
		private MethodBase parseInlineMethod() {
			var ret = Module.ResolveMethod(EatInt32(),
				this.method.DeclaringType.IsGenericType ? this.method.DeclaringType.GetGenericArguments() : null,
				this.method.IsGenericMethod ? this.method.GetGenericArguments() : null);
#if DEGEN
			if (ret.DeclaringType.IsGenericType) {
				ret = (MethodBase)MemberInfoExtensions.GenericDefinitionOf(ret);
			}
#endif
			return ret;
		}
		private Type parseInlineType() {
			var ret = Module.ResolveType(EatInt32(),
				this.method.DeclaringType.IsGenericType ? this.method.DeclaringType.GetGenericArguments() : null,
				this.method.IsGenericMethod ? this.method.GetGenericArguments() : null);
#if DEGEN
			if (ret.IsGenericType) {
				ret = ret.GetGenericTypeDefinition();
			}
#endif
			return ret;
		}
		private string parseInlineString() { return Module.ResolveString(EatInt32()); }
#warning ??
		private FieldInfo parseInlineTok() { return Module.ResolveField(EatInt32()); }

		private int position;
		private byte[] il;
		MethodBase method;
		public ILToken[] GetTokens(MethodBase method) {
			this.method = method;
			il = method.GetMethodBody().GetILAsByteArray();
			ILToken[] ret = new ILToken[il.Length];
			position = 0;
			this.Module = method.Module;
			while (position < il.Length) {
				var opCode = byteCodes[il[position]];
				var p = this.position;
				if (opCode == OpCodes.Prefix1) {
					opCode = shortCodes[512 + (short)((il[position] << 8) + il[position + 1])];
					++position;
				}
				ret[p] = new ILToken(position, opCode.Size, opCode);
				++this.position;
				p = this.position;
				if ((OpCodeValue)opCode.Value == OpCodeValue.Switch) {
					var count = this.EatUInt32();
					ret[p] = new ILToken(p, 4, count);
					for (int i = 0; i < count; ++i) {
						p = this.position;
						ret[p] = new ILToken(p, 4, this.EatInt32());
					}
				} else
					switch (opCode.OperandType) {
						case OperandType.InlineBrTarget: ret[p] = new ILToken(p, 4, this.EatInt32()); break;
						case OperandType.InlineField: ret[p] = new ILToken(p, 4, parseInlineField()); break;
						case OperandType.InlineI: ret[p] = new ILToken(p, 4, this.EatInt32()); break;
						//case OperandType.InlineI8:
						//  break;
						case OperandType.InlineMethod: ret[p] = new ILToken(p, 4, parseInlineMethod()); break;
						case OperandType.InlineNone: break;
						//case OperandType.InlinePhi:
						//  break;
						case OperandType.InlineR: ret[p] = new ILToken(p, 8, this.EatFloat64()); break;
						//case OperandType.InlineSig:
						//  break;
						case OperandType.InlineString: ret[p] = new ILToken(p, 4, parseInlineString()); break;
						//case OperandType.InlineTok: ret[p] = new ILToken(p, 4, parseInlineTok()); break;
						case OperandType.InlineType: ret[p] = new ILToken(p, 4, parseInlineType()); break;
						//case OperandType.InlineVar: 
						case OperandType.ShortInlineBrTarget: ret[p] = new ILToken(p, 1, (sbyte)this.il[this.position++]); break;

						case OperandType.ShortInlineI: ret[p] = new ILToken(p, 1, (sbyte)this.il[this.position++]); break;
						case OperandType.ShortInlineR: ret[p] = new ILToken(p, 4, this.EatFloat32()); break;
						case OperandType.ShortInlineVar: ret[p] = new ILToken(p, 1, this.il[this.position++]); break;
						case OperandType.InlineSwitch: ret[p] = new ILToken(p, 4, this.EatInt32()); break;
						default:
							ThrowHelper.Method.Throw(this.method, this.position, "Not supported OperandType.{0}.", opCode.OperandType.ToString());
							break;
					}
			}
			return ret;
		}
	}
	public enum ILInstructionType : byte {

		_Store = 128,
		_BeginBlock = 64,
		_EndBlock = 32,

		None = 0,
		Return = 1,
		Statement = 2,
		LoadLocal = 3,
		Branch = 9,
		ConditionalBranch = 10,
		Label = 11,
		Push = 12,

		StoreArgument = 1 | _Store,
		StoreLocal = 2 | _Store,
		StoreField = 3 | _Store,
		StoreStaticField = 4 | _Store,

		BeginSwitch = 3 | _BeginBlock,
		EndSwitch = 3 | _EndBlock,
		BeginCase = 4 | _BeginBlock,
		EndCase = 4 | _EndBlock,
		BeginIf = 5 | _BeginBlock,
		EndIf = 5 | _EndBlock,
		BeginElse = 6 | _BeginBlock,
		EndElse = 6 | _EndBlock,
		BeginDoWhile = 7 | _BeginBlock,
		EndDoWhile = 7 | _EndBlock,
		BeginWhile = 8 | _BeginBlock,
		EndWhile = 8 | _EndBlock,
	}
	public class ILInstruction {
		public readonly ILInstructionType Type;
		public string[] Arguments;
		public readonly int NextInstructionPosition;
		public ILInstruction(int nextInstructionPosition, ILInstructionType type, params string[] args) {
			this.Type = type;
			this.Arguments = args;
			this.NextInstructionPosition = nextInstructionPosition;
		}
		public bool IsStoreInstruction { get { return (this.Type & ILInstructionType._Store) == ILInstructionType._Store; } }
		public override string ToString() {
			return this.Type + "(" + string.Join(",", this.Arguments) + ")@" + this.NextInstructionPosition;
		}
	}
}