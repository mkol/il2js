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
using MK.JavaScript.Reflection;
using System;
namespace MK.JavaScript.Framework.Mappings.System.Threading {
	public static class Monitor {
		public const string VaribleKey = "@l";
		public const string EnterCountVaribleKey = "@e";

		public const int EnterSleepMillisecondsTimeout = 100;

		public static void Enter(object obj) {
			while (
				obj.AsDictionary<global::System.Threading.Thread>().ContainsKey(VaribleKey) &&
				obj.AsDictionary<global::System.Threading.Thread>()[VaribleKey] != global::System.Threading.Thread.CurrentThread
			) {
				global::System.Threading.Thread.Sleep(EnterSleepMillisecondsTimeout);
			}
			if (obj.AsDictionary<int>().ContainsKey(EnterCountVaribleKey)) {
				++obj.AsDictionary<int>()[EnterCountVaribleKey];
			} else {
				obj.AsDictionary<int>()[EnterCountVaribleKey] = 1;
				obj.AsDictionary<global::System.Threading.Thread>()[VaribleKey] = global::System.Threading.Thread.CurrentThread;
			}
		}
		public static void Exit(object obj) {
			if (--obj.AsDictionary<int>()[EnterCountVaribleKey] == 0) {
				obj.AsDictionary<int>().Remove(EnterCountVaribleKey);
				obj.AsDictionary<object>().Remove(VaribleKey);
			}
		}
	}
	public class Thread {
		public const string ManagedThreadIdKey = "@t";
		public int ManagedThreadId {
			[JSNative(Code = "this.i?this.i:this.i=window['" + ManagedThreadIdKey + "']?++window['" + ManagedThreadIdKey + "']:window['" + ManagedThreadIdKey + "']=1")]
			get { throw new InvalidOperationException(JS.CannotRunAtServer); }
		}
	}
}
