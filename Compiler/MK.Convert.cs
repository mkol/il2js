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

namespace MK{
	public static class Convert {
		public static string ToJSString(string s) {
			StringBuilder sb = new StringBuilder(s.Length + 8);
			sb.Append('"');
			//sb.Append('\'');
			for (int i = 0; i < s.Length; ++i) {
				switch (s[i]) {
					case '\\': sb.Append(@"\\"); break;
					case '\"': sb.Append("\\\""); break;
					//case '\'': sb.Append(@"\'"); break;
					case '\n': sb.Append(@"\n"); break;
					case '\r': break;
					default: sb.Append(s[i]); break;
				}
			}
			return sb.Append('"').ToString();
			//return sb.Append('\'').ToString();
		}

	}
}
