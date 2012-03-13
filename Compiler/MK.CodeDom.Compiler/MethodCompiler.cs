/*
il2js Compiler - JavaScript VM for .NET
Copyright (C) 2012 Michael Kolarz

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using MK.JavaScript;
using System.Text.RegularExpressions;
using MK.JavaScript.Framework;
using MK.JavaScript.CodeDom.Compiler;
using MK.JavaScript.Reflection;
using MK.JavaScript.Compiler;
public static class MemberInfoExtensions {
	private static void AppendTypes(Type t, StringBuilder sb) {
		if (t.DeclaringType != null && !t.IsGenericParameter) {
			AppendTypes(t.DeclaringType, sb);
			sb.Append('+');
		}
		sb.Append(t.Name);
		if (t.IsGenericType) {
			sb.Append('[');
			foreach (var gt in t.GetGenericArguments()) {
				AppendTypes(gt, sb);
				sb.Append(',');
			}
			--sb.Length;
			sb.Append(']');
		}
	}
	public static string GetSignature(this MemberInfo member) {
		StringBuilder sb = new StringBuilder();

		var field = member as FieldInfo;
		var methodBase = member as MethodBase;
		var methodInfo = member as MethodInfo;
		var type = member as Type;

		if (field != null) {
			if (field.IsStatic) { sb.Append(".static "); }
			sb.Append(field.FieldType.Name).Append(' ');
		} else if (methodBase != null) {
			if (methodBase.IsStatic) { sb.Append(".static "); }
			if (methodBase.IsVirtual) { sb.Append(".virtual "); }
			if (methodBase.IsAbstract) { sb.Append(".abstract "); }
			if (methodBase.IsFinal) { sb.Append(".final "); }
			if (methodInfo != null) { sb.Append(methodInfo.ReturnType.Name).Append(' '); }
		}

		if (member.DeclaringType != null) {
			AppendTypes(member.DeclaringType, sb);
			sb.Append("::");
		}
		if (type != null)
			AppendTypes(type, sb);
		else
			sb.Append(member.Name);

		if (methodBase != null) {
			sb.Append("(");
			var pos = sb.Length;
			foreach (var p in methodBase.GetParameters()) {
				sb.Append(p.ParameterType.Name).Append(',');
			}
			if (sb.Length != pos)
				--sb.Length;
			sb.Append(')');
		}

		return sb.ToString();
	}
}
namespace MK.CodeDom.Compiler {


	partial class MethodCompiler {
		private ILToken[] ilTokens;
		protected int Position { get; private set; }
		public readonly MethodBaseData CalleeData;
		public readonly MethodBase Method;

		protected class Jump {
			public readonly int WritePosition;
			public readonly int BeginPosition;
			public readonly int InputPosition;

			public Jump(int beginPosition, int inputPosition, int writePosition) {
				this.BeginPosition = beginPosition;
				this.WritePosition = writePosition;
				this.InputPosition = inputPosition;
			}
		}

		private List<Jump> jumps = new List<Jump>();
		protected void AddJump(int offset) {
			this.AddJump(this.Position + offset, this.output.Length);
		}
		protected void AddJump(int targetLabelIndex, int beginPosition) {
			this.jumps.Add(new Jump(beginPosition, targetLabelIndex, this.output.Length));
		}

		protected readonly AssemblyCompiler AssemblyCompiler;
		public MethodCompiler(AssemblyCompiler assemblyCompiler, MethodBaseData callee, MethodBase method) {
			this.AssemblyCompiler = assemblyCompiler;
			this.CalleeData = callee;
			this.Method = method;
		}
		public string Body { get; private set; }
		public void Compile() {
			this._Instructions = new LinkedList<ILInstruction>();

			//bo konstuktory structow nie loaduja parametru w celu zawolania object::.ctor a to mnie boli bo sie nie zwraca
			//a nie da sie tego zrobic przez to ze operator newobj mowi ze to zwraca bo niekiedy konstruktor
			//wola konstruktor natywny (np Element)
			if (this.Method.IsConstructor && this.Method.DeclaringType.IsValueType) {
				this.output.Append(JSOpCode.LdA).Append((0).Encode());
			}

			//		try {
			//typeof(MulticastDelegate).GetConstructors(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance)

			var body = this.Method.GetMethodBody();




			this.protectedRegions = new Dictionary<int, Stack<ProtectedRegion>>();
			this.ignoredExceptionHandlingClauses = new Dictionary<int, int>();
			foreach (var clause in body.ExceptionHandlingClauses) {
				switch (clause.Flags) {
					case ExceptionHandlingClauseOptions.Clause:
						if (Settings.IgnoreClause)
							goto default;
						else {
							Stack<ProtectedRegion> tryBlocks;
							if (this.protectedRegions.TryGetValue(clause.TryOffset, out tryBlocks)) {
								if (tryBlocks.Peek().Length != clause.TryLength) {
									tryBlocks.Push(new ProtectedRegion(ProtectedRegionType.Try, clause.TryLength));
								} else {
									Program.XmlLog.WriteElementString("warning",
										string.Format(
											"Ingored {0} exception handling clause for Try@{1}(+{2}) since it is not first clause for that try.",
											clause.Flags,
											clause.TryOffset,
											clause.TryLength
									));
									break;
								}
							} else {
								this.protectedRegions[clause.TryOffset] = new Stack<ProtectedRegion>(new[]{
									new ProtectedRegion(ProtectedRegionType.Try,clause.TryLength)
								});
							}
							this.protectedRegions[clause.HandlerOffset] = new Stack<ProtectedRegion>(new[]{
								new ProtectedRegion(clause.CatchType,clause.HandlerLength)
							});
						} break;
					case ExceptionHandlingClauseOptions.Finally:
						if (Settings.IgnoreFinally)
							goto default;
						else {
							Stack<ProtectedRegion> tryBlocks;
							if (this.protectedRegions.TryGetValue(clause.TryOffset, out tryBlocks)) {
								if (tryBlocks.Peek().Length != clause.TryLength) {
									tryBlocks.Push(new ProtectedRegion(ProtectedRegionType.Try, clause.TryLength));
								} else {
									Program.XmlLog.WriteElementString("warning",
										string.Format(
											"Ingored {0} exception handling clause for Try@{1}(+{2}) since it is not first clause for that try.",
											clause.Flags,
											clause.TryOffset,
											clause.TryLength
									));
									break;
								}
							} else {
								this.protectedRegions[clause.TryOffset] = new Stack<ProtectedRegion>(new[]{
									new ProtectedRegion(ProtectedRegionType.Try,clause.TryLength)
								});
							}
							this.protectedRegions[clause.HandlerOffset] = new Stack<ProtectedRegion>(new[]{
								new ProtectedRegion(ProtectedRegionType.Finally,clause.HandlerLength)
							});
						} break;
					case ExceptionHandlingClauseOptions.Fault:
					case ExceptionHandlingClauseOptions.Filter:
					default:
						Program.XmlLog.WriteElementString("warning", string.Format("Ingored {0} exception handling clause.", clause.Flags));
						this.ignoredExceptionHandlingClauses[clause.HandlerOffset] = clause.HandlerLength;
						break;
				}
			}


			this.Position = 0;
			this.ilTokens = new ILParser().GetTokens(Method);
			this.PositionMappings = new int[ilTokens.Length];


			#region Css.Import
			var attrs = this.Method.GetCustomAttributes(typeof(MK.JavaScript.Css.ImportAttribute), false);
			if (attrs.Length > 0) {
				CssSupport.Register(this.Method, attrs);
			}
			#endregion
			#region LoadResources
			StringBuilder sb = new StringBuilder();
			foreach (LoadResourcesAttribute attr in this.Method.GetCustomAttributes(typeof(LoadResourcesAttribute), false)) {
				foreach (var type in attr.ResourceTypes) {
					if (Resources.IsResourceClass(type)) {
						sb.Append(Resources.Resolve(type)).Append(',');
					} else {
						this.Throw("Incorrect resource type {0} in LoadResourcesAttribute.", type.GetSignature());
					}
				}
			}
			if (sb.Length > 0) {
				--sb.Length;
				this.Ldstr(this.AssemblyCompiler.Register(this.CalleeData, sb.ToString()));
				this.OnCall(Resources.LoadResources);
			}
			#endregion

			this.CompileInstructions();

			Debug.Assert(this.ilTokens.All(t => t == null || t.IsEaten), "Not all tokens eaten in " + Method.GetSignature() + "\n==\n" + string.Join("\n", (from t in this.ilTokens where t != null && !t.IsEaten select t.ToString()).ToArray()));

			this.Instructions_Compile();

			this.postCompileLabels();

			this.OnCompiled();

		}

		JumpData[] jumpData;

		private void postCompileLabels() {
			this.jumpData = this.jumps.Select(j => new JumpData(j.BeginPosition, this.PositionMappings[j.InputPosition], j.WritePosition)).ToArray();
			JumpsInliner.Compute(this.jumpData);

			int last = 0;
			StringBuilder sb = new StringBuilder();
			foreach (var jump in this.jumpData) {
				sb.Append(this.output.ToString(last, jump.WritePosition - last))
					.Append(jump.ToWrite.Encode());
				last = jump.WritePosition;
			}
			sb.Append(this.output.ToString(last, this.output.Length - last));
			this.Body = sb.ToString();
		}
		protected virtual void OnCompiled() { }
		#region Instructions
		protected LinkedList<ILInstruction> _Instructions;
		protected LinkedListNode<ILInstruction> Instructions_Add(ILInstructionType type, params string[] args) {
			return this._Instructions.AddLast(new ILInstruction(this.Position, type, args));
		}
#if STACKCONTROL
		public void Instructions_PackStack() {

			var dict = new Dictionary<string, LinkedListNode<ILInstruction>>();
			var currentNode = this._Instructions.First;
			while (currentNode.Next != null) {
				if (currentNode.Value.Type == ILInstructionType.StoreStack) {
					LinkedListNode<ILInstruction> lastAssignmenNode;
					if (dict.TryGetValue(currentNode.Value.Arguments[0], out lastAssignmenNode)) {

						Instructions_PackStack_Replace(lastAssignmenNode, currentNode);
						dict.Remove(lastAssignmenNode.Value.Arguments[0]);

					}
					////i wstawiamy to
					//if (currentNode.Value.Arguments.Length == 2 &&
					//  (
					//    Regex.IsMatch(currentNode.Value.Arguments[1], @"^\$[^\$]+\$[^\$]+\$$")
					//  )
					//) {
					dict[currentNode.Value.Arguments[0]] = currentNode;
					//}

				}
				currentNode = currentNode.Next;
			}

			// na koniec obcinamy to co bylo nieuzywane dalej
			foreach (var lastAssignmenNode in dict.Values) {
				this.Instructions_PackStack_Replace(lastAssignmenNode, null);
			}
		}
		private void Instructions_PackStack_Replace(LinkedListNode<ILInstruction> lastAssignmenNode, LinkedListNode<ILInstruction> currentNode) {
			var toReplaceNode = lastAssignmenNode.Next;
			do {
				if (toReplaceNode == null) break;
				for (int i = 0; i < toReplaceNode.Value.Arguments.Length; ++i) {
					toReplaceNode.Value.Arguments[i] =
						toReplaceNode.Value.Arguments[i].Replace(lastAssignmenNode.Value.Arguments[0], lastAssignmenNode.Value.Arguments[1]);
				}
				toReplaceNode = toReplaceNode.Next;
				//do-while bo moze byc $stack$0$=$stack$0$.$member$0$
			} while (toReplaceNode != currentNode);
			if (toReplaceNode != null) {
				//JEDEN
				for (int i = 1; i < toReplaceNode.Value.Arguments.Length; ++i) {
					toReplaceNode.Value.Arguments[i] =
						toReplaceNode.Value.Arguments[i].Replace(lastAssignmenNode.Value.Arguments[0], lastAssignmenNode.Value.Arguments[1]);
				}
			}
			this._Instructions.Remove(lastAssignmenNode);
		}
#endif
		private void Instructions_Compile() {


			//var enumerator = this.labels.GetEnumerator();
			//var wasLabel = enumerator.MoveNext();
			//return;
			var node = this._Instructions.First;
			while (node != null) {
				//if (wasLabel) {
				//  if (node.Value.NextInstructionPosition == enumerator.Current.Key) {
				//    this._Instructions.AddAfter(node, new ILInstruction(enumerator.Current.Key, ILInstructionType.Label, enumerator.Current.Key.ToString()));
				//    wasLabel = enumerator.MoveNext();
				//  }
				//}
				var next = node.Next;
				switch (node.Value.Type) {
					case ILInstructionType.None:
						this._Instructions.Remove(node);
						break;
					case ILInstructionType.EndCase:
						if (node.Previous.Value.Type == ILInstructionType.BeginCase) {
							this._Instructions.Remove(node.Previous);
							this._Instructions.Remove(node);
						}
						break;
					case ILInstructionType.EndElse:
						if (node.Previous.Value.Type == ILInstructionType.BeginElse) {
							this._Instructions.Remove(node.Previous);
							this._Instructions.Remove(node);
						}
						break;
				}
				node = next;
			}
			//Debug.Assert(!wasLabel);
		}
		//    public string Instructions_ToString() {
		//      int indent = 0;
		//      return string.Join("\n", this._Instructions.Select(i => {
		//        if (i.Type == ILInstructionType.StoreLocal) {
		//          return new string('\t', indent) + this.Set(this.GetLocalName(int.Parse(i.Arguments[0])), i.Arguments[1]) + ";";
		//        } else if (i.Type == ILInstructionType.LoadLocal) {
		//          return new string('\t', indent) + this.Push(this.GetLocalName(int.Parse(i.Arguments[0]))) + ";";
		//        } else if (i.IsStoreInstruction) {
		//          return new string('\t', indent) + this.Set(i.Arguments[0], i.Arguments[1]) + ";";
		//        } else if (i.Type == ILInstructionType.Return) {
		//          return new string('\t', indent) + this.Return(i.Arguments[0]) + ";";
		//        } else if (i.Type == ILInstructionType.Statement) {
		//          return new string('\t', indent) + i.Arguments[0] + ";";
		//          #region stack
		//        } else if (i.Type == ILInstructionType.Push) {
		//          return new string('\t', indent) + this.Push(i.Arguments[0]);
		//          #endregion
		//          #region branches
		//        } else if (i.Type == ILInstructionType.Branch) {
		//          return this.Branch(this.labels[int.Parse(i.Arguments[0])].Name);
		//          //return this.Branch(i.Arguments[0]);
		//        } else if (i.Type == ILInstructionType.ConditionalBranch) {
		//          return this.Branch(this.labels[int.Parse(i.Arguments[0])].Name, i.Arguments[1]);
		//          //return this.Branch(i.Arguments[0], i.Arguments[1]);
		//        } else if (i.Type == ILInstructionType.Label) {
		//          return this.CreateLabel(this.labels[int.Parse(i.Arguments[0])].Name);
		//          //return this.CreateLabel(i.Arguments[0]);
		//          #endregion
		//          #region BLOCKS
		//        } else if (i.Type == ILInstructionType.BeginIf) {
		//          return new string('\t', indent++) + this.BeginIf(i.Arguments[0]);
		//        } else if (i.Type == ILInstructionType.BeginElse) {
		//          return new string('\t', indent++) + this.BeginElse();
		//        } else if (i.Type == ILInstructionType.EndDoWhile) {
		//          return new string('\t', --indent) + this.EndDoWhile(i.Arguments[0]);
		//        } else if (i.Type == ILInstructionType.BeginDoWhile) {
		//          return new string('\t', indent++) + this.BeginDoWhile();
		//        } else if (i.Type == ILInstructionType.BeginWhile) {
		//          return new string('\t', indent++) + this.BeginWhile(i.Arguments[0]);
		//        } else if (i.Type == ILInstructionType.BeginSwitch) {
		//          return new string('\t', indent++) + this.BeginSwitch(i.Arguments[0]);
		//        } else if (i.Type == ILInstructionType.BeginCase) {
		//          return new string('\t', indent++) + this.BeginCase(i.Arguments);
		//        } else if (i.Type == ILInstructionType.EndCase) {
		//          return new string('\t', --indent) + this.EndCase();
		//        } else if ((i.Type & ILInstructionType._EndBlock) == ILInstructionType._EndBlock) {
		//          return new string('\t', --indent) + this.EndBlock();
		//          #endregion
		//        } else {
		//          return new string('\t', indent) + "??????????????????????????????????????" + i.Type;
		//        }

		//      }).ToArray())
		//#if DEBUG
		// + "\n"
		//#endif
		//;
		//    }
		#endregion

		/// <summary>
		/// InputPosition=>OutputPosition
		/// </summary>
		protected int[] PositionMappings;

		private delegate bool Predicate();

		private Dictionary<int, int> ignoredExceptionHandlingClauses;

		private enum ProtectedRegionType {
			Try,
			Catch,
			Finally,
		}
		private class ProtectedRegion {
			public readonly ProtectedRegionType Type;
			public readonly int Length;
			public readonly Type CatchType;
			public ProtectedRegion(ProtectedRegionType type, int length) {
				this.Type = type;
				this.Length = length;
			}
			public ProtectedRegion(Type catchType, int length)
				: this(ProtectedRegionType.Catch, length) {
				this.CatchType = catchType;
			}
		}

		private Dictionary<int, Stack<ProtectedRegion>> protectedRegions;

		private void CompileInstructions() {
			while (this.Position < this.ilTokens.Length) {
				#region Skipping exception handling clauses
				int length;
				if (this.ignoredExceptionHandlingClauses.TryGetValue(this.Position, out length)) {
					this.Position += length;
					continue;
				}
				#endregion
				this.PositionMappings[this.Position] = this.output.Length;
				#region protected blocks
				Stack<ProtectedRegion> regions;
				if (this.protectedRegions.TryGetValue(this.Position, out regions)) {
					foreach (var region in regions) {
						switch (region.Type) {
							case ProtectedRegionType.Try:
								this.BeginTry(region.Length);
								break;
							case ProtectedRegionType.Catch:
#warning nie sprawdznaie typu wyjatku
								this.BeginCath(region.Length, null);
								break;
							case ProtectedRegionType.Finally:
								//nie jest mi potrzebna dlugosc bo finally jest ostatnim handlerem, i opuszczczany jest przez endfinally
								this.BeginFinally();
								break;
						}
					}
				}
				/*
 *						begin try
 *						-> end try
 *						[try code]
 *						leave
 *						-> end
 * end try:		begin catch
 *						exception id
 *						-> end catch
 *						[catch code]
 *						leave
 *						-> end
 * end catch:	begin catch
 *						exception id
 *						-> end catch
 *						[catch code]
 *						leave
 *						-> end
 * end:
 * 
 */
				#endregion


				var eaten = this.EatToken();
				Debug.Assert(eaten.Value is OpCode, "OpCode not found but " + eaten.Value.GetType().Name + ": " + eaten.Value.ToString() + " @" + this.Position);
				this.HandleOpCode((OpCode)eaten.Value);
			}
		}
		public ILToken EatToken() {

			var ret = this.ilTokens[this.Position];
			this.ilTokens[this.Position].IsEaten = true;
			//to jest prawdziwe gdy wrocilismy do czegos co bylo wczesniej zjedzone innym przeplywem
			if (ret != null)
				this.Position += ret.Size;
			return ret;
		}

#if DEBUG
		public string Parse(MethodBase method) {
			var sb = new StringBuilder();
			if (method.GetMethodBody() != null) {
				var parser = new ILParser();
				foreach (var item in parser.GetTokens(method).Where(x => x != null)) {
					sb.Append(item).Append('\n');
				}
			}
			return sb.ToString();
		}
#endif
	}

}
