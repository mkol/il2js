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

namespace MK.JavaScript {
	/// <summary>
	/// Marks class as Window class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class WindowAttribute : Attribute {
		/// <summary>
		/// Indicates if html content of page should be generated using page's Render method.
		/// </summary>
		/// <remarks>
		/// If true, IL2JS will search for `protected Element Render()` or `protected IEnumerable&lt;Element> Render()` method. In this case constructor of the page must be empty!
		/// </remarks>
		/// <example>
		/// [Window("~/NotExisting.html")]
		/// public class DefaultPage : Window {
		///		protected Element Render(){
		///			return new Span { InnedHTML = "Sample content of page" };
		///		}
		/// }
		/// </example>
		public bool Render;
		/// <summary>
		/// Virtual path of page. Must starts with tilde (~).
		/// </summary>
		public readonly string Path;
		/// <param name="path">
		/// Virtual path. Must starts with tilde (~).
		/// </param>
		/// <example>
		/// <code>
		/// [Window("~/Default.aspx")]
		/// public class DefaultPage : Window {}
		/// </code>
		/// </example>
		public WindowAttribute(string path) { this.Path = path; }
		/// <summary>
		/// If true, this class used methods will be placed in top .js file, regardles of which subframe depends on it.
		/// It reduces output .js total size.
		/// </summary>
		/// <example>
		/// When method is called on two subframes if MethodsOnTop is false - code of that method (with it's dependencies) will be placed in .js files for that subframes; if MethodsOnTop is true - all codes will be placed in top .js file only.
		/// </example>
		public bool MethodsOnTop = false;
		/// <summary>
		/// All members of this type will be placed in this window .js file.
		/// </summary>
		public Type[] RunAtThis = Type.EmptyTypes;
	}

	/// <summary>
	/// Resources listed in this attribute will be loaded at tht beginning of method/constructor call.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false, AllowMultiple = true)]
	public sealed class LoadResourcesAttribute : Attribute {
		/// <summary>
		/// Types generated using System.Resources.Tools.StronglyTypedResourceBuilder.
		/// </summary>
		public readonly Type[] ResourceTypes;
		/// <param name="resourceTypes">Types generated using System.Resources.Tools.StronglyTypedResourceBuilder.</param>
		public LoadResourcesAttribute(params Type[] resourceTypes) {
			this.ResourceTypes = resourceTypes;
		}
	}
}
