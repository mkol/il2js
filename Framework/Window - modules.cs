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
using MK.JavaScript.Reflection;

namespace MK.JavaScript {
	partial class Window {
		/// <summary>
		/// Dynamically adds CSS StyleSheet to the window.
		/// </summary>
		/// <param name="uri">A CSS StyleSheet URI (may be relative wrt. <c>this</c> page).</param>
		[JSNative(CallType = NativeCallType.Static, Code = @"
function(x,y,z){
	if(x.$links$===undefined)
		x.$links$={};
	else if(x.$links$[y])
		return;
	x.$links$[y]=1;
	z=new Element('link');
	z.href=y;
	z.rel='stylesheet';
	z.type='text/css';
	x.document.body.previousSibling.appendChild(z);
}
")]
		public void AddCssStylesheet(string uri) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		#region Export
		/// <summary>
		/// Binds JavaScript global method identifier to a method making method available from inline JavaScripts.
		/// </summary>
		/// <param name="name">A JavaScript identifier.</param>
		/// <param name="method">A method.</param>
		[JSNative(CallType = NativeCallType.Static, Code = @"function(_,a,b){_[a]=$handler$(b)}")]
		public void Export(string name, Action method) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Binds JavaScript global method identifier to a method making method available from inline JavaScripts.
		/// </summary>
		/// <param name="name">A JavaScript identifier.</param>
		/// <param name="method">A method.</param>
		[JSNative(CallType = NativeCallType.Static, Code = @"function(_,a,b){_[a]=$handler$(b)}")]
		public void Export<T>(string name, Action<T> method) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Binds JavaScript global method identifier to a method making method available from inline JavaScripts.
		/// </summary>
		/// <param name="name">A JavaScript identifier.</param>
		/// <param name="method">A method.</param>
		[JSNative(CallType = NativeCallType.Static, Code = @"function(_,a,b){_[a]=$handler$(b)}")]
		public void Export<T1, T2>(string name, Action<T1, T2> method) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Binds JavaScript global method identifier to a method making method available from inline JavaScripts.
		/// </summary>
		/// <param name="name">A JavaScript identifier.</param>
		/// <param name="method">A method.</param>
		[JSNative(CallType = NativeCallType.Static, Code = @"function(_,a,b){_[a]=$handler$(b)}")]
		public void Export<T1, T2, T3>(string name, Action<T1, T2, T3> method) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Binds JavaScript global method identifier to a method making method available from inline JavaScripts.
		/// </summary>
		/// <param name="name">A JavaScript identifier.</param>
		/// <param name="method">A method.</param>
		[JSNative(CallType = NativeCallType.Static, Code = @"function(_,a,b){_[a]=$handler$(b)}")]
		public void Export<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> method) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		/// <summary>
		/// Binds JavaScript global method identifier to a method making method available from inline JavaScripts.
		/// </summary>
		/// <param name="name">A JavaScript identifier.</param>
		/// <param name="method">A method.</param>
		[JSNative(CallType = NativeCallType.Static, Code = @"function(_,a,b){_[a]=$handler$(b)}")]
		public void Export<TResult>(string name, Func<TResult> method) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Binds JavaScript global method identifier to a method making method available from inline JavaScripts.
		/// </summary>
		/// <param name="name">A JavaScript identifier.</param>
		/// <param name="method">A method.</param>
		[JSNative(CallType = NativeCallType.Static, Code = @"function(_,a,b){_[a]=$handler$(b)}")]
		public void Export<T, TResult>(string name, Func<T, TResult> method) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Binds JavaScript global method identifier to a method making method available from inline JavaScripts.
		/// </summary>
		/// <param name="name">A JavaScript identifier.</param>
		/// <param name="method">A method.</param>
		[JSNative(CallType = NativeCallType.Static, Code = @"function(_,a,b){_[a]=$handler$(b)}")]
		public void Export<T1, T2, TResult>(string name, Func<T1, T2, TResult> method) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Binds JavaScript global method identifier to a method making method available from inline JavaScripts.
		/// </summary>
		/// <param name="name">A JavaScript identifier.</param>
		/// <param name="method">A method.</param>
		[JSNative(CallType = NativeCallType.Static, Code = @"function(_,a,b){_[a]=$handler$(b)}")]
		public void Export<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> method) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Binds JavaScript global method identifier to a method making method available from inline JavaScripts.
		/// </summary>
		/// <param name="name">A JavaScript identifier.</param>
		/// <param name="method">A method.</param>
		[JSNative(CallType = NativeCallType.Static, Code = @"function(_,a,b){_[a]=$handler$(b)}")]
		public void Export<T1, T2, T3, T4, TResult>(string name, Func<T1, T2, T3, T4, TResult> method) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		#endregion
	}
}
