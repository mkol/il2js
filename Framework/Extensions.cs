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
using System.Collections;
using MK.JavaScript.Reflection;
namespace MK.JavaScript {
	/// <summary>
	/// Extensions of <value>object</value>.
	/// </summary>
	public static class Extensions {
		/// <summary>
		/// Treats self as <value>Dictionary&lt;string,<typeparamref name="T"/>></value>.
		/// </summary>
		/// <param name="self">
		/// Any JavaScript object.
		/// </param>
		/// <typeparam name="T">
		/// Any type.
		/// </typeparam>
		/// <returns><paramref name="self"/> as <value>Dictionary&lt;string,<typeparamref name="T"/>></value>.</returns>
		/// <remarks>
		/// Some browsers (like IE) doesn't supports delete operator for DOM elements, so avoid calling <value>aDomElement.AsDictionary&lt;<typeparamref name="T"/>>().Remove(aTKey)</value>.
		/// <value>1</value>, <value>true</value>, <value>"abc"</value> are not JavaScript objects.
		/// </remarks>
		[JSNative(Ignore = true)]
		public static Dictionary<string, T> AsDictionary<T>(this object self) {  throw new InvalidOperationException(JS.CannotRunAtServer); }
	}
}
