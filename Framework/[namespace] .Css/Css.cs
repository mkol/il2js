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
using System.Drawing;
using MK.JavaScript.Framework;
using MK.JavaScript.Reflection;
namespace MK.JavaScript.Css {
	#region Enums
	/// <summary>list-style-position value type.</summary>
	public sealed class ListStylePosition : EnumString {
		private ListStylePosition(string value) : base(value) { }
		public static readonly ListStylePosition
		Empty = new ListStylePosition(""),
		Inside = new ListStylePosition("inside"),
		Outside = new ListStylePosition("outside"),
		Inherit = new ListStylePosition("inherit");
	}

	/// <summary>table-layout value type.</summary>
	public sealed class TableLayout : EnumString {
		private TableLayout(string value) : base(value) { }
		public static readonly TableLayout
		Empty = new TableLayout(""),
		Auto = new TableLayout("auto"),
		Fixed = new TableLayout("fixed"),
		Inherit = new TableLayout("inherit");
	}
	/// <summary>background-repeat value type.</summary>
	public sealed class BackgroundRepeat : EnumString {
		private BackgroundRepeat(string value) : base(value) { }
		public static readonly BackgroundRepeat
		Empty = new BackgroundRepeat(""),
		Repeat = new BackgroundRepeat("repeat"),
		RepeatX = new BackgroundRepeat("repeat-x"),
		RepeatY = new BackgroundRepeat("repeat-y"),
		NoRepeat = new BackgroundRepeat("no-repeat"),
		Inherit = new BackgroundRepeat("inherit");
	}
	/// <summary>display value type.</summary>
	public sealed class Display : EnumString {
		private Display(string value) : base(value) { }
		public static readonly Display
		Empty = new Display(""),
		Inline = new Display("inline"),
		Block = new Display("block"),
		ListItem = new Display("last-item"),
		RunIn = new Display("run-in"),
		Compact = new Display("compact"),
		Marker = new Display("marker"),
		Table = new Display("table"),
		InlineTable = new Display("inline-table"),
		TableRowGroup = new Display("table-row-group"),
		TableHeaderGroup = new Display("table-header-group"),
		TableFooterGroup = new Display("table-footer-group"),
		TableRow = new Display("table-row"),
		TableColumnGroup = new Display("table-column-group"),
		TableColumn = new Display("table-column"),
		TableCell = new Display("table-cell"),
		TableCaption = new Display("table-caption"),
		None = new Display("none"),
		Inherit = new Display("inherit");
	}
	/// <summary>cursor value type.</summary>
	public sealed class Cursor : EnumString {
		private Cursor(string value) : base(value) { }
		public static readonly Cursor
		Empty = new Cursor(""),
		Auto = new Cursor("auto"),
		CrossHair = new Cursor("crosshair"),
		Default = new Cursor("default"),
		Pointer = new Cursor("pointer"),
		Move = new Cursor("move"),
		EResize = new Cursor("e-resize"),
		NEResize = new Cursor("ne-resize"),
		NWResize = new Cursor("nw-resize"),
		NResize = new Cursor("n-resize"),
		SEResize = new Cursor("se-resize"),
		SWResize = new Cursor("sw-resize"),
		SResize = new Cursor("s-resize"),
		WResize = new Cursor("w-resize"),
		Text = new Cursor("text"),
		Wait = new Cursor("wait"),
		Help = new Cursor("help"),
		Inherit = new Cursor("inherit");
#warning <uri>
	}
	/// <summary>border-style value type.</summary>
	public sealed class BorderStyle : EnumString {
		private BorderStyle(string value) : base(value) { }
		public static readonly BorderStyle
		Empty = new BorderStyle(""),
		None = new BorderStyle("none"),
		Hidden = new BorderStyle("hidden"),
		Dotted = new BorderStyle("dotted"),
		Dashed = new BorderStyle("dashed"),
		Solid = new BorderStyle("solid"),
		Double = new BorderStyle("double"),
		Groove = new BorderStyle("groove"),
		Ridge = new BorderStyle("ridge"),
		Inset = new BorderStyle("inset"),
		Outset = new BorderStyle("outset"),
		Inherit = new BorderStyle("inherit");
	}
	/// <summary>visibility value type.</summary>
	public sealed class Visibility : EnumString {
		private Visibility(string value) : base(value) { }
		public static readonly Visibility
		Empty = new Visibility(""),
		Visible = new Visibility("visible"),
		Hidden = new Visibility("hidden"),
		Collapse = new Visibility("collapse"),
		Inherit = new Visibility("inherit");
	}
	/// <summary>float value type.</summary>
	public sealed class Float : EnumString {
		private Float(string value) : base(value) { }
		public static readonly Float
		Empty = new Float(""),
		Left = new Float("left"),
		Right = new Float("right"),
		None = new Float("none"),
		Inherit = new Float("inherit");
	}
	/// <summary>font-weigth value type.</summary>
	public sealed class FontWeight : EnumString {
		private FontWeight(string value) : base(value) { }
		public static readonly FontWeight
		Empty = new FontWeight(""),
		Normal = new FontWeight("normal"),
		Bold = new FontWeight("bold"),
		Bolder = new FontWeight("bolder"),
		Lighter = new FontWeight("lighter"),
		Value100 = new FontWeight("100"),
		Value200 = new FontWeight("200"),
		Value300 = new FontWeight("300"),
		Value400 = new FontWeight("400"),
		Value500 = new FontWeight("500"),
		Value600 = new FontWeight("600"),
		Value700 = new FontWeight("700"),
		Value800 = new FontWeight("800"),
		Value900 = new FontWeight("900"),
		Inherit = new FontWeight("inherit");
	}
	/// <summary>text-align value type.</summary>
	public sealed class TextAlign : EnumString {
		private TextAlign(string value) : base(value) { }
		public static readonly TextAlign
		Left = new TextAlign("left"),
		Right = new TextAlign("right"),
		Center = new TextAlign("center"),
		Justify = new TextAlign("justify"),
		Inherit = new TextAlign("inherit");
#warning <string>
	}
	/// <summary>vertical-aligh value type.</summary>
	public sealed class VerticalAlign : EnumString {
		private VerticalAlign(string value) : base(value) { }
		public static readonly VerticalAlign
			BaseLine = new VerticalAlign("baseline"),
			Sub = new VerticalAlign("sub"),
			Super = new VerticalAlign("super"),
			Top = new VerticalAlign("top"),
			TextTop = new VerticalAlign("text-top"),
			Middle = new VerticalAlign("middle"),
			Bottom = new VerticalAlign("bottom"),
			TextBottom = new VerticalAlign("text-bottom"),
			Inherit = new VerticalAlign("inherit");
#warning						<percentage> | <length>
	}
	/// <summary>position value type.</summary>
	public sealed class Position : EnumString {
		private Position(string value) : base(value) { }
		public static readonly Position
			Empty = new Position(""),
			Static = new Position("static"),
			Relative = new Position("relative"),
			Absolute = new Position("absolute"),
			Fixed = new Position("fixed"),
			Inherit = new Position("inherit");
	}
	/// <summary>overflow value type.</summary>
	public sealed class Overflow : EnumString {
		private Overflow(string value) : base(value) { }
		public static readonly Overflow
			Empty = new Overflow(""),
			Visible = new Overflow("visible"),
			Hidden = new Overflow("hidden"),
			Scroll = new Overflow("scroll"),
			Auto = new Overflow("auto"),
			Inherit = new Overflow("inherit");
	}
	/// <summary>font-variant value type.</summary>
	public sealed class FontVariant : EnumString {
		private FontVariant(string value) : base(value) { }
		public static readonly FontVariant
			Empty = new FontVariant(""),
			Normal = new FontVariant("normal"),
			SmallCaps = new FontVariant("small-caps"),
			Inherit = new FontVariant("inherit");
	}
	/// <summary>list-style-type value type.</summary>
	public sealed class ListStyleType : EnumString {
		private ListStyleType(string value) : base(value) { }
		public static readonly ListStyleType
			Empty = new ListStyleType(""),
			Disc = new ListStyleType("disc"),
			Circle = new ListStyleType("circle"),
			Square = new ListStyleType("square"),
			Decimal = new ListStyleType("decimal"),
			DecimalLeadingZero = new ListStyleType("decimal-leading-zero"),
			LowerRoman = new ListStyleType("lower-roman"),
			UpperRoman = new ListStyleType("upper-roman"),
			LowerGreek = new ListStyleType("lower-greek"),
			LowerAlpha = new ListStyleType("lower-alpha"),
			LowerLatin = new ListStyleType("lower-latin"),
			UpperAlpha = new ListStyleType("upper-alpha"),
			UpperLatin = new ListStyleType("upper-latin"),
			Hebrew = new ListStyleType("hebrew"),
			Armenian = new ListStyleType("armenian"),
			Georgian = new ListStyleType("georgian"),
			CjkIdeographic = new ListStyleType("cjk-ideographic"),
			Hiragana = new ListStyleType("hiragana"),
			Katakana = new ListStyleType("katakana"),
			HiraganaIroha = new ListStyleType("hiragana-iroha"),
			KatakanaIroha = new ListStyleType("katakana-iroha"),
			None = new ListStyleType("none"),
			Inherit = new ListStyleType("inherit");
	}
	/// <summary>text-decoration value type.</summary>
	public sealed class TextDecoration : EnumString {
		private TextDecoration(string value) : base(value) { }
		public static readonly TextDecoration
			Empty = new TextDecoration(""),
			None = new TextDecoration("none"),
			Underline = new TextDecoration("underline"),
			Overline = new TextDecoration("overline"),
			LineThrough = new TextDecoration("line-through"),
			Blink = new TextDecoration("blink"),
			Inherit = new TextDecoration("inherit");
	}

	#endregion
	/// <summary>
	/// Units for CSS lengths.
	/// </summary>
	public sealed class Unit : EnumString {
		private Unit(string value) : base(value) { }
		/// <summary>
		/// Millimeter (mm) unit.
		/// </summary>
		public static readonly Unit Millimeter = new Unit("mm");
		/// <summary>
		/// Centimeter (cm) unit.
		/// </summary>
		public static readonly Unit Centimeter = new Unit("cm");
		/// <summary>
		/// Pica (pc) unit.
		/// </summary>
		public static readonly Unit Pica = new Unit("pc");
		/// <summary>
		/// Inch (in) unit.
		/// </summary>
		public static readonly Unit Inch = new Unit("in");
		/// <summary>
		/// Pixel (px) unit.
		/// </summary>
		public static readonly Unit Pixel = new Unit("px");
		/// <summary>
		/// Point (pt) unit.
		/// </summary>
		public static readonly Unit Point = new Unit("pt");
		/// <summary>
		/// Percent (%) unit.
		/// </summary>
		public static readonly Unit Percent = new Unit("%");
		/// <summary>
		/// Adds unit to number.
		/// </summary>
		[JSNative(OpCode = "Add")]
		[Inline]
		public static Unit operator *(int number, Unit unit) { return new Unit(number.ToString() + unit.Value); }
		/// <summary>
		/// Adds unit to number.
		/// </summary>
		[JSNative(OpCode = "Add")]
		[Inline]
		public static Unit operator *(double number, Unit unit) { return new Unit(number.ToString() + unit.Value); }
		/// <summary>
		/// Equivalent for <c>value * Unit.Pixel;</c>;
		/// </summary>
		[Inline]
		public static implicit operator Unit(int value) { return value * Pixel; }
		/// <summary>
		/// Equivalent for <c>value * Unit.Pixel;</c>.
		/// </summary>
		[Inline]
		public static implicit operator Unit(double value) { return value * Pixel; }
	}

	/// <summary>
	/// CSS style wrapper.
	/// </summary>
	public sealed class Style {
		#region server side support
		private Dictionary<string, string> _values;
		private Dictionary<string, string> values {
			get {
				if (this._values == null)
					this._values = new Dictionary<string, string>();
				return this._values;
			}
		}
		#endregion

		#region
		[JSNative(Code = "this[$1]=$0")]
		private void setUnit(Unit value, string property) { this.values[property] = value.Value; }
		[JSNative(OpCode = "Ldelem")]
		private Unit getUnit(string property) { throw new InvalidOperationException(JS.CannotRunAtServer); }

#warning float vs. cssFloat
		[JSNative(Code = "this[$1]=$0")]
		private void setEnum<T>(T value, string property) where T : EnumString {
			this.values[property == "cssFloat" ? "float" : property] = value.Value;
		}
		[JSNative(OpCode = "Ldelem")]
		private T getEnum<T>(string property) where T : EnumString {
			throw new InvalidOperationException(JS.CannotRunAtServer);
		}

		[JSNative(Code = "this[$1]=$0")]
		private void setColor(Color value, string property) { this.values[property] = "#" + value.R.ToString("x2") + value.G.ToString("x2") + value.B.ToString("x2"); }
		[JSNative(OpCode = "Ldelem")]
		private Color getColor(string property) { throw new InvalidOperationException(JS.CannotRunAtServer); }

		[JSNative(Code = "this[$1]=$0")]
		private void setValue<T>(T value, string property) { this.values[property] = value.ToString(); }
		[JSNative(OpCode = "Ldelem")]
		private T getValue<T>(string property) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		#endregion

		#region Css units
		/// <summary>font-size.</summary>
		public Unit FontSize {
			[Inline]
			set { this.setUnit(value, "fontSize"); }
			[Inline]
			get { return this.getUnit("fontSize"); }
		}
		/// <summary>padding.</summary>
		public Unit Padding {
			[Inline]
			set { this.setUnit(value, "padding"); }
			[Inline]
			get { return this.getUnit("padding"); }
		}
		/// <summary>padding-right.</summary>
		public Unit PaddingRight {
			[Inline]
			set { this.setUnit(value, "paddingRight"); }
			[Inline]
			get { return this.getUnit("paddingRight"); }
		}
		/// <summary>padding-left.</summary>
		public Unit PaddingLeft {
			[Inline]
			set { this.setUnit(value, "paddingLeft"); }
			[Inline]
			get { return this.getUnit("paddingLeft"); }
		}
		/// <summary>padding-bottom.</summary>
		public Unit PaddingBottom {
			[Inline]
			set { this.setUnit(value, "paddingBottom"); }
			[Inline]
			get { return this.getUnit("paddingBottom"); }
		}
		/// <summary>padding-top.</summary>
		public Unit PaddingTop {
			[Inline]
			set { this.setUnit(value, "paddingTop"); }
			[Inline]
			get { return this.getUnit("paddingTop"); }
		}
		/// <summary>margin.</summary>
		public Unit Margin {
			[Inline]
			set { this.setUnit(value, "margin"); }
			[Inline]
			get { return this.getUnit("margin"); }
		}
		/// <summary>margin-right.</summary>
		public Unit MarginRight {
			[Inline]
			set { this.setUnit(value, "marginRight"); }
			[Inline]
			get { return this.getUnit("marginRight"); }
		}
		/// <summary>margin-left.</summary>
		public Unit MarginLeft {
			[Inline]
			set { this.setUnit(value, "marginLeft"); }
			[Inline]
			get { return this.getUnit("marginLeft"); }
		}
		/// <summary>margin-bottom.</summary>
		public Unit MarginBottom {
			[Inline]
			set { this.setUnit(value, "marginBottom"); }
			[Inline]
			get { return this.getUnit("marginBottom"); }
		}
		/// <summary>margin-top.</summary>
		public Unit MarginTop {
			[Inline]
			set { this.setUnit(value, "marginTop"); }
			[Inline]
			get { return this.getUnit("marginTop"); }
		}
		/// <summary>min-heigth.</summary>
		public Unit MinHeight {
			[Inline]
			set { this.setUnit(value, "minHeight"); }
			[Inline]
			get { return this.getUnit("minHeight"); }
		}
		/// <summary>min-width.</summary>
		public Unit MinWidth {
			[Inline]
			set { this.setUnit(value, "minWidth"); }
			[Inline]
			get { return this.getUnit("minWidth"); }
		}
		/// <summary>max-heigth.</summary>
		public Unit MaxHeight {
			[Inline]
			set { this.setUnit(value, "maxHeight"); }
			[Inline]
			get { return this.getUnit("maxHeight"); }
		}
		/// <summary>max-width.</summary>
		public Unit MaxWidth {
			[Inline]
			set { this.setUnit(value, "minWidth"); }
			[Inline]
			get { return this.getUnit("minWidth"); }
		}
		/// <summary>heigth.</summary>
		public Unit Height {
			[Inline]
			set { this.setUnit(value, "height"); }
			[Inline]
			get { return this.getUnit("height"); }
		}
		/// <summary>width.</summary>
		public Unit Width {
			[Inline]
			set { this.setUnit(value, "width"); }
			[Inline]
			get { return this.getUnit("width"); }
		}
		/// <summary>top.</summary>
		public Unit Top {
			[Inline]
			set { this.setUnit(value, "top"); }
			[Inline]
			get { return this.getUnit("top"); }
		}
		/// <summary>left.</summary>
		public Unit Left {
			[Inline]
			set { this.setUnit(value, "left"); }
			[Inline]
			get { return this.getUnit("left"); }
		}
		/// <summary>bottom.</summary>
		public Unit Bottom {
			[Inline]
			set { this.setUnit(value, "bottom"); }
			[Inline]
			get { return this.getUnit("bottom"); }
		}
		/// <summary>right.</summary>
		public Unit Right {
			[Inline]
			set { this.setUnit(value, "right"); }
			[Inline]
			get { return this.getUnit("right"); }
		}
		/// <summary>border-width.</summary>
		public Unit BorderWidth {
			[Inline]
			set { this.setUnit(value, "borderWidth"); }
			[Inline]
			get { return this.getUnit("borderWidth"); }
		}
		/// <summary>border-top-width.</summary>
		public Unit BorderTopWidth {
			[Inline]
			set { this.setUnit(value, "borderTopWidth"); }
			[Inline]
			get { return this.getUnit("borderTopWidth"); }
		}
		/// <summary>border-bottom-width.</summary>
		public Unit BorderBottomWidth {
			[Inline]
			set { this.setUnit(value, "borderBottomWidth"); }
			[Inline]
			get { return this.getUnit("borderBottomWidth"); }
		}
		/// <summary>border-left-width.</summary>
		public Unit BorderLeftWidth {
			[Inline]
			set { this.setUnit(value, "borderLeftWidth"); }
			[Inline]
			get { return this.getUnit("borderLeftWidth"); }
		}
		/// <summary>border-right-width.</summary>
		public Unit BorderRightWidth {
			[Inline]
			set { this.setUnit(value, "borderRightWidth"); }
			[Inline]
			get { return this.getUnit("borderRightWidth"); }
		}
		#endregion
		#region Css enums
		/// <summary>list-style-position.</summary>
		public ListStylePosition ListStylePosition {
			[Inline]
			set { this.setEnum(value, "listStylePosition"); }
			[Inline]
			get { return this.getEnum<ListStylePosition>("listStylePosition"); }
		}
		/// <summary>table-layout.</summary>
		public TableLayout TableLayout {
			[Inline]
			set { this.setEnum(value, "tableLayout"); }
			[Inline]
			get { return this.getEnum<TableLayout>("tableLayout"); }
		}
		/// <summary>background-repeat.</summary>
		public BackgroundRepeat BackgroundRepeat {
			[Inline]
			set { this.setEnum(value, "backgroundRepeat"); }
			[Inline]
			get { return this.getEnum<BackgroundRepeat>("backgroundRepeat"); }
		}
		/// <summary>position.</summary>
		public Position Position {
			[Inline]
			set { this.setEnum(value, "position"); }
			[Inline]
			get { return this.getEnum<Position>("position"); }
		}
#warning TextDecoration should be list type
		/// <summary>text-decoration.</summary>
		public TextDecoration TextDecoration {
			[Inline]
			set { this.setEnum(value, "textDecoration"); }
			[Inline]
			get { return this.getEnum<TextDecoration>("textDecoration"); }
		}
		/// <summary>list-style-type.</summary>
		public ListStyleType ListStyleType {
			[Inline]
			set { this.setEnum(value, "listStyleType"); }
			[Inline]
			get { return this.getEnum<ListStyleType>("listStyleType"); }
		}
		/// <summary>font-variant.</summary>
		public FontVariant FontVariant {
			[Inline]
			set { this.setEnum(value, "fontVariant"); }
			[Inline]
			get { return this.getEnum<FontVariant>("fontVariant"); }
		}
		/// <summary>display.</summary>
		public Display Display {
			[Inline]
			set { this.setEnum(value, "display"); }
			[Inline]
			get { return this.getEnum<Display>("display"); }
		}
		/// <summary>text-align.</summary>
		public TextAlign TextAlign {
			[Inline]
			set { this.setEnum(value, "textAlign"); }
			[Inline]
			get { return this.getEnum<TextAlign>("textAlign"); }
		}
		/// <summary>vertical-align.</summary>
		public VerticalAlign VerticalAlign {
			[Inline]
			set { this.setEnum(value, "verticalAlign"); }
			[Inline]
			get { return this.getEnum<VerticalAlign>("verticalAlign"); }
		}
#warning float vs. cssFloat
		/// <summary>float.</summary>
		public Float Float {
			[Inline]
			set { this.setEnum(value, "cssFloat"); }
			[Inline]
			get { return this.getEnum<Float>("cssFloat"); }
		}
		/// <summary>font-weigth.</summary>
		public FontWeight FontWeight {
			[Inline]
			set { this.setEnum(value, "fontWeight"); }
			[Inline]
			get { return this.getEnum<FontWeight>("fontWeight"); }
		}
		/// <summary>cursor.</summary>
		public Cursor Cursor {
			[Inline]
			set { this.setEnum(value, "cursor"); }
			[Inline]
			get { return this.getEnum<Cursor>("cursor"); }
		}
		/// <summary>visibility.</summary>
		public Visibility Visibility {
			[Inline]
			set { this.setEnum(value, "visibility"); }
			[Inline]
			get { return this.getEnum<Visibility>("visibility"); }
		}
		/// <summary>border-style.</summary>
		public BorderStyle BorderStyle {
			[Inline]
			set { this.setEnum(value, "borderStyle"); }
			[Inline]
			get { return this.getEnum<BorderStyle>("borderStyle"); }
		}
		/// <summary>border-top-style.</summary>
		public BorderStyle BorderTopStyle {
			[Inline]
			set { this.setEnum(value, "borderTopStyle"); }
			[Inline]
			get { return this.getEnum<BorderStyle>("borderTopStyle"); }
		}
		/// <summary>border-left-style.</summary>
		public BorderStyle BorderLeftStyle {
			[Inline]
			set { this.setEnum(value, "borderLeftStyle"); }
			[Inline]
			get { return this.getEnum<BorderStyle>("borderLeftStyle"); }
		}
		/// <summary>border-bottom-style.</summary>
		public BorderStyle BorderBottomStyle {
			[Inline]
			set { this.setEnum(value, "borderBottomStyle"); }
			[Inline]
			get { return this.getEnum<BorderStyle>("borderBottomStyle"); }
		}
		/// <summary>border-right-style.</summary>
		public BorderStyle BorderRightStyle {
			[Inline]
			set { this.setEnum(value, "borderRightStyle"); }
			[Inline]
			get { return this.getEnum<BorderStyle>("borderRightStyle"); }
		}
		/// <summary>overflow.</summary>
		public Overflow Overflow {
			[Inline]
			set { this.setEnum(value, "overflow"); }
			[Inline]
			get { return this.getEnum<Overflow>("overflow"); }
		}
		#endregion
		#region colors
		/// <summary>background-color.</summary>
		public Color BackgroundColor {
			[Inline]
			set { this.setColor(value, "backgroundColor"); }
			[Inline]
			get { return this.getColor("backgroundColor"); }
		}
		/// <summary>border-color.</summary>
		public Color BorderColor {
			[Inline]
			set { this.setColor(value, "borderColor"); }
			[Inline]
			get { return this.getColor("borderColor"); }
		}
		/// <summary>border-top-color.</summary>
		public Color BorderTopColor {
			[Inline]
			set { this.setColor(value, "borderTopColor"); }
			[Inline]
			get { return this.getColor("borderTopColor"); }
		}
		/// <summary>border-bottom-color.</summary>
		public Color BorderBottomColor {
			[Inline]
			set { this.setColor(value, "borderBottomColor"); }
			[Inline]
			get { return this.getColor("borderBottomColor"); }
		}
		/// <summary>border-left-color.</summary>
		public Color BorderLeftColor {
			[Inline]
			set { this.setColor(value, "borderLeftColor"); }
			[Inline]
			get { return this.getColor("borderLeftColor"); }
		}
		/// <summary>border-right-color.</summary>
		public Color BorderRightColor {
			[Inline]
			set { this.setColor(value, "borderRightColor"); }
			[Inline]
			get { return this.getColor("borderRightColor"); }
		}
		/// <summary>color.</summary>
		public Color Color {
			[Inline]
			set { this.setColor(value, "color"); }
			[Inline]
			get { return this.getColor("color"); }
		}
		#endregion
		#region other values
		/// <summary>background-image.</summary>
		public string BackgroundImage {
			[Inline]
			set { this.setValue(value, "backgroundImage"); }
			[Inline]
			get { return this.getValue<string>("backgroundImage"); }
		}
		/// <summary>background-position.</summary>
		public string BackgroundPosition {
			[Inline]
			set { this.setValue(value, "backgroundPosition"); }
			[Inline]
			get { return this.getValue<string>("backgroundPosition"); }
		}
		/// <summary>z-index.</summary>
		public int ZIndex {
			[Inline]
			set { this.setValue(value, "zIndex"); }
			[Inline]
			get { return this.getValue<int>("zIndex"); }
		}
		#endregion


		public Style() { }
	}


}
