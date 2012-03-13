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
using System.Runtime.Serialization;
using MK.JavaScript.Framework;

namespace MK.CodeDom.Compiler {


	public class FieldInfoMeta {
		public readonly FieldInfo FieldInfo;
		public readonly string Token;

		public FieldInfoMeta(FieldInfo fieldInfo, string token) {
			this.FieldInfo = fieldInfo;
			this.Token = token;
		}

		#region static
		public static FieldInfoMeta Get(FieldInfo info) { return TypeMeta.Get(info.DeclaringType).Fields[info]; }
		#endregion
	}
	public class TypeMeta {
		#region static
		internal static Dictionary<Type, TypeMeta> metas = new Dictionary<Type, TypeMeta>();
		public static TypeMeta Get(Type type) {
			TypeMeta value;
			if (!metas.TryGetValue(type, out value)) {
				value = new TypeMeta(type);
				metas[type] = value;
			}
			return value;
		}
		#endregion
		public readonly Type Type;
		private int tokenIndex;
		public readonly Dictionary<FieldInfo, FieldInfoMeta> Fields;

		private void assertTokens() {
			if (this.Type.IsSubclassOf(typeof(MK.JavaScript.Window))) {
				if (this.tokenIndex >= (Utils.Tokens.Length - 27)) {
					ThrowHelper.Throw("Window class {0} togather with all classes it inherits contains more then {1} fields, which is not supported.", this.Type.GetSignature(), Utils.Tokens.Length - 27);
				}
			} else {
				if (this.tokenIndex >= Utils.Tokens.Length) {
					ThrowHelper.Throw("Class {0} togather with all classes it inherits contains more then {1} fields, which is not supported.", this.Type.GetSignature(), Utils.Tokens.Length);
				}
			}
		}

		private TypeMeta(Type type) {
			this.Type = type;
			this.tokenIndex = type.BaseType == typeof(object) ? 0 : Get(type.BaseType).tokenIndex;
			//ten if jest rozdzielony z przyczyn czytelnosciowo-konwencyjnych.. tak naprawde tworza sie te same slowniki
			//ale jesli jest datacontract to worningowane sa dodatkowe atrybuty datamember'a
			if (this.Type.GetCustomAttribute<DataContractAttribute>() != null) {
				this.Fields = new Dictionary<FieldInfo, FieldInfoMeta>();
#warning kombinowane bo problem: fieldy generic'a nei sa declaratedonly
				//poniewaz field.DeclaringType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) dla field.DeclaringType.IsGenericType && !field.DeclaringType.IsGenericTypeDefinition
				//to tez powoduje ze nie mozna wziasc losowej kolejnosci!
				foreach (var field in from f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
															where f.DeclaringType == type
															orderby f.Name
															select f
																) {
					var dataMember = field.GetCustomAttribute<DataMemberAttribute>();
					if (dataMember != null) {
						if (dataMember.Name != null) Program.XmlLog.WriteElementString("warning",
							string.Format("[DataMember(Name=\"{0}\")] {1}; Name is ignored.", dataMember.Name, field.GetSignature()));
						if (dataMember.Order != -1) Program.XmlLog.WriteElementString("warning",
							string.Format("[DataMember(Order={0})] {1}; Order is ignored.", dataMember.Order, field.GetSignature()));
					}
					this.assertTokens();
					this.Fields[field] = new FieldInfoMeta(field, Utils.Tokens[this.tokenIndex++].ToString());
				}

			} else {
				this.Fields = (
					from f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					where f.DeclaringType == type
					orderby f.Name
					select f
				).ToDictionary(
					field => field,
					field => {
						this.assertTokens();
						return new FieldInfoMeta(field, Utils.Tokens[this.tokenIndex++].ToString());
					}
				);
			}

		}
	}
}
