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
using MK.JavaScript.Framework;
namespace MK.JavaScript {
	/// <summary>
	/// JavaScript global context class.
	/// </summary>
	public abstract class JS {

		internal const string CannotRunAtServer = "This method cannot be called ad server side.";

		/// <summary>
		/// Evaluates JavaScript code.
		/// <remarks>
		/// </remarks>
		/// Avoid situation when You expect return value of primitive type (number, boolean) and use T=Object with cast on primitive type.
		/// It should works but note that in JavaScript <value>eval("1")!==new Number(1)</value> but <value>eval("1")===1</value>.
		/// Framework may handle this but it wasn't good tested yet.
		/// </summary>
		[Obsolete("Some constructions, like int x=(int)JS.Eval<object>(\"1\"), may fails. See Remarks for details.")]
		[JSNative(GlobalMethod = true)]
		public static T Eval<T>(string value) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[JSNative(GlobalMethod = true)]
		public static bool IsNaN(int value) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[JSNative(GlobalMethod = true)]
		public static bool IsNaN(double value) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[JSNative(GlobalMethod = true)]
		public static string Escape(string value) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[JSNative(GlobalMethod = true)]
		public static string Unescape(string value) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		/// <summary>
		/// Accepts an array-like collection (anything with numeric indices) and returns its equivalent as an actual Array object. This method is a convenience alias of Array.from, but is the preferred way of casting to an Array.
		/// </summary>
		[PrototypeJS("$A", GlobalMethod = true)]
		public static T[] _A<T>(IEnumerable<T> value) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		/// <summary>
		/// Identity. Use this method to wrap result of two integers division.
		/// </summary>
		/// <remarks>
		/// This method is because in JavaScript there is no different types for integers and floats.
		/// In particular <c>(1/2)!=0</c> but <c>(1/2)==0.5</c>.
		/// </remarks>
		/// <param name="value">Result of two integers division</param>
		/// <returns>parseInt(<paramref name="value"/>)</returns>
		[JSNative("parseInt", GlobalMethod = true)]
		public static int Int(int value) { return value; }


		/// <summary>
		/// Client-side wrapper to creating an IEnumerable&lt;> that supports System.Linq.Enumerable methods.
		/// </summary>
		/// <remarks>This method cannot be called ad server-side.</remarks>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns>JavaScript list.</returns>
		public static IEnumerable<T> IEnumerable<T>(IEnumerable<T> source) {
			//if (PrototypeJs._Object.IsArray(source)) {
			//  return (List<T>)source;
			//} else 
			{
				var value = new List<T>();
				foreach (var item in source) {
					value.Add(item);
				}
				return value;
			}
		}
	}
}
