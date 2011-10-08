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
using System.Collections;
using System.Collections.Generic;
using MK.JavaScript.Reflection;
using System.Reflection;

namespace MK.JavaScript.Framework.Mappings {
	public class JSEnumerable<T> : IEnumerable<T> {

		private static IEnumerator<T> GetEnumerator(IEnumerable self) { return new JSArrayEnumerator<T>(self); }

		public IEnumerator<T> GetEnumerator() { return GetEnumerator(this); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(this); }
		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(this); }

	}


	public struct JSArrayEnumerator<T> : IEnumerator<T> {
		private T[] array;
		private int index;

		public JSArrayEnumerator(object array) {
			this.array = (T[])array;
			this.index = -1;
		}

		public T Current {
			get {
				return this.array[this.index];
			}
		}

		T IEnumerator<T>.Current {
			get {
				return this.array[this.index];
			}
		}
		object IEnumerator.Current {
			get {
				return this.array[this.index];
			}
		}
		bool IEnumerator.MoveNext() {
			return ++this.index < this.array.Length;
		}
		void IEnumerator.Reset() {
			this.index = -1;
		}

		void IDisposable.Dispose() { }



	}
}