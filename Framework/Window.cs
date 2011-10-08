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
using System.Xml.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using MK.JavaScript.Dom;
using PrototypeJs;
using MK.JavaScript.Reflection;
using System.Linq;

namespace MK.JavaScript {


	public class Location {
		[JSNative("search")]
		public readonly string QueryString;
		[JSNative("pathname")]
		public readonly string Path;

		public string Href { [Get] get; [Set] set; }
		private Location() { }
	}
	public class Document {

		public string Title { [Get] get; [Set] set; }
		public string Cookie { [Get] get; [Set] set; }


		public Element Body { [Get] get { throw new InvalidOperationException(JS.CannotRunAtServer); } }
		public Element[] Links { [Get] get { throw new InvalidOperationException(JS.CannotRunAtServer); } }

		[JSNative]
		public Element[] GetElementsByTagName(string tagName) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		internal Document() { }
	}
	/// <summary>
	/// JavaScript event's data.
	/// </summary>
	public class JSEventArgs {
		#region PrototypeJS: Event (prototypejs.org/api/event.html)
		/// <summary>
		/// Returns the DOM element on which the event occurred.
		/// </summary>
		public Element Element { [PrototypeJS("element")]get { throw new InvalidOperationException(JS.CannotRunAtServer); } }
		/// <summary>
		/// Returns the absolute horizontal position for a mouse event.
		/// </summary>
		public int PointerX { [PrototypeJS("pointerX")]get { throw new InvalidOperationException(JS.CannotRunAtServer); } }
		/// <summary>
		/// Returns the absolute vertical position for a mouse event.
		/// </summary>
		public int PointerY { [PrototypeJS("pointerY")]get { throw new InvalidOperationException(JS.CannotRunAtServer); } }
		#endregion
		[JSNative]
		public int Which;
		[JSNative]
		public KeyCode KeyCode;
	}
	/// <summary>
	/// Represents the method that will handle an JavaScript event.
	/// </summary>
	/// <param name="args">JavaScript event.</param>
	public delegate void JSEventHandler(JSEventArgs args);

	/// <summary>
	/// Window (page) class.
	/// </summary>
	//[JSNative]
	public abstract partial class Window : JS {
		#region to dawniej bylo w _window....
		[JSNative(GlobalMethod = true)]
		public static void Alert(string message) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[JSNative(GlobalMethod = true)]
		public static string Prompt(string message) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[JSNative(GlobalMethod = true)]
		public static string Prompt(string message, string defaultValue) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[JSNative(GlobalMethod = true)]
		public static Window Open(String url) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[JSNative(GlobalMethod = true)]
		public static Window Open(String url, String name) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[JSNative(GlobalMethod = true)]
		public static Window Open(String url, String name, String features) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		[JSNative(GlobalMethod = true)]
		public static Window Open(String url, String name, String features, Boolean replace) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <remarks>
		/// This method is obsolote. Instead of <c>Window.SetTimeout(func,timeout)</c>, use
		/// <code>
		/// new Thread(()=>{
		///		Thread.Sleep(timeout);
		///		func();
		/// }).Start();
		/// </code>
		/// </remarks>
		[Obsolete("See remarks.")]
		[JSNative(Code = "setTimeout($handler$($0),$1)", GlobalMethod = true)]
		public static int SetTimeout(Action func, int timeout) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <remarks>
		/// This method is obsolote. Instead of <c>Window.SetInterval(func,timeout)</c>, use
		/// <code>
		/// new Thread(() => {
		///		while (true) {
		///			Thread.Sleep(timeout);
		///			func();
		///		}
		/// }).Start();
		/// </code>
		/// </remarks>
		[Obsolete("See remarks.")]
		[JSNative(Code = "setInterval($handler$($0),$1)", GlobalMethod = true)]
		public static int SetInterval(Action func, int interval) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <remarks>
		/// This method is obsolote. Instead of 
		/// <code>
		/// var id=Window.SetTimeout(func,timeout);
		/// //...
		/// Window.ClearTimeout(id);
		/// </code>
		/// use
		/// <code>
		/// var thread=new Thread(()=>{
		///		Thread.Sleep(timeout);
		///		func();
		/// };
		/// thread.Start();
		/// //...
		/// thread.Abort();
		/// </code>
		/// </remarks>
		[Obsolete("See remarks.")]
		[JSNative(GlobalMethod = true)]
		public static void ClearTimeout(int timeout) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <remarks>
		/// This method is obsolote. Instead of 
		/// <code>
		/// var id=Window.SetInterval(func,timeout);
		/// //...
		/// Window.ClearInterval(id);
		/// </code>
		/// use
		/// <code>
		/// var thread=new Thread(()=>{
		///		while(true){
		///			Thread.Sleep(timeout);
		///			func();
		///		}
		/// };
		/// thread.Start();
		/// //...
		/// thread.Abort();
		/// </code>
		/// </remarks>
		[Obsolete("See remarks.")]
		[JSNative(GlobalMethod = true)]
		public static void ClearInterval(int timeout) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		[JSNative]
		public static readonly Window Top;

		/// <summary>
		/// Returns the passed element. Element returned by the function is extended with Prototype DOM extensions.
		/// </summary>
		[PrototypeJS("$", GlobalMethod = true)]
		public static T _<T>(T element) where T : Element { throw new InvalidOperationException(JS.CannotRunAtServer); }
		#endregion

		public string Title {
			[Inline]
			get { return this.Document.Title; }
			set { this.Document.Title = value; }
		}

		/// <summary>
		/// Returns the element in the document with matching ID. Element returned by the function is extended with Prototype DOM extensions.
		/// </summary>
		[PrototypeJS("$")]
		public Element _(string id) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Returns the element in the document with matching ID. Element returned by the function is extended with Prototype DOM extensions.
		/// </summary>
		[PrototypeJS("$")]
		public T _<T>(string id) where T : Element { throw new InvalidOperationException(JS.CannotRunAtServer); }
		#region PrototypeJS: Utility (prototypejs.org/api/utility.html)
		/// <summary>
		/// Takes an arbitrary number of CSS selectors (strings) and returns a document-order array of extended DOM elements that match any of them.
		/// </summary>
		[PrototypeJS("$$")]
		public Element[] __(params string[] cssRule) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		#endregion


		public Window Parent { [Get] get { throw new InvalidOperationException(JS.CannotRunAtServer); } }
		public Dictionary<string, Window> Frames { [Get] get { throw new InvalidOperationException(JS.CannotRunAtServer); } }
		public Location Location { [Get] get { throw new InvalidOperationException(JS.CannotRunAtServer); } }

		public Document Document { [Get] get; private set; }


		#region Events
		public event JSEventHandler Resize {
			[Inline]
			add { Event.Observe(this, value, "resize"); }
			[Inline]
			remove { Event.StopObserving(this, value, "resize"); }
		}
		public event JSEventHandler Load {
			[Inline]
			add { Event.Observe(this.Document, value, "dom:loaded"); }
			[Inline]
			remove { Event.StopObserving(this.Document, value, "dom:loaded"); }
		}
		public event JSEventHandler Unload {
			[Inline]
			add { Event.Observe(this, value, "unload"); }
			[Inline]
			remove { Event.StopObserving(this, value, "unload"); }
		}
		#endregion


		[JSNative(Ignore = true)]
		protected Window() {
			this.Document = new Document();
		}
	}
	/// <summary>
	/// Window that have Top.
	/// </summary>
	/// <remarks>
	/// To allow communication between frames, all frames' classes must inherit from Window&lt;T> for common T.
	/// </remarks>
	/// <typeparam name="T">Top window type</typeparam>
	[JSNative]
	public abstract class Window<T> : Window where T : Window {
		/// <summary>
		/// Top window.
		/// </summary>
		[JSNative]
		public readonly new T Top;
		[JSNative(Ignore = true)]
		protected Window() { }
	}


}


