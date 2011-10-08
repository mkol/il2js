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
using MK.JavaScript.Reflection;

namespace MK.JavaScript.Ajax {
	/// <summary>
	/// Utility class for explicit AJAX calls.
	/// </summary>
	[JSFramework]
	public static class Server {
		#region Call
		/// <summary>
		/// Synchronously calls given method with given parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="T2">The type of the second parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="T3">The type of the third parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="T4">The type of the fourth parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="TResult">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The method to be called at server.</param>
		/// <param name="t1">The value of the first parameter for <paramref name="func"/>.</param>
		/// <param name="t2">The value of the second parameter for <paramref name="func"/>.</param>
		/// <param name="t3">The value of the third parameter for <paramref name="func"/>.</param>
		/// <param name="t4">The value of the fourth parameter for <paramref name="func"/>.</param>
		/// <returns>Return value of <paramref name="func"/> call.</returns>
		[JSFramework]
		public static TResult Call<T1, T2, T3, T4, TResult>
			(Func<T1, T2, T3, T4, TResult> func, T1 t1, T2 t2, T3 t3, T4 t4) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Synchronously calls given method with given parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="T2">The type of the second parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="T3">The type of the third parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="TResult">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The method to be called at server.</param>
		/// <param name="t1">The value of the first parameter for <paramref name="func"/>.</param>
		/// <param name="t2">The value of the second parameter for <paramref name="func"/>.</param>
		/// <param name="t3">The value of the third parameter for <paramref name="func"/>.</param>
		/// <returns>Return value of <paramref name="func"/> call.</returns>
		[JSFramework]
		public static TResult Call<T1, T2, T3, TResult>
			(Func<T1, T2, T3, TResult> func, T1 t1, T2 t2, T3 t3) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Synchronously calls given method with given parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="T2">The type of the second parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="TResult">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The method to be called at server.</param>
		/// <param name="t1">The value of the first parameter for <paramref name="func"/>.</param>
		/// <param name="t2">The value of the second parameter for <paramref name="func"/>.</param>
		/// <returns>Return value of <paramref name="func"/> call.</returns>
		[JSFramework]
		public static TResult Call<T1, T2, TResult>
			(Func<T1, T2, TResult> func, T1 t1, T2 t2) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Synchronously calls given method with given parameter.
		/// </summary>
		/// <typeparam name="T">The type of the parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="TResult">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The method to be called at server.</param>
		/// <param name="t">The value of the parameter for <paramref name="func"/>.</param>
		/// <returns>Return value of <paramref name="func"/> call.</returns>
		[JSFramework]
		public static TResult Call<T, TResult>
			(Func<T, TResult> func, T t) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Synchronously calls given method.
		/// </summary>
		/// <typeparam name="TResult">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <returns>Return value of <paramref name="func"/> call.</returns>
		[JSFramework]
		public static TResult Call<TResult>
			(Func<TResult> func) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		/// <summary>
		/// Synchronously calls given method with given parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter of <paramref name="action"/>.</typeparam>
		/// <typeparam name="T2">The type of the second parameter of <paramref name="action"/>.</typeparam>
		/// <typeparam name="T3">The type of the third parameter of <paramref name="action"/>.</typeparam>
		/// <typeparam name="T4">The type of the fourth parameter of <paramref name="action"/>.</typeparam>
		/// <param name="action">The method to be called at server.</param>
		/// <param name="t1">The value of the first parameter for <paramref name="action"/>.</param>
		/// <param name="t2">The value of the second parameter for <paramref name="action"/>.</param>
		/// <param name="t3">The value of the third parameter for <paramref name="action"/>.</param>
		/// <param name="t4">The value of the fourth parameter for <paramref name="action"/>.</param>
		[JSFramework]
		public static void Call<T1, T2, T3, T4>
			(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Synchronously calls given method with given parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter of <paramref name="action"/>.</typeparam>
		/// <typeparam name="T2">The type of the second parameter of <paramref name="action"/>.</typeparam>
		/// <typeparam name="T3">The type of the third parameter of <paramref name="action"/>.</typeparam>
		/// <param name="action">The method to be called at server.</param>
		/// <param name="t1">The value of the first parameter for <paramref name="action"/>.</param>
		/// <param name="t2">The value of the second parameter for <paramref name="action"/>.</param>
		/// <param name="t3">The value of the third parameter for <paramref name="action"/>.</param>
		[JSFramework]
		public static void Call<T1, T2, T3>
			(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Synchronously calls given method with given parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter of <paramref name="action"/>.</typeparam>
		/// <typeparam name="T2">The type of the second parameter of <paramref name="action"/>.</typeparam>
		/// <param name="action">The method to be called at server.</param>
		/// <param name="t1">The value of the first parameter for <paramref name="action"/>.</param>
		/// <param name="t2">The value of the second parameter for <paramref name="action"/>.</param>
		[JSFramework]
		public static void Call<T1, T2>
			(Action<T1, T2> action, T1 t1, T2 t2) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Synchronously calls given method with given parameter.
		/// </summary>
		/// <typeparam name="T">The type of the parameter of <paramref name="action"/>.</typeparam>
		/// <param name="action">The method to be called at server.</param>
		/// <param name="t">The value of the parameter for <paramref name="action"/>.</param>
		[JSFramework]
		public static void Call<T>
			(Action<T> action, T t) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Synchronously calls given method.
		/// </summary>
		/// <param name="action">The method to be called at server.</param>
		[JSFramework]
		public static void Call(Action action) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		#endregion
		#region AsyncCall
		/// <summary>
		/// Asynchronously calls given method with given parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="T2">The type of the second parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="T3">The type of the third parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="T4">The type of the fourth parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="TResult">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The method to be called at server.</param>
		/// <param name="t1">The value of the first parameter for <paramref name="func"/>.</param>
		/// <param name="t2">The value of the second parameter for <paramref name="func"/>.</param>
		/// <param name="t3">The value of the third parameter for <paramref name="func"/>.</param>
		/// <param name="t4">The value of the fourth parameter for <paramref name="func"/>.</param>
		/// <param name="callback">An action that will be performed on return value of <paramref name="func"/>.</param>
		[JSFramework]
		public static void AsyncCall<T1, T2, T3, T4, TResult>
			(Func<T1, T2, T3, T4, TResult> func, T1 t1, T2 t2, T3 t3, T4 t4, Action<TResult> callback) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Asynchronously calls given method with given parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="T2">The type of the second parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="T3">The type of the third parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="TResult">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The method to be called at server.</param>
		/// <param name="t1">The value of the first parameter for <paramref name="func"/>.</param>
		/// <param name="t2">The value of the second parameter for <paramref name="func"/>.</param>
		/// <param name="t3">The value of the third parameter for <paramref name="func"/>.</param>
		/// <param name="callback">An action that will be performed on return value of <paramref name="func"/>.</param>
		[JSFramework]
		public static void AsyncCall<T1, T2, T3, TResult>
			(Func<T1, T2, T3, TResult> func, T1 t1, T2 t2, T3 t3, Action<TResult> callback) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Asynchronously calls given method with given parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="T2">The type of the second parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="TResult">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The method to be called at server.</param>
		/// <param name="t1">The value of the first parameter for <paramref name="func"/>.</param>
		/// <param name="t2">The value of the second parameter for <paramref name="func"/>.</param>
		/// <param name="callback">An action that will be performed on return value of <paramref name="func"/>.</param>
		[JSFramework]
		public static void AsyncCall<T1, T2, TResult>
			(Func<T1, T2, TResult> func, T1 t1, T2 t2, Action<TResult> callback) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Asynchronously calls given method with given parameter.
		/// </summary>
		/// <typeparam name="T1">The type of the parameter of <paramref name="func"/>.</typeparam>
		/// <typeparam name="TResult">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The method to be called at server.</param>
		/// <param name="t1">The value of the parameter for <paramref name="func"/>.</param>
		/// <param name="callback">An action that will be performed on return value of <paramref name="func"/>.</param>
		[JSFramework]
		public static void AsyncCall<T1, TResult>
			(Func<T1, TResult> func, T1 t1, Action<TResult> callback) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Asynchronously calls given method.
		/// </summary>
		/// <typeparam name="TResult">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The method to be called at server.</param>
		/// <param name="callback">An action that will be performed on return value of <paramref name="func"/>.</param>
		[JSFramework]
		public static void AsyncCall<TResult>
			(Func<TResult> func, Action<TResult> callback) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		/// <summary>
		/// Asynchronously calls given method with given parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter of <paramref name="action"/>.</typeparam>
		/// <typeparam name="T2">The type of the second parameter of <paramref name="action"/>.</typeparam>
		/// <typeparam name="T3">The type of the third parameter of <paramref name="action"/>.</typeparam>
		/// <typeparam name="T4">The type of the fourth parameter of <paramref name="action"/>.</typeparam>
		/// <param name="action">The method to be called at server.</param>
		/// <param name="t1">The value of the first parameter for <paramref name="action"/>.</param>
		/// <param name="t2">The value of the second parameter for <paramref name="action"/>.</param>
		/// <param name="t3">The value of the third parameter for <paramref name="action"/>.</param>
		/// <param name="t4">The value of the fourth parameter for <paramref name="action"/>.</param>
		[JSFramework]
		public static void AsyncCall<T1, T2, T3, T4>
			(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Asynchronously calls given method with given parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter of <paramref name="action"/>.</typeparam>
		/// <typeparam name="T2">The type of the second parameter of <paramref name="action"/>.</typeparam>
		/// <typeparam name="T3">The type of the third parameter of <paramref name="action"/>.</typeparam>
		/// <param name="action">The method to be called at server.</param>
		/// <param name="t1">The value of the first parameter for <paramref name="action"/>.</param>
		/// <param name="t2">The value of the second parameter for <paramref name="action"/>.</param>
		/// <param name="t3">The value of the third parameter for <paramref name="action"/>.</param>
		[JSFramework]
		public static void AsyncCall<T1, T2, T3>
			(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Asynchronously calls given method with given parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter of <paramref name="action"/>.</typeparam>
		/// <typeparam name="T2">The type of the second parameter of <paramref name="action"/>.</typeparam>
		/// <param name="action">The method to be called at server.</param>
		/// <param name="t1">The value of the first parameter for <paramref name="action"/>.</param>
		/// <param name="t2">The value of the second parameter for <paramref name="action"/>.</param>
		[JSFramework]
		public static void AsyncCall<T1, T2>
			(Action<T1, T2> action, T1 t1, T2 t2) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Asynchronously calls given method with given parameter.
		/// </summary>
		/// <typeparam name="T">The type of the parameter of <paramref name="action"/>.</typeparam>
		/// <param name="action">The method to be called at server.</param>
		/// <param name="t">The value of the parameter for <paramref name="action"/>.</param>
		[JSFramework]
		public static void AsyncCall<T>
			(Action<T> action, T t) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Asynchronously calls given method.
		/// </summary>
		/// <param name="action">The method to be called at server.</param>
		[JSFramework]
		public static void AsyncCall(Action action) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		#endregion
		/// <summary>
		/// Synchronously performs AJAX request.
		/// </summary>
		/// <param name="url">Request URL.</param>
		/// <returns>Text of response.</returns>
		[PrototypeJS(Code = "new Ajax.Request($0,{method:'get',asynchronous:false}).transport.responseText")]
		public static string Get(string url) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		/// <summary>
		/// Asynchronously performs AJAX request.
		/// </summary>
		/// <param name="url">Request URL.</param>
		/// <param name="callback">An action that will be performed on text of response.</param>
		/// <returns>Text of response.</returns>
		[PrototypeJS(Code = @"
function(a,b){
	new Ajax.Request(a,{method:'get',onSuccess:function(c){$handler$(b)(c.transport.responseText)}})
}
")]
		public static string AsyncGet(string url, Action<string> callback) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		/// <summary>
		/// Utility class for handling AJAX requests.
		/// </summary>
		public static class Request {
			/// <summary>
			/// Total number of current AJAX requests.
			/// </summary>
			[PrototypeJS("Ajax.activeRequestCount")]
			public static readonly int Count;
			/// <summary>
			/// Occures when AJAX request is created.
			/// </summary>
			public static event Action Create {
				[Inline]
				add { PrototypeJs.Ajax.Responders.Register(value, "Create"); }
				[Inline]
				remove { PrototypeJs.Ajax.Responders.Unregister(value, "Create"); }
			}
			/// <summary>
			/// Occures when AJAX request is completed.
			/// </summary>
			public static event Action Complete {
				[Inline]
				add { PrototypeJs.Ajax.Responders.Register(value, "Complete"); }
				[Inline]
				remove { PrototypeJs.Ajax.Responders.Unregister(value, "Complete"); }
			}
		}
	}
}
