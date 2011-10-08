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
using MK.JavaScript.Css.Selectors;
using MK.JavaScript.Dom;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MK.JavaScript.Css {


	/// <summary>
	/// Adds static .css files to all pages that may invoke the attribute target method or constructor.
	/// </summary>
	/// <remarks>
	/// If method is called from module - no css link is added.
	/// </remarks>
	[global::System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false, AllowMultiple = true)]
	public sealed class ImportAttribute : Attribute {
		public readonly Type[] CssTypes;
		/// <param name="cssTypes">Subclasses of StyleSheet class.</param>
		public ImportAttribute(params Type[] cssTypes) {
			this.CssTypes = cssTypes;
		}
	}
	/// <summary>
	/// CSS StyleSheet class. Used as type parameter of ImportAttribute.
	/// </summary>
	public abstract class StyleSheet {
		/// <summary>
		/// Returns CSS rules of a StyleSheet.
		/// </summary>
		/// <returns>CSS rules.</returns>
		public abstract IEnumerable<Rule> GetRules();
	}
	/// <summary>
	/// Types of attibute-related selectors.
	/// </summary>
	public enum AttributeSelectorType {
		/// <summary>
		/// Used for `[attributeName=value]` selector.
		/// </summary>
		EqualsTo,
		/// <summary>
		/// Used for `[attributeName~=value]` selector.
		/// </summary>
		Contains,
		/// <summary>
		/// Used for `[attributeNam|=value]` selector.
		/// </summary>
		StartsWith,
	}
	/// <summary>
	/// Pseudo-classes for selectors.
	/// </summary>
	public enum PseudoClass {
		FirstChild,
		Link, Visited,//The link pseudo-classes
		Hover, Active, Focus,//The dynamic pseudo-classes
	}
	/// <summary>
	/// CSS selector.
	/// </summary>
	public abstract class Selector {

		internal Selector() { }
		internal abstract void AppendTo(StringBuilder sb);

		/// <summary>
		/// Returns CSS representation of selector.
		/// </summary>
		/// <returns>CSS representation of selector.</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			this.AppendTo(sb);
			return sb.ToString();
		}


		#region
		/// <summary>
		/// `selector,this` selector.
		/// </summary>
		/// <param name="selector">A selector.</param>
		/// <returns>`selector,this` selector.</returns>
		public Selector Or(Selector selector) {
			return new SelectorsCascade(selector, SelectorOperator.Alternative, this);
		}
		/// <summary>
		/// `selector+this` selector.
		/// </summary>
		/// <param name="selector">A selector.</param>
		/// <returns>`selector+this` selector.</returns>
		public Selector PrecededBy(Selector selector) {
			return new SelectorsCascade(selector, SelectorOperator.PrecendantOf, this);
		}
		/// <summary>
		/// `selector>this` selector.
		/// </summary>
		/// <param name="selector">A selector.</param>
		/// <returns>`selector>this` selector.</returns>
		public Selector ChildOf(Selector selector) {
			return new SelectorsCascade(selector, SelectorOperator.ParentOf, this);
		}
		/// <summary>
		/// `selector this` selector.
		/// </summary>
		/// <param name="selector">A selector.</param>
		/// <returns>`selector this` selector.</returns>
		public Selector DescencantOf(Selector selector) {
			return new SelectorsCascade(selector, SelectorOperator.AncestorOf, this);
		}
		#endregion

		#region class selector
		/// <summary>
		/// `this.className` selector.
		/// </summary>
		/// <param name="className">A class name.</param>
		/// <returns>`this.className` selector.</returns>
		public Selector WithClass(string className) { return new ClassSelector(this, className); }
		/// <summary>
		/// `selector.className` selector.
		/// </summary>
		/// <param name="selector">A selector.</param>
		/// <param name="className">A class name.</param>
		/// <returns>`selector.className` selector.</returns>
		public static Selector operator *(Selector selector, string className) { return new ClassSelector(selector, className); }
		/// <summary>
		/// `.className` selector.
		/// </summary>
		/// <param name="className">A class name.</param>
		/// <returns>`.className` selector.</returns>
		public static Selector Class(string className) { return new ClassSelector(null, className); }
		#endregion
		#region attribute selector
		/// <summary>
		/// `this[attributeName]` selector.
		/// </summary>
		/// <param name="attributeName">An attribute name.</param>
		/// <returns>`this[attributeName]` selector.</returns>
		public Selector WithAttribute(string attributeName) {
			return new AttributeSelector(this, attributeName);
		}
		/// <summary>
		/// `this[attributeName type value]` selector.
		/// </summary>
		/// <param name="attributeName">An attribute name.</param>
		/// <param name="type">An attribute selector type.</param>
		/// <param name="value">A value.</param>
		/// <returns>`this[attributeName type value]` selector.</returns>
		public Selector WithAttribute(string attributeName, AttributeSelectorType type, string value) {
			return new AttributeSelector2(this, attributeName, type, value);
		}

		/// <summary>
		/// `this[attributeName]` selector.
		/// </summary>
		/// <param name="attributeName">An attribute name.</param>
		/// <returns>`this[attributeName]` selector.</returns>
		public Selector this[string attributeName] {
			get { return new AttributeSelector(this, attributeName); }
		}
		/// <summary>
		/// `this[attributeName type value]` selector.
		/// </summary>
		/// <param name="attributeName">An attribute name.</param>
		/// <param name="type">An attribute selector type.</param>
		/// <param name="value">A value.</param>
		/// <returns>`this[attributeName type value]` selector.</returns>
		public Selector this[string attributeName, AttributeSelectorType type, string value] {
			get { return new AttributeSelector2(this, attributeName, type, value); }
		}
		#endregion
		#region pseudo-class selector
		/// <summary>
		/// `this:pseudoClass` selector.
		/// </summary>
		/// <param name="pseudoClass">A pseudo-class.</param>
		/// <returns>`this:pseudoClass` selector.</returns>
		public Selector WithPseudoClass(PseudoClass pseudoClass) {
			return new PseudoClassSelector(this, pseudoClass);
		}
		/// <summary>
		/// `this:pseudoClass` selector.
		/// </summary>
		/// <param name="pseudoClass">A pseudo-class.</param>
		/// <returns>`this:pseudoClass` selector.</returns>
		public Selector this[PseudoClass pseudoClass] {
			get { return new PseudoClassSelector(this, pseudoClass); }
		}
		#endregion
		#region type selector
		[Obsolete("Use Selector.WithType<> if possible.")]
		public static implicit operator Selector(Type elementType) { return new TypeSelector(elementType); }
		public static Selector Type<E>() where E : Element { return new TypeSelector(typeof(E)); }
		#endregion
		#region id selector
		/// <summary>
		/// `#id` selector.
		/// </summary>
		/// <param name="id">An id.</param>
		/// <returns>`#id` selector.</returns>
		public static Selector Id(string id) { return new IdSelector(id); }
		#endregion

		#region operators

		/// <summary>
		/// `a,b` selector.
		/// </summary>
		/// <param name="a">A selector.</param>
		/// <param name="b">A selector.</param>
		/// <returns>`a,b` selector.</returns>
		public static Selector operator |(Selector a, Selector b) {
			return new SelectorsCascade(a, SelectorOperator.Alternative, b);
		}
		/// <summary>
		/// `a+b` selector.
		/// </summary>
		/// <param name="a">A selector.</param>
		/// <param name="b">A selector.</param>
		/// <returns>`a+b` selector.</returns>
		public static Selector operator +(Selector a, Selector b) {
			return new SelectorsCascade(a, SelectorOperator.PrecendantOf, b);
		}
		/// <summary>
		/// `a>b` selector.
		/// </summary>
		/// <param name="a">A selector.</param>
		/// <param name="b">A selector.</param>
		/// <returns>`a>b` selector.</returns>
		public static Selector operator >(Selector a, Selector b) {
			return new SelectorsCascade(a, SelectorOperator.ParentOf, b);
		}
		/// <summary>
		/// `a b` selector.
		/// </summary>
		/// <param name="a">A selector.</param>
		/// <param name="b">A selector.</param>
		/// <returns>`a b` selector.</returns>
		public static Selector operator >=(Selector a, Selector b) {
			return new SelectorsCascade(a, SelectorOperator.AncestorOf, b);
		}
		#endregion

		/// <summary>
		/// Equivalent for <c>new Rule(selector, style)</c>.
		/// </summary>
		/// <param name="selector">A selector.</param>
		/// <param name="style">A style.</param>
		/// <returns>new Rule(selector, style).</returns>
		public static Rule operator <=(Selector selector, Style style) {
			return new Rule(selector, style);
		}

		/// <summary>
		/// Use 'b>a' instead.
		/// </summary>
		[Obsolete("Use 'b>a' instead.", true)]
		public static Selector operator <(Selector a, Selector b) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Use 'b>=a' instead.
		/// </summary>
		[Obsolete("Use 'b>=a' instead.", true)]
		public static Selector operator <=(Selector a, Selector b) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// Use 'b<=a' instead.
		/// </summary>
		[Obsolete("Use 'b<=a' instead.", true)]
		public static Rule operator >=(Selector a, Style b) { throw new InvalidOperationException(JS.CannotRunAtServer); }
		/// <summary>
		/// `*` selector.
		/// </summary>
		public static readonly Selector Universal = new UniversalSelector();
	}
	/// <summary>
	/// CSS rule.
	/// </summary>
	public class Rule {
		/// <summary>
		/// CSS style.
		/// </summary>
		public Style Style;
		/// <summary>
		/// CSS selector.
		/// </summary>
		public readonly Selector Selector;
		/// <summary>
		/// Creates a CSS rule with an empty style.
		/// </summary>
		/// <param name="selector">A selector.</param>
		public Rule(Selector selector) {
			this.Selector = selector;
			this.Style = new Style();
		}
		/// <summary>
		/// Creates a CSS rule.
		/// </summary>
		/// <param name="selector">A selector.</param>
		/// <param name="style">A style.</param>
		public Rule(Selector selector, Style style) {
			this.Selector = selector;
			this.Style = style;
		}
	}


	namespace Selectors {
		internal class UniversalSelector : Selector {
			internal override void AppendTo(StringBuilder sb) {
				sb.Append('*');
			}
		}
		internal class IdSelector : Selector {
			public readonly string Id;
			public IdSelector(string id) {
				this.Id = id;
			}
			internal override void AppendTo(StringBuilder sb) {
				sb.Append('#').Append(this.Id);
			}
		}
		internal class TypeSelector : Selector {
			public readonly Type ElementType;
			public TypeSelector(Type elementType) {
				this.ElementType = elementType;
			}
			internal override void AppendTo(StringBuilder sb) {
				sb.Append(this.ElementType.Module.ResolveString(
					BitConverter.ToInt32(
						this.ElementType.GetConstructor(
							BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
							null,
							System.Type.EmptyTypes,
							null).GetMethodBody().GetILAsByteArray(), 2)
				));
			}
		}
		internal class ClassSelector : Selector {
			public readonly Selector Selector;
			public readonly string ClassName;
			public ClassSelector(Selector selector, string className) {
				this.Selector = selector;
				this.ClassName = className;
			}
			internal override void AppendTo(StringBuilder sb) {
				if (this.Selector != null) this.Selector.AppendTo(sb);
				sb.Append('.').Append(this.ClassName);
			}
		}
		internal class AttributeSelector2 : Selector {
			public readonly Selector Selector;
			public readonly AttributeSelectorType AttributeSelectorType;
			public readonly string AttributeName;
			public readonly string Value;
			public AttributeSelector2(Selector selector, string attributeName, AttributeSelectorType attributeSelectorType, string value) {
				this.Selector = selector;
				this.AttributeName = attributeName;
				this.AttributeSelectorType = attributeSelectorType;
				this.Value = value;
			}
			internal override void AppendTo(StringBuilder sb) {
				this.Selector.AppendTo(sb);
				sb.Append('[').Append(this.AttributeName);
				switch (this.AttributeSelectorType) {
					case AttributeSelectorType.EqualsTo: sb.Append('='); break;
					case AttributeSelectorType.Contains: sb.Append("~="); break;
					case AttributeSelectorType.StartsWith: sb.Append("|="); break;
				}
				sb.Append(MK.JavaScript.Ajax.Serialization.Serializer.Serialize(this.Value)).Append(']');
			}
		}
		internal class AttributeSelector : Selector {
			public readonly Selector Selector;
			public readonly string AttributeName;
			public AttributeSelector(Selector selector, string attributeName) {
				this.Selector = selector;
				this.AttributeName = attributeName;
			}
			internal override void AppendTo(StringBuilder sb) {
				this.Selector.AppendTo(sb);
				sb.Append('[').Append(this.AttributeName).Append(']');
			}
		}
		internal class PseudoClassSelector : Selector {
			private static string Dasherize(string str) {
				return new Regex(@"[A-Z]", RegexOptions.Compiled).Replace(str, m => {
					return "-" + char.ToLower(m.Value[0]);
				});
			}


			public readonly Selector Selector;
			public readonly PseudoClass PseudoClass;
			public PseudoClassSelector(Selector selector, PseudoClass pseudoClass) {
				this.Selector = selector;
				this.PseudoClass = pseudoClass;
			}
			internal override void AppendTo(StringBuilder sb) {
				if (this.Selector != null) this.Selector.AppendTo(sb);
				sb.Append(':').Append(Dasherize(this.PseudoClass.ToString()).Substring(1));
			}
		}
		internal class SelectorsCascade : Selector {
			public readonly IEnumerable<Selector> Selectors;
			public readonly IEnumerable<SelectorOperator> Operators;

			public SelectorsCascade(Selector sel1, SelectorOperator op, Selector sel2) {
				var ss1 = sel1 as SelectorsCascade;
				var ss2 = sel2 as SelectorsCascade;
				if (ss1 == null) {
					if (ss2 == null) {
						this.Selectors = new[] { sel1, sel2 };
						this.Operators = new[] { op };
					} else {
						this.Selectors = (new[] { sel1 }).Concat(ss2.Selectors);
						this.Operators = (new[] { op }).Concat(ss2.Operators);
					}
				} else {
					if (ss2 == null) {
						this.Selectors = ss1.Selectors.Concat(new[] { sel2 });
						this.Operators = ss1.Operators.Concat(new[] { op });
					} else {
						this.Selectors = ss1.Selectors.Concat(ss2.Selectors);
						this.Operators = ss1.Operators.Concat(new[] { op }).Concat(ss2.Operators);
					}
				}
			}

			internal override void AppendTo(StringBuilder sb) {
				using (var selectorsEnumerator = this.Selectors.GetEnumerator()) {
					using (var operatorsEnumerator = this.Operators.GetEnumerator()) {
						selectorsEnumerator.MoveNext();
						selectorsEnumerator.Current.AppendTo(sb);
						selectorsEnumerator.MoveNext();
						do {
							operatorsEnumerator.MoveNext();
							switch (operatorsEnumerator.Current) {
								case SelectorOperator.ParentOf: sb.Append('>'); break;
								case SelectorOperator.AncestorOf: sb.Append(' '); break;
								case SelectorOperator.PrecendantOf: sb.Append('+'); break;
								case SelectorOperator.Alternative: sb.Append(','); break;
							}
							selectorsEnumerator.Current.AppendTo(sb);
						} while (selectorsEnumerator.MoveNext());
					}
				}
			}
		}

		internal enum SelectorOperator {
			ParentOf, AncestorOf, PrecendantOf, Alternative
		}

	}
}