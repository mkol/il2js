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
using System.Text;

namespace MK.JavaScript.Ajax {
	/// <summary>
	/// Indicates how the method should be invoked.
	/// </summary>
	public enum RunAt : byte {
		/// <summary>
		/// Method will be invoked at client side (using standart method(args...)) notation.
		/// </summary>
		Client = 0x1,
		/// <summary>
		/// Method will be invoked at server side.
		/// It may be called either in synchronous mode (method(args...) or Server.Call(method,args...))
		/// or in asynchronous mode (Sever.AsyncCall(method,args...[,callback])).
		/// No client code except call will be generated.
		/// </summary>
		Server = 0x2,
		/// <summary>
		/// Method can be invoked at client (method(args...)
		/// or at server (Server.Call(method,args...) or Sever.AsyncCall(method,args...[,callback])).
		/// </summary>
		ClientOrServer = Client | Server,
	}
	/// <summary>
	/// By default, all methods in assembly are treat as RunAt.Client. This attriute override this.
	/// </summary>
	[global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class RunAtAttribute : Attribute {
		public readonly RunAt RunAt;
		public bool HideExceptionMessage;
		public RunAtAttribute(RunAt runAt) {
			this.RunAt = runAt;
		}
	}

	///// <summary>
	///// You may assign whole class or some of it's method to module.
	///// All used methods in module will be loaded when first method of module will about to be executed. 
	///// </summary>
	//[global::System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	//public sealed class ModuleAttribute : Attribute {
	//  public readonly string Name;
	//  public ModuleAttribute() {
	//    this.Name = Guid.NewGuid().ToString();
	//  }
	//  public ModuleAttribute(string name) {
	//    this.Name = name;
	//  }
	//}
}
