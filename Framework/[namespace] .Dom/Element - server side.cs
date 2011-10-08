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
using System.Reflection;
using MK.JavaScript.Ajax.Serialization;
using System.Text.RegularExpressions;
using MK.JavaScript.Compiler;

namespace MK.JavaScript.Dom {
	partial class Element {
		#region Client & Server side methods
		/// <summary>
		/// Creates DOM element with given tag name.
		/// </summary>
		/// <param name="name">The tag name of element.</param>
		[Obsolete("If possible, call one of this class subclasses constructor instead of explicit calling this constructor.")]
		[PrototypeJS]
		public Element(string tagName) {
			this.TagName = tagName;
			this.elements = new List<Element>();
			this.attributes = new Dictionary<string, string>();
		}
		/// <summary>
		/// Adds element as a child of this element.
		/// </summary>
		/// <param name="element">An element to add.</param>
		[JSNative("appendChild")]
		public void Add(Element element) {
			this.elements.Add(element);
		}
		[JSNative]
		public void InsertBefore(Element what, Element where) {
			this.elements.Insert(this.elements.IndexOf(where), what);
		}
		[JSNative]
		public Element RemoveChild(Element child) {
			this.elements.Remove(child);
			return child;
		}
		public Element FirstChild {
			[Get]
			get { return this.elements[0]; }
		}
		[JSNative]
		public Element LastChild {
			[Get]
			get { return this.elements[this.elements.Count - 1]; }
		}
		/// <summary>
		/// Gets or sets a value of element's attribute.
		/// </summary>
		/// <param name="key">Name of attribute.</param>
		/// <returns></returns>
		public string this[string key] {
			[JSNative("getAttribute")]
			get {
				string value;
				return this.attributes.TryGetValue(key, out value) ? value : null;
			}
			[JSNative("setAttribute")]
			set { this.attributes[key] = value; }
		}

		public override string ToString() {
			var sb = new StringBuilder();
			this.append(sb);
			return sb.ToString();
		}
		public static string ToString(IEnumerable<Element> elements) {
			var sb = new StringBuilder();
			foreach (var item in elements) {
				item.append(sb);
			}
			return sb.ToString();
		}
		#endregion




		internal readonly List<Element> elements;
		private readonly Dictionary<string, string> attributes;

		private static string escapeHtml(string s) {
			return Regex.Replace(s, @"[""&<]", match => {
				switch (match.Value) {
					case "\"": return "&quot;";
					case "&": return "&amp;";
					case "<": return "&lt;";
					default: throw new ArgumentException();
				}
			});
		}

		internal void append(StringBuilder sb) {
			sb.Append('<').Append(this.TagName);
			foreach (var attr in this.attributes) {
				sb.Append(' ').Append(attr.Key).Append("=\"").Append(escapeHtml(attr.Value)).Append('"');
			}
			//if (this.Style != null) {
			//  sb.Append(" style=\"");
			//  CssWriter.append(this.Style, sb);
			//  sb.Append('"');
			//}
			if (this.InnerHTML != null) {
				sb.Append('>').Append(this.InnerHTML).Append("</").Append(this.TagName).Append('>');
			} else if (this.elements.Count != 0) {
				sb.Append('>');
				foreach (var element in this.elements) {
					element.append(sb);
				}
				sb.Append("</").Append(this.TagName).Append('>');
			} else {
				sb.Append("/>");
			}
		}
	}
}
