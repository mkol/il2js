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
using System.Reflection;

namespace MK.JavaScript.Reflection {
	/// <summary>
	/// Indicates if method should be inlined or not.
	/// </summary>
	/// <remarks>
	/// This method overrides compilation option.
	/// Target method must uses parameters in stack order and must use all parameters, cannot have branches and local varibles.
	/// </remarks>
	[global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class Inline : Attribute {
		/// <summary>
		/// If true, target method will be inlined (if possible).
		/// </summary>
		public readonly bool Value;
		public Inline() { Value = true; }
		public Inline(bool inline) { Value = inline; }
	}
	/// <summary>
	/// Calling syntax type for native method.
	/// </summary>
	public enum NativeCallType {
		/// <summary>
		/// Method will be called as it is declarated.
		/// </summary>
		Default,
		/// <summary>
		/// Method will be called as instance method (first parameter is referenced by this).
		/// </summary>
		Instance,
		/// <summary>
		/// Method will be called as static method (first parameter is referenced by $0).
		/// </summary>
		Static,
	}
	public enum NativeCommandType {
		None,
		Ldfld,
		Stfld,
	}

	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	public class JSNative : Attribute {
		public string Name;
		public JSNative() { }
		public JSNative(string name) { this.Name = name; }
		
		public string Ldfld {
			set {
				this.CommandType = NativeCommandType.Ldfld;
				this.CommandArgument = value;
			}
			get { return this.CommandType == NativeCommandType.Ldfld ? this.CommandArgument : null; }
		}
		public string Stfld {
			set {
				this.CommandType = NativeCommandType.Stfld;
				this.CommandArgument = value;
			}
			get { return this.CommandType == NativeCommandType.Stfld ? this.CommandArgument : null; }
		}

		public string Code;
		public string OpCode;
		public string AdditionalCode;
		public object Value;
		public bool Ignore;
		public NativeCommandType CommandType = NativeCommandType.None;
		public string CommandArgument;
		NativeCallType _CallType = NativeCallType.Default;
		public NativeCallType CallType {
			get { return this._CallType; }
			set {
				this._CallType = value;
				if (value == NativeCallType.Instance) {
					GlobalMethod = true;
				}
			}
		}
		public bool GlobalMethod = false;
	}
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	public class Set : JSNative {
		public Set() {
			this.CommandType = NativeCommandType.Stfld;
		}
		public Set(string fieldName) {
			this.Stfld = fieldName;
		}
	}
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	public class Get : JSNative {
		public Get() {
			this.CommandType = NativeCommandType.Ldfld;
		}
		public Get(string fieldName) {
			this.Ldfld = fieldName;
		}
	}

	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	public class PrototypeJS : JSNative {
		public PrototypeJS() { }
		public PrototypeJS(string name) : base(name) { }
	}
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	public class JSFramework : JSNative {
		public JSFramework() { }
	}
}
