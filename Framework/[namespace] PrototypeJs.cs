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
using System.Text;
using MK.JavaScript.Dom;
using MK.JavaScript;
using MK.JavaScript.Reflection;


using HTMLElement = MK.JavaScript.Dom.Element;

namespace PrototypeJs {
	[PrototypeJS("Prototype.Browser")]
	public static class Browser {
		[PrototypeJS("IE")]
		public static readonly bool IE;
		[PrototypeJS("Gecko")]
		public static readonly bool Gecko;
		[PrototypeJS("Opera")]
		public static readonly bool Opera;
		[PrototypeJS("WebKit")]
		public static readonly bool WebKit;
		[PrototypeJS("MobileSafari")]
		public static readonly bool MobileSafari;
	}
	/// <summary>
	/// Methods from prototypejs.org/api/object.html.
	/// </summary>
	[PrototypeJS("Object")]
	public static class _Object {
		#region PrototypeJS: Object (prototypejs.org/api/object.html)
		//generics to avoid boxing/unboxing

		/// <summary>
		/// Returns a JSON string.
		/// </summary>
		[PrototypeJS]
		public static string ToJSON<T>(this T obj) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Returns true if obj is an array, false otherwise.
		/// </summary>
		[PrototypeJS]
		public static bool IsArray<T>(this T obj) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Returns true if obj is of type string, false otherwise.
		/// </summary>
		[PrototypeJS]
		public static bool IsString<T>(this T obj) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Returns true if obj is of type number, false otherwise.
		/// </summary>
		[PrototypeJS]
		public static bool IsNumber<T>(this T obj) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		#endregion
	}
	/// <summary>
	/// Methods from prototypejs.org/api/string.html.
	/// </summary>
	public static class _String {
		#region PrototypeJS: String (prototypejs.org/api/string.html)
		/// <summary>
		/// Converts HTML special characters to their entity equivalents.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static string EscapeHTML(this string self) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		#endregion
	}
	/// <summary>
	/// Methods from prototypejs.org/api/element.html.
	/// </summary>
	public static class _Element {
		#region PrototypeJS: Element (prototypejs.org/api/element.html)
		/// <summary>
		/// Removes element’s CSS className and returns element.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement RemoveClassName(this HTMLElement element, string className) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Adds a CSS class to element.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement AddClassName(this HTMLElement element, string className) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Takes an arbitrary number of CSS selectors (strings) and returns an array of extended descendants of element that match any of them.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement[] Select(this HTMLElement element, params string[] selector) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Hides and returns element.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement Hide(this HTMLElement element) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Displays and returns element.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement Show(this HTMLElement element) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Modifies element’s CSS style properties. Styles are passed as a hash of property-value pairs in which the properties are specified in their camelized form.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement SetStyle(this HTMLElement element, string styles) {
			element["style"] = styles;
			return element;
		}
		/// <summary>
		/// Checks whether element has the given CSS className.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static bool HasClassName(this HTMLElement element, string className) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Inserts content before, after, at the top of, or at the bottom of element, as specified by the position property of the second argument. If the second argument is the content itself, insert will append it to element.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement Insert(this HTMLElement element, Dictionary<string, HTMLElement> content) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		#endregion
	}
	/// <summary>
	/// Methods from prototypejs.org/api/form/element.html.
	/// </summary>
	public static class _Form_Element {
		#region PrototypeJS: Form.Element (prototypejs.org/api/form/element.html)
		/// <summary>
		/// Gives focus to a form control and selects its contents if it is a text input.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement Activate(this HTMLElement element) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Clears the contents of a text input.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement Clear(this HTMLElement element) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Disables a form control, effectively preventing its value to be changed until it is enabled again.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement Disable(this HTMLElement element) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Enables a previously disabled form control.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement Enable(this HTMLElement element) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Gives keyboard focus to an element.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement Focus(this HTMLElement element) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Selects the current text in a text input.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static HTMLElement Select(this HTMLElement element) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Creates an URL-encoded string representation of a form control in the name=value format.
		/// </summary>
		[PrototypeJS(CallType = NativeCallType.Instance)]
		public static string Serialize(this HTMLElement element) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		#endregion
	}
	public enum KeyCode {
		Backspace = 8,
		Tab = 9,
		Return = 13,
		Escape = 27,
		Left = 37,
		Up = 38,
		Right = 39,
		Down = 40,
		Delete = 46,
		Home = 36,
		End = 35,
		PageUp = 33,
		PageDown = 34,
		Insert = 45,
	}

	[PrototypeJS]
	public static class Event {

		///// <summary>
		///// In order to use this function, add following to prototype.js:
		///// <para>version 1.6.0.3 (line 4052)</para>
		///// <![CDATA[
		///// //@author(mkol@smp.if.uj.edu.pl){
		/////		call:function(e,eventName){
		/////			var list=null;
		/////			if((list=cache[getEventID(e)])&&(list=list[eventName])){
		/////				var event={element:function(){return e}};
		/////				list.each(function(wrapper) {
		/////					wrapper.handler.call(e,event);
		/////	      });				
		/////			}
		/////      return e;
		/////		},
		///// //@}
		///// ]]>
		///// <para>version 1.7</para>
		///// Add content of https://github.com/kangax/protolicious/blob/5b56fdafcd7d7662c9d648534225039b2e78e371/event.simulate.js changing `simulate` to `call` to prototype.js
		///// </summary>

		/// <summary>
		/// In order to use this function, add content of https://github.com/kangax/protolicious/blob/5b56fdafcd7d7662c9d648534225039b2e78e371/event.simulate.js changing `simulate` to `call` to prototype.js
		/// </summary>
		[PrototypeJS]
		public static Element Call(Element element, string eventName) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Stops the event’s propagation and prevents its default action from being triggered eventually.
		/// </summary>
		[PrototypeJS]
		public static void Stop(JSEventArgs args) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		[PrototypeJS(Code = "Event.observe($0,$2,$1)")]
		public static void Observe(object obj, JSEventHandler handler, string eventName) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[PrototypeJS(Code = "Event.stopObserving($0,$2,$1)")]
		public static void StopObserving(object obj, JSEventHandler handler, string eventName) { throw new InvalidOperationException(JS.CannotRunAtServer); }

	}

	[PrototypeJS]
	public static class Ajax {
		[PrototypeJS]
		public static class Responders {
			[PrototypeJS(
				Code = @"
function(a,b,c){
	c={};
	c['on'+b]=$handler$(a);
	Ajax.Responders.register(c)
}
"
			)]
			public static void Register(Action handler, string eventName) { throw new InvalidOperationException(JS.CannotRunAtServer); }
#warning public static void Unregister(Action handler, string eventName) { throw new Exception("Not supported by .net2js framework"); }
			[PrototypeJS]
			public static void Unregister(Action handler, string eventName) { throw new Exception("Not supported by .net2js framework"); }
		}
	}
}
