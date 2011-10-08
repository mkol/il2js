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
using System.Drawing;
using System;
using MK.JavaScript.Css;

using HTMLElement = MK.JavaScript.Dom.Element;
using System.Collections.Generic;
using PrototypeJs;
using MK.JavaScript.Reflection;
using System.Collections;
using MK.JavaScript.Framework;
using MK.JavaScript.Framework.Mappings;
namespace MK.JavaScript.Dom {

#warning SelectOptionsList

	public class SelectOptionsList : IEnumerable<Option> {
		private SelectOptionsList() { }
		[JSNative]
		public void Add(Option option) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[JSNative]
		public void Add(Option option, int index) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		public Option this[int index] {
			[JSNative(OpCode = "Ldelem")]
			get { throw new InvalidOperationException(JS.CannotRunAtServer); }
			[JSNative(OpCode = "Stelem")]
			set { throw new InvalidOperationException(JS.CannotRunAtServer); }
		}
		[JSNative("length")]
		public int Count;

		#region IEnumerable<Option> Members
		public IEnumerator<Option> GetEnumerator() { return new JSArrayEnumerator<Option>(this); }
		#endregion
		#region IEnumerable Members
		IEnumerator IEnumerable.GetEnumerator() { return new JSArrayEnumerator<Option>(this); }
		#endregion
	}

	public class Option : Element {
		[JSNative]
		public bool DefaultSelected;
		[JSNative]
		public int Index;
		public bool Selected {
			[Get]
			get { return bool.Parse(this["selected"]); }
			[Set]
			set { this["selected"] = value.ToString(); }
		}
		/// <summary>
		/// TEXT attribute.
		/// </summary>
		public string Text {
			[Get]
			get { return this["text"]; }
			[Set]
			set { this["text"] = value; }
		}
		/// <summary>
		/// VALUE attribute.
		/// </summary>
		public new string Value {
			[Get]
			get { return this["value"]; }
			[Set]
			set { this["value"] = value; }
		}
		[JSNative]
		public Option() : base("option") { }
	}


	[PrototypeJS]
	public class Offset {
		[PrototypeJS]
		public int Top;
		[PrototypeJS]
		public int Left;

		private Offset() { }
	}
	/// <summary>
	/// BODY tag.
	/// </summary>
	public class Body : Element {
		private Body() : base("body") { }
	}
	/// <summary>
	/// Base class for HTML elements.
	/// </summary>
	public partial class Element : IEnumerable<Element> {

		#region IEnumerable<Element> Members
		[Inline]
		public IEnumerator<Element> GetEnumerator() { return (IEnumerator<Element>)this.ChildNodes.GetEnumerator(); }
		#endregion
		#region IEnumerable Members
		[Inline]
		IEnumerator IEnumerable.GetEnumerator() { return this.ChildNodes.GetEnumerator(); }
		#endregion
		#region ##### Node #####
		#region Properties
		/// <summary>
		/// Setting is equivalent for <value>this.Add(value)</value>.
		/// </summary>
		[JSNative]
		public Element[] ChildNodes {
			[Get]
			get { return this.elements.ToArray(); }
			[Inline]
			set { this.Add(value); }
		}
		[JSNative]
		public readonly Element NextSibling;
		[JSNative]
		public string NodeName;
		[JSNative]
		public readonly Element ParentNode;
		#endregion
		#region Methods
		[Obsolete("Use Add(element) instead.")]
		[JSNative]
		public void AppendChild(Element element) { this.Add(element); }

		#region Add - smart initialization support
		/// <summary>
		/// Equivalent for <c>this.Add(new Span { InnerHTML = html });</c>.
		/// </summary>
		/// <param name="html">InnerHTML of Span element.</param>
		public void Add(string html) { this.Add(new Span { InnerHTML = html }); }
		/// <summary>
		/// Equivalent for <c>foreach (var element in elements) this.Add(element);</c>.
		/// </summary>
		[Inline]
		public void Add(IEnumerable<Element> elements) {
			this.AddRange(elements);
		}
		/// <summary>
		/// Equivalent for <c>foreach (var element in elements) this.Add(element);</c>.
		/// </summary>
		public void Add(params Element[] elements) {
			foreach (var element in elements) {
				this.Add(element);
			}
		}
		#endregion

		/// <summary>
		/// Equivalent for <c>foreach (var element in elements) this.Add(element);</c>.
		/// </summary>
		public void AddRange(IEnumerable<Element> elements) {
			foreach (var element in elements) {
				this.Add(element);
			}
		}
		#endregion
		#endregion

		#region Properties
		[JSNative]
		public string TagName {
			[Get("tagName")]
			get;
			#region ServerSide
			private set;
			#endregion
		}

		#endregion

		#region Methods
		[Obsolete("Use element[key]; instead.")]
		[JSNative]
		public string GetAttribute(string key) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[Obsolete("Use element[key]=value; instead.")]
		[JSNative]
		public void SetAttribute(string key, string value) { }
		[JSNative]
		public void RemoveAttribute(string key) { }
		[JSNative]
		public void ScrollIntoView(bool alignToTop) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		#endregion



		#region PrototypeJS: Element (prototypejs.org/api/element.html)
		/// <summary>
		/// Returns the offsets of element from the top left corner of the document.
		/// </summary>
		public Offset CumulativeOffset { [PrototypeJS("cumulativeOffset")] get { throw new InvalidOperationException(JS.CannotRunAtServer); } }
		/// <summary>
		/// Sets the visual opacity of an element while working around inconsistencies in various browsers.
		/// The opacity argument should be a floating point number, where the value of 0 is fully transparent and 1 is fully opaque.
		/// </summary>
		public double Opacity { [PrototypeJS("setOpacity")]	set { throw new InvalidCastException(); } }
		/// <summary>
		/// Modifies element’s CSS style properties.
		/// Styles are passed as a hash of property-value pairs in which the properties are specified in their camelized form.
		/// </summary>
		public string StyleText { [Inline]	set { this.SetStyle(value); } }
		#endregion

		#region PrototypeJS: Form.Element (prototypejs.org/api/form/element.html)
		/// <summary>
		/// Returns the current value of a form control. A string is returned for most controls; only multiple select boxes return an array of values. The global shortcut for this method is _F().
		/// </summary>
		public string Value {
			[PrototypeJS("getValue")]
			get { return this["value"]; }
			[PrototypeJS("setValue")]
			set { this["value"] = value; }
		}
		/// <summary>
		/// Returns true if a text input has contents, false otherwise.
		/// </summary>
		public bool IsPresent { [PrototypeJS("present")] get { throw new InvalidOperationException(JS.CannotRunAtServer); } }
		#endregion

		[JSNative]
		public Element[] GetElementsByTagName(string tagName) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// ownerDocument
		/// </summary>
		public Document Document {
			[Get("ownerDocument")]
			get { throw new InvalidOperationException(JS.CannotRunAtServer); }
		}
		/// <summary>
		/// ID attribute.
		/// </summary>
		public string Id {
			[Get]
			get { return this["id"]; }
			[Set]
			set { this["id"] = value; }
		}
		/// <summary>
		/// TITLE attribute.
		/// </summary>
		public string Title {
			[Get]
			get { return this["title"]; }
			[Set]
			set { this["title"] = value; }
		}
		[JSNative]
		public string InnerHTML;

#warning niekonsystencja dom'u i javascriptu
		public string ClassName {
			[Get]
			get { return this["class"]; }
			[Set]
			set { this["class"] = value; }
		}

		/// <summary>
		/// Setting styles overrides element's style properties defined in value.
		/// </summary>
		public Style Style {
			[Get]
			get;
			[PrototypeJS("setStyle")]
			set;
		}
		[JSNative]
		public readonly int OffsetHeight;
		[JSNative]
		public readonly int OffsetWidth;
		#region Events
		public event JSEventHandler MouseOver {
			[Inline]
			add { Event.Observe(this, value, "mouseover"); }
			[Inline]
			remove { Event.StopObserving(this, value, "mouseover"); }
		}
		public event JSEventHandler MouseOut {
			[Inline]
			add { Event.Observe(this, value, "mouseout"); }
			[Inline]
			remove { Event.StopObserving(this, value, "mouseout"); }
		}
		public event JSEventHandler MouseDown {
			[Inline]
			add { Event.Observe(this, value, "mousedown"); }
			[Inline]
			remove { Event.StopObserving(this, value, "mousedown"); }
		}
		public event JSEventHandler MouseMove {
			[Inline]
			add { Event.Observe(this, value, "mousemove"); }
			[Inline]
			remove { Event.StopObserving(this, value, "mousemove"); }
		}
		public event JSEventHandler MouseUp {
			[Inline]
			add { Event.Observe(this, value, "mouseup"); }
			[Inline]
			remove { Event.StopObserving(this, value, "mouseup"); }
		}
		public event JSEventHandler KeyPress {
			[Inline]
			add { Event.Observe(this, value, "keypress"); }
			[Inline]
			remove { Event.StopObserving(this, value, "keypress"); }
		}
		public event JSEventHandler KeyDown {
			[Inline]
			add { Event.Observe(this, value, "keydown"); }
			[Inline]
			remove { Event.StopObserving(this, value, "keydown"); }
		}
		public event JSEventHandler Click {
			[Inline]
			add { Event.Observe(this, value, "click"); }
			[Inline]
			remove { Event.StopObserving(this, value, "click"); }
		}
		public event JSEventHandler DoubleClick {
			[Inline]
			add { Event.Observe(this, value, "dblclick"); }
			[Inline]
			remove { Event.StopObserving(this, value, "dblclick"); }
		}
		public event JSEventHandler Change {
			[Inline]
			add { Event.Observe(this, value, "change"); }
			[Inline]
			remove { Event.StopObserving(this, value, "change"); }
		}
		public event JSEventHandler Blur {
			[Inline]
			add { Event.Observe(this, value, "blur"); }
			[Inline]
			remove { Event.StopObserving(this, value, "blur"); }
		}

		/// <summary>Equivalent for <c>this.MouseOver += value;</c>.</summary>
		public JSEventHandler OnMouseOver { [Inline] set { this.MouseOver += value; } }
		/// <summary>Equivalent for <c>this.MouseOut += value;</c>.</summary>
		public JSEventHandler OnMouseOut { [Inline] set { this.MouseOut += value; } }
		/// <summary>Equivalent for <c>this.MouseDown += value;</c>.</summary>
		public JSEventHandler OnMouseDown { [Inline] set { this.MouseDown += value; } }
		/// <summary>Equivalent for <c>this.MouseMove += value;</c>.</summary>
		public JSEventHandler OnMouseMove { [Inline] set { this.MouseMove += value; } }
		/// <summary>Equivalent for <c>this.MouseUp += value;</c>.</summary>
		public JSEventHandler OnMouseUp { [Inline] set { this.MouseUp += value; } }
		/// <summary>Equivalent for <c>this.KeyPress += value;</c>.</summary>
		public JSEventHandler OnKeyPress { [Inline] set { this.KeyPress += value; } }
		/// <summary>Equivalent for <c>this.KeyDown += value;</c>.</summary>
		public JSEventHandler OnKeyDown { [Inline] set { this.KeyDown += value; } }
		/// <summary>Equivalent for <c>this.Click += value;</c>.</summary>
		public JSEventHandler OnClick { [Inline] set { this.Click += value; } }
		/// <summary>Equivalent for <c>this.DoubleClick += value;</c>.</summary>
		public JSEventHandler OnDoubleClick { [Inline] set { this.DoubleClick += value; } }
		/// <summary>Equivalent for <c>this.Change += value;</c>.</summary>
		public JSEventHandler OnChange { [Inline] set { this.Change += value; } }
		/// <summary>Equivalent for <c>this.Blur += value;</c>.</summary>
		public JSEventHandler OnBlur { [Inline] set { this.Blur += value; } }
		#endregion

		#region Element's children's
		#region Events
		public event JSEventHandler Load {
			[Inline]
			add { Event.Observe(this, value, "load"); }
			[Inline]
			remove { Event.StopObserving(this, value, "load"); }
		}
		/// <summary>Equivalent for <c>this.Load += value;</c>.</summary>
		public JSEventHandler OnLoad { [Inline] set { this.Load += value; } }

		#endregion

		public Window ContentWindow {
			[Get]
			get { throw new InvalidOperationException(JS.CannotRunAtServer); }
		}
		/// <summary>
		/// NAME attribute.
		/// </summary>
		public string Name {
			[Get]
			get { return this["name"]; }
			[Set]
			set { this["name"] = value; }
		}

		#endregion

	}
	public sealed class TableRules : EnumString {
		private TableRules(string value) : base(value) { }
		public static readonly TableRules
			Empty = new TableRules(""),
			All = new TableRules("all"),
			Cols = new TableRules("cols"),
			None = new TableRules("none"),
			Groups = new TableRules("groups"),
			Rows = new TableRules("rows");
	}
}
