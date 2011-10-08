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

namespace MK.JavaScript.Framework {
	/*
	 * dlaczego sie nie da inaczej, tzn nie da sie enumem:
	 * element.Style.Display=element.ClassName==xx?Display.None:Display.Block
	 * i to jest tlumaczone na inty dla enuma i nijak mozna stwierdzic kiedy int ba myc jako taki enum
	 * i trza by wszystkie wartosci wysylac do klienta
	 * co jest mocno nieefektywne bo wiekszoscie i tak nigdy nie uzyje...
	 * 
	 * inny problem to funkcje co biora to jako parametr - wtedy to juz zywcem sie nie da obeznac kiedy konwertowac na .ToString
	 */
	/// <summary>
	/// Base class for enum-like css classes.
	/// </summary>
	public abstract class EnumString {
		internal readonly string Value;
		internal static T Parse<T>(string value) where T : EnumString {
			return (T)typeof(T).GetField(value, BindingFlags.Public | BindingFlags.Static).GetValue(null);
		}
		protected EnumString(string value) { this.Value = value; }
	}

	public static class Utils {
		public static char[] Tokens ={
																		'0','1','2','3','4','5','6','7','8','9',
																		'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
																		' ','!',
																	 //'\'',
																	 '#','%','&',
																	 //'"',
																	 '(',')','*','+',',','-','.','/',
																	 ':',';','<','=','>','?','@',
																	 
																	 '[',
																	 //'\\',
																	 ']','^','_',
																	 //'`',
																	 '{','|','}','~',
																 
																	 '$',
																	 'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
																 };
	}

}
