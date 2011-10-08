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

namespace MK.JavaScript.Ajax {
	/// <summary>
	/// Base class for modules.
	/// </summary>
	/// <remarks>
	/// Module's code is loaded on client when first static method or first constructor of module is called.
	/// </remarks>
	public abstract class Module {
		[JSFramework(Ignore = true)]
		protected Module() { }
	}
}
