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
using MK.JavaScript.Reflection;

namespace MK.JavaScript.Framework {
	//internal static class StringAttributeUtil {
	//  private static Dictionary<Type, string[]> _values = new Dictionary<Type, string[]>();
	//  private static string[] getValues(Type type) {
	//    string[] ret;
	//    if (!_values.TryGetValue(type, out ret)) {
	//      var fields=type.GetFields(BindingFlags.Static | BindingFlags.Public);
	//      ret = new string[fields.Length];
	//      _values[type] = ret;
	//      foreach (var fieldInfo in fields) {
	//        ret[(int)fieldInfo.GetValue(null)]=((ValueAttribute)fieldInfo.GetCustomAttributes(typeof(ValueAttribute), false)[0]).Value;
	//      }
	//    }
	//    return ret;
	//  }
	//  public static string GetValue(Type type, int value) {
	//    return getValues(type)[value];
	//  }
	//  public static string[] GetValues(Type type) {
	//    return getValues(type);
	//  }
	//}


	internal class FrameworkUtil {

		public static ConstructorInfo _delegate_ = typeof(FrameworkUtil).GetConstructor(new[] { typeof(object), typeof(IntPtr) });

		public FrameworkUtil(object o,IntPtr p) {	}
	}

}
