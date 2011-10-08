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

namespace MK.JavaScript.Dom {
	/// <summary>
	/// HR tag.
	/// </summary>
	public class HorizontalRule : Element {
		public HorizontalRule() : base("hr") { }
	}
	/// <summary>
	/// BR tag.
	/// </summary>
	public class LineBreak : Element {
		public LineBreak() : base("br") { }
	}

	/// <summary>
	/// LABEL tag.
	/// </summary>
	public class Label : Element {
		public Label() : base("label") { }
#warning niekonsystencja dom i javascript
		/// <summary>
		/// FOR attribute.
		/// </summary>
		public string For {
			[Get("htmlFor")]
			get { return this["for"]; }
			[Set("htmlFor")]
			set { this["for"] = value; }
		}
	}
	/// <summary>
	/// PRE tag.
	/// </summary>
	public class PreformattedText : Element {
		public PreformattedText() : base("pre") { }
	}

	/// <summary>
	/// TABLE tag.
	/// </summary>
	public class Table : Element {
		public Table() : base("table") { }
		/// <summary>
		/// CELLPADDING attribute.
		/// </summary>
		public int CellPadding {
			[Get]
			get { return int.Parse(this["cellPadding"]); }
			[Set]
			set { this["cellPadding"] = value.ToString(); }
		}
		/// <summary>
		/// CELLSPACING attribute.
		/// </summary>
		public int CellSpacing {
			[Get]
			get { return int.Parse(this["cellSpacing"]); }
			[Set]
			set { this["cellSpacing"] = value.ToString(); }
		}
		/// <summary>
		/// BORDER attribute.
		/// </summary>
		public int Border {
			[Get]
			get { return int.Parse(this["border"]); }
			[Set]
			set { this["border"] = value.ToString(); }
		}
		/// <summary>
		/// RULES attribute.
		/// </summary>
		public TableRules Rules {
			[Get]
			get { return TableRules.Parse<TableRules>(this["rules"]); }
			[Set]
			set { this["rules"] = value.Value; }
		}
	}
	/// <summary>
	/// TEXTAREA tag.
	/// </summary>
	public class TextArea : Element {
		public TextArea() : base("textarea") { }
		/// <summary>
		/// Number of columns.
		/// </summary>
		public int Columns {
			[Get("cols")]
			get { return int.Parse(this["cols"]); }
			[Set("cols")]
			set { this["cols"] = value.ToString(); }
		}
		/// <summary>
		/// Number of rows.
		/// </summary>
		public int Rows {
			[Get]
			get { return int.Parse(this["rows"]); }
			[Set]
			set { this["rows"] = value.ToString(); }
		}
		/// <summary>
		/// READONLY attribute.
		/// </summary>
		public bool ReadOnly {
			[Get]
			get { return bool.Parse(this["readOnly"]); }
			[Set]
			set { this["readOnly"] = value.ToString(); }
		}
	}
	/// <summary>
	/// LI tag.
	/// </summary>
	public class ListItem : Element {
		public ListItem() : base("li") { }
	}
	/// <summary>
	/// UL tag.
	/// </summary>
	public class UnorderedList : Element {
		public UnorderedList() : base("ul") { }
	}
	/// <summary>
	/// THEAD tag.
	/// </summary>
	public class TableHeader : Element {
		public TableHeader() : base("thead") { }
	}
	/// <summary>
	/// TH tag.
	/// </summary>
	public class TableHeaderCell : Element {
		public TableHeaderCell() : base("th") { }
	}
	/// <summary>
	/// TBODY tag.
	/// </summary>
	public class TableBody : Element {
		public TableBody() : base("tbody") { }
	}
	/// <summary>
	/// TR tag.
	/// </summary>
	public class TableRow : Element {
		public TableRow() : base("tr") { }
	}
	/// <summary>
	/// TD tag.
	/// </summary>
	public class TableCell : Element {
		public TableCell() : base("td") { }
		/// <summary>
		/// COLSPAN attribute.
		/// </summary>
		public int ColumnSpan {
			[Get("colSpan")]
			get { return int.Parse(this["colSpan"]); }
			[Set("colSpan")]
			set { this["colSpan"] = value.ToString(); }
		}
		/// <summary>
		/// ROWSPAN attribute.
		/// </summary>
		public int RowSpan {
			[Get]
			get { return int.Parse(this["rowSpan"]); }
			[Set]
			set { this["rowSpan"] = value.ToString(); }
		}
	}
	/// <summary>
	/// DIV tag.
	/// </summary>
	public class Division : Element {
		public Division() : base("div") { }
	}
	/// <summary>
	/// SPAN tag.
	/// </summary>
	public class Span : Element {
		public Span() : base("span") { }
	}
	/// <summary>
	/// IMG tag.
	/// </summary>
	public class Image : Element {
		public Image() : base("img") { }
		/// <summary>
		/// SRC attribute.
		/// </summary>
		public string Source {
			[Get("src")]
			get { return this["src"]; }
			[Set("src")]
			set { this["src"] = value; }
		}
		/// <summary>
		/// ALT attribute.
		/// </summary>
		public string AlternativeText {
			[Get("alt")]
			get { return this["alt"]; }
			[Set("alt")]
			set { this["alt"] = value; }
		}
	}
	/// <summary>
	/// IFRAME tag.
	/// </summary>
	public class IFrame : Element {
		public IFrame() : base("iframe") { }
		/// <summary>
		/// SRC attribute.
		/// </summary>
		public string Source {
			[Get("src")]
			get { return this["src"]; }
			[Set("src")]
			set { this["src"] = value; }
		}

	}
	/// <summary>
	/// A tag.
	/// </summary>
	public class Anchor : Element {
		/// <summary>
		/// HREF attribute.
		/// </summary>
		public string Href {
			[Get]
			get { return this["href"]; }
			[Set]
			set { this["href"] = value; }
		}
		public Anchor() : base("a") { }
		/// <summary>
		/// TARGET attribute.
		/// </summary>
		public string Target {
			[Get]
			get { return this["target"]; }
			[Set]
			set { this["target"] = value; }
		}
	}
	/// <summary>
	/// INPUT tag.
	/// </summary>
	public class Input : Element {

		public string Type {
			[Get]
			get { return this["type"]; }
			[Set]
			set { this["type"] = value; }
		}
		private Input() : base("input") { }
		[Obsolete("If possible, call one of this class subclasses constructor instead of explicit calling this constructor.")]
		public Input(string type)
			: base("input") {
			this.Type = type;
		}
		#region subclasses
		/// <summary>
		/// INPUT tag with TYPE="text" attribute.
		/// </summary>
		public class Text : Input {
			public int MaxLength {
				[Get]
				get { return int.Parse(this["maxLength"]); }
				[Set]
				set { this["maxLength"] = value.ToString(); }
			}
			public Text() : base("text") { }
		}
		/// <summary>
		/// INPUT tag with TYPE="text" attribute.
		/// </summary>
		public class Password : Input {
			public Password() : base("password") { }
		}
		/// <summary>
		/// INPUT tag with TYPE="text" attribute.
		/// </summary>
		public class Reset : Input {
			public Reset() : base("reset") { }
		}
		/// <summary>
		/// INPUT tag with TYPE="submit" attribute.
		/// </summary>
		public class Submit : Input {
			public Submit() : base("submit") { }
		}
		/// <summary>
		/// INPUT tag with TYPE="image" attribute.
		/// </summary>
		public class Image : Input {
			public Image() : base("image") { }
		}
		/// <summary>
		/// INPUT tag with TYPE="hidden" attribute.
		/// </summary>
		public class Hidden : Input {
			public Hidden() : base("hidden") { }
		}
		/// <summary>
		/// INPUT tag with TYPE="file" attribute.
		/// </summary>
		public class FileUpload : Input {
			public FileUpload() : base("file") { }
		}
		/// <summary>
		/// INPUT tag with TYPE="button" attribute.
		/// </summary>
		public class Button : Input {
			public Button() : base("button") { }
		}
		/// <summary>
		/// INPUT tag with TYPE="checkbox" attribute.
		/// </summary>
		public class CheckBox : Input {
			public CheckBox() : base("checkbox") { }
			/// <summary>
			/// CHECKED attribute.
			/// </summary>
			public bool Checked {
				[Get]
				get { return bool.Parse(this["checked"]); }
				[Set]
				set { this["checked"] = value.ToString(); }
			}
		}
		/// <summary>
		/// INPUT tag with TYPE="Input" attribute.
		/// </summary>
		public class Radio : Input {
			/// <summary>
			/// CHECKED attribute.
			/// </summary>
			public bool Checked {
				[Get]
				get { return bool.Parse(this["checked"]); }
				[Set]
				set { this["checked"] = value.ToString(); }
			}
			public Radio() : base("radio") { }
		}
		#endregion
	}
	/// <summary>
	/// SELECT tag.
	/// </summary>
	public class Select : Element {
		public Select() : base("select") { }
		public int SelectedIndex {
			[Get]
			get { throw new InvalidOperationException(JS.CannotRunAtServer); }
			[Set]
			set { throw new InvalidOperationException(JS.CannotRunAtServer); }
		}
		[JSNative]
		public readonly SelectOptionsList Options;
		[JSNative]
		public void Remove(int index) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		public string SelectedText { get { return this.Options[this.SelectedIndex].Text; } }

	}

}
