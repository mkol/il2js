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



namespace MK.JavaScript.Ajax.Serialization {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.IO;
	using System.Diagnostics;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Web;
	using System.Collections;
	using MK.JavaScript.Framework;

	internal class Parser {
		////bo prototype nie ma innych whitespacesow niz ' '
		//private void skipWhitespaces() { 
		//  while (char.IsWhiteSpace(this.source[this.position]))
		//  ++this.position; 
		//}
		private object ParseObject() {
			switch (this.source[this.position]) {
				case '{':
					#region object
 {
						Dictionary<string, object> ret = new Dictionary<string, object>();
						++this.position;// /{/
						if (this.source[this.position] == '}') {
							++this.position;
							return ret;
						} else do {
								var index = this.ParseString();
								this.position += 2;// /: /
								ret[index] = this.ParseObject();
								switch (this.source[this.position]) {
									case ',':
										this.position += 2;
										break;
									case '}':
										++this.position;
										return ret;
								}
							} while (true);
					}
					#endregion
				case '[':
 #region array
 {
	 List<object> ret = new List<object>();
	 ++this.position;// /\[/
	 if (this.source[this.position] == ']') {
		 ++this.position;
		 return ret;
	 } else do {
			 ret.Add(this.ParseObject());
			 switch (this.source[this.position]) {
				 case ',':
					 this.position += 2;
					 break;
				 case ']':
					 ++this.position;
					 return ret;
			 }
		 } while (true);
 }
 #endregion
				case '"': return ParseString();
				case '-':
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					#region number
 {
						int length = 1;
						while (new[] { '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }.Contains(this.source[this.position + length])) {
							++length;
						}
						var ret = this.source.Substring(this.position, length);
						this.position += length;
						return ret;

					}
					#endregion
				case 'n':
 #region null
 if (
	 this.source[this.position + 1] == 'u' &&
	 this.source[this.position + 2] == 'l' &&
	 this.source[this.position + 3] == 'l') {
	 this.position += 4;
	 return null;
 } else {
	 throw new Exception(this.source.Substring(this.position));
 }
 #endregion
				case 't':
 #region true
 if (
	 this.source[this.position + 1] == 'r' &&
	 this.source[this.position + 2] == 'u' &&
	 this.source[this.position + 3] == 'e') {
	 this.position += 4;
	 return true;
 } else {
	 throw new Exception(this.source.Substring(this.position));
 }
 #endregion
				case 'f':
 #region false
 if (
	 this.source[this.position + 1] == 'a' &&
	 this.source[this.position + 2] == 'l' &&
	 this.source[this.position + 3] == 's' &&
	 this.source[this.position + 4] == 'e') {
	 this.position += 5;
	 return false;
 } else {
	 throw new Exception(this.source.Substring(this.position));
 }
 #endregion
			}
			throw new Exception("Unknown character: " + this.source[this.position] + "@" + this.position);
		}

		private string ParseString() {
			++this.position;
			StringBuilder sb = new StringBuilder("");
			do {
				switch (this.source[this.position]) {
					case '\"':
						++this.position;
						return sb.ToString();
					case '\\':
						++this.position;
						switch (this.source[this.position]) {
							case 'n': sb.Append('\n'); break;
							case 't': sb.Append('\t'); break;
							case '"': sb.Append('"'); break;
							default:
								throw new Exception(@"\" + this.source[this.position]);
						}
						++this.position;
						break;
					default:
						sb.Append(this.source[this.position++]);
						break;
				}
			} while (true);
		}
		private string source;
		private int position;
		public object Parse(string s) {
			this.source = s;
			this.position = 0;
			return this.ParseObject();
		}
	}

	internal interface ISerializer {
		string Serialize(object obj);
		object Deserialize(object source);
	}
	internal class NumberSerializer : ISerializer {
		private readonly MethodInfo parse;
		public NumberSerializer(Type type) {
			this.parse = type.GetMethod("Parse", new[] { typeof(string) });
		}
		string ISerializer.Serialize(object obj) { return obj.ToString().Replace(',', '.'); }
		object ISerializer.Deserialize(object source) { return this.parse.Invoke(null, new[] { source }); }
	}
	internal class BooleanSerializer : ISerializer {
		string ISerializer.Serialize(object obj) { return true.Equals(obj) ? "true" : "false"; }
		object ISerializer.Deserialize(object source) { return source; }
	}
	internal class StringSerializer : ISerializer {
		string ISerializer.Serialize(object obj) {
			if (obj == null) return "null";
			StringBuilder sb = new StringBuilder();
			sb.Append('"');
			foreach (var c in (string)obj) {
				switch (c) {
					case '\\': sb.Append(@"\\"); break;
					case '\"': sb.Append("\\\""); break;
					case '\n': sb.Append(@"\n"); break;
					case '\r': break;
					default: sb.Append(c); break;
				}
			}
			sb.Append('"');
			return sb.ToString();
		}
		object ISerializer.Deserialize(object source) { return source; }
	}
	internal class IEnumerable1Serializer : ISerializer {
		string ISerializer.Serialize(object obj) {
			if (obj == null) return "null";
			StringBuilder sb = new StringBuilder();
			sb.Append('[');
			foreach (var item in (IEnumerable)obj) {
				sb.Append(Serializer.Serialize(item)).Append(',');
			}
			if (sb.Length > 1)
				--sb.Length;
			sb.Append(']');
			return sb.ToString();
		}
		object ISerializer.Deserialize(object source) {
			var values = (List<object>)source;
			var ret = (IList)typeof(List<>).MakeGenericType(this.type).GetConstructor(Type.EmptyTypes).Invoke(null);
			foreach (var item in values) {
				ret.Add(Serializer.Deserialize(this.type, item));
			}
			return ret;
		}
		private Type type;
		public IEnumerable1Serializer(Type type) { this.type = type; }
	}
	internal class ArraySerializer : ISerializer {
		string ISerializer.Serialize(object obj) {
			if (obj == null) return "null";
			StringBuilder sb = new StringBuilder();
			sb.Append('[');
			foreach (var item in (Array)obj) {
				sb.Append(Serializer.Serialize(item)).Append(',');
			}
			if (sb.Length > 1)
				--sb.Length;
			sb.Append(']');
			return sb.ToString();
		}
		object ISerializer.Deserialize(object source) {
			var values = (List<object>)source;
			var ret = Array.CreateInstance(this.type, values.Count);
			for (int i = 0; i < values.Count; ++i) {
				ret.SetValue(Serializer.Deserialize(this.type, values[i]), i);
			}
			return ret;
		}
		private Type type;
		public ArraySerializer(Type type) { this.type = type; }
	}
	internal class IDictionary2Serializer : ISerializer {
		string ISerializer.Serialize(object obj) {
			if (obj == null) return "null";
			StringBuilder sb = new StringBuilder();
			sb.Append('{');
			foreach (DictionaryEntry item in (IDictionary)obj) {
				sb.Append(Serializer.Serialize(item.Key)).Append(':').Append(Serializer.Serialize(item.Value)).Append(',');
			}
			if (sb.Length > 1)
				--sb.Length;
			sb.Append('}');
			return sb.ToString();
		}
		object ISerializer.Deserialize(object source) {
			var values = (Dictionary<string, object>)source;
			var ret = (IDictionary)typeof(Dictionary<,>).MakeGenericType(this.keyType, this.valueType).GetConstructor(Type.EmptyTypes).Invoke(null);
			foreach (var item in values) {
				ret[Serializer.Deserialize(this.keyType, item.Key)] = Serializer.Deserialize(this.valueType, item.Value);
			}
			return ret;
		}
		private Type keyType;
		private Type valueType;
		public IDictionary2Serializer(Type keyType, Type valueType) { this.keyType = keyType; this.valueType = valueType; }
	}
	internal class ClassSerializer : ISerializer {
		private class FieldSerializer {
			public void Set(object obj, object value) {
				this.Field.SetValue(obj, value);
			}
			public object Get(object obj) {
				return this.Field.GetValue(obj);
			}
			public readonly FieldInfo Field;
			public FieldSerializer(FieldInfo field) {
				this.Field = field;
			}
		}
		private Dictionary<char, FieldSerializer> fieldSerializers = new Dictionary<char, FieldSerializer>();

		internal char GetToken(FieldInfo fieldInfo) {
			foreach (var item in this.fieldSerializers) {
				if (item.Value.Field == fieldInfo) {
					return item.Key;
				}
			}
			return ((ClassSerializer)Serializer.GetSerializer(this.type.BaseType)).GetToken(fieldInfo);
		}

		private readonly Type type;
		private int tokenIndex;
		public ClassSerializer(Type type) {
			this.type = type;
			//throw new Exception(type+"|"+type.BaseType);
			this.tokenIndex = type.BaseType == typeof(object) ? 0 : ((ClassSerializer)Serializer.GetSerializer(type.BaseType)).tokenIndex;

			foreach (var field in from f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
														where f.DeclaringType == type
														orderby f.Name
														select f) {
				var attributes = field.GetCustomAttributes(typeof(DataMemberAttribute), false);
				if (attributes.Length == 1) {
					this.fieldSerializers[Utils.Tokens[this.tokenIndex]] = new FieldSerializer(field);
				}
				++this.tokenIndex;
			}


			//do {
			//  foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
			//    var attrs = field.GetCustomAttributes(typeof(DataMemberAttribute), false);
			//    if (attrs.Length == 1) {
			//      var data = (DataMemberAttribute)attrs[0];
			//      this.fieldSerializers[data.Order] = new FieldSerializer(field);
			//    }
			//  }
			//  type = type.BaseType;
			//} while (type.GetCustomAttributes(typeof(DataContractAttribute), false).Length == 1);
		}
		string ISerializer.Serialize(object obj) {
			if (obj == null) return "null";
			StringBuilder sb = new StringBuilder();
			sb.Append('{');
			var serializer = this;
			while (true) {
				foreach (var item in serializer.fieldSerializers) {
					sb.Append(item.Key).Append(':');
					sb.Append(Serializer.Serialize(item.Value.Get(obj)));
					sb.Append(',');
				}
				if (serializer.type.BaseType != typeof(object)) {
					serializer = (ClassSerializer)Serializer.GetSerializer(serializer.type.BaseType);
				} else {
					break;
				}
			}
			if (sb.Length > 1)
				--sb.Length;
			sb.Append('}');
			return sb.ToString();
		}
		object ISerializer.Deserialize(object source) {
			var values = (Dictionary<string, object>)source;
			var ret = this.type.GetConstructor(Type.EmptyTypes).Invoke(null);
			foreach (var item in values) {
				FieldSerializer serializer;
				if (this.fieldSerializers.TryGetValue(item.Key[0], out serializer))
					try {
						serializer.Set(ret, Serializer.Deserialize(serializer.Field.FieldType, item.Value));
					} catch (Exception e) {
						throw new Exception("Error while setting " + serializer.Field.Name + ":\n" + e);
					}
			}
			return ret;
		}
	}

	public static class Serializer {
		private static Dictionary<Type, ISerializer> serializers = new Dictionary<Type, ISerializer>() {
			{typeof(byte),new NumberSerializer(typeof(byte))},
			{typeof(sbyte),new NumberSerializer(typeof(sbyte))},
			{typeof(ushort),new NumberSerializer(typeof(ushort))},
			{typeof(short),new NumberSerializer(typeof(short))},
			{typeof(uint),new NumberSerializer(typeof(uint))},
			{typeof(int),new NumberSerializer(typeof(int))},
			{typeof(ulong),new NumberSerializer(typeof(ulong))},
			{typeof(long),new NumberSerializer(typeof(long))},
			{typeof(decimal),new NumberSerializer(typeof(decimal))},
			{typeof(float),new NumberSerializer(typeof(float))},
			{typeof(double),new NumberSerializer(typeof(double))},
			{typeof(string),new StringSerializer()},
			{typeof(bool),new BooleanSerializer()},
		};
		internal static ISerializer GetSerializer(Type type) {
			ISerializer ret;
			if (!serializers.TryGetValue(type, out ret)) {
				if (type.IsArray) {
					ret = new ArraySerializer(type.GetElementType());
				} else {
					if (type.Namespace == "System.Collections.Generic") {
						switch (type.Name) {
							case "IDictionary`2": {
									Type[] types = type.GetGenericArguments();
									ret = new IDictionary2Serializer(types[0], types[1]);
									goto end;
								}
							case "IEnumerable`1":
								ret = new IEnumerable1Serializer(type.GetGenericArguments()[0]);
								goto end;
						}
					}
					foreach (var i in type.GetInterfaces().OrderBy(t => t.Name)) {//orderby zeby slownik jako slownik a nie jako enumeracje key/value
						switch (i.Name) {
							case "IDictionary`2": {
									Type[] types = i.GetGenericArguments();
									ret = new IDictionary2Serializer(types[0], types[1]);
									goto end;
								}
							case "IEnumerable`1":
								ret = new IEnumerable1Serializer(i.GetGenericArguments()[0]);
								goto end;
						}
					}
					ret = new ClassSerializer(type);
				}
			end:
				serializers[type] = ret;
			}
			return ret;
		}

		public static List<object> Deserialize(HttpContext context, params Type[] types) {

			//      var bytes = new byte[context.Request.TotalBytes];
			//      context.Request.InputStream.Read(bytes, 0, context.Request.TotalBytes);
			//#warning to nie bedzie dzialac dla unicode character
			//      var s = new string(bytes.Select(b => (char)b).ToArray());

			byte[] bytes = new byte[context.Request.TotalBytes];
			context.Request.InputStream.Read(bytes, 0, bytes.Length);
			var s = context.Request.ContentEncoding.GetString(bytes);


			var ret = (List<object>)new Parser().Parse(s);
			for (int i = 0; i < types.Length; ++i) {
				ret[i] = Deserialize(types[i], ret[i]);
			}
			//throw new Exception(string.Join(",", ret.Select(r => r.GetType().Name).ToArray()));
			return ret;
		}
		public static string Serialize(object obj) {
			if (obj == null) return "null";
			return GetSerializer(obj.GetType()).Serialize(obj);
		}
		internal static object Deserialize(Type type, object source) {
			return source == null ? null : GetSerializer(type).Deserialize(source);
		}
	}


}
