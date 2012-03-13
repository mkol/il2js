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

namespace MK.CodeDom.Compiler {
	public class JumpData {
		public readonly int WritePosition;
		public readonly int BeginPosition;
		public readonly int EndPosition;
		public readonly int ToWriteOriginal;
		public int ToWrite;
		public int SizeOfLabel {
			get {
				if (ToWrite == 0) return 1;
				if (ToWrite > 0) {
					var tmp = ToWrite;
					var value = 1;
					while ((tmp >>= 4) != 0) ++value;
					return value;
				} else {
					var tmp = -ToWrite;
					var value = 2;//bo -
					while ((tmp >>= 4) != 0) ++value;
					return value;
				}
			}
		}
		public JumpData(int beginPosition, int endPosition, int writePosition) {
			this.WritePosition = writePosition;
			this.BeginPosition = beginPosition;
			this.EndPosition = endPosition;
			this.ToWrite = endPosition - beginPosition;
			this.ToWriteOriginal = endPosition - beginPosition;
		}
	}
	public class JumpsInliner {
		//hm... dziwne ze wyszedl ten sam wzor na left i right...
		//ale dziala ;) 
		//to ze szuka sie left<=x<right to pewnie to powoduje ten sam wzor. i dobrze...

		public static int Left(int[] ps, int p) {
			int index = Array.BinarySearch(ps, p);
			if (index >= 0) {
				return index + 1;
			} else {
				return ~index;
			}
		}
		public static int Right(int[] ps, int p) {
			int index = Array.BinarySearch(ps, p);
			if (index >= 0) {
				return index + 1;
			} else {
				return ~index;
			}
		}
		public static void Compute(JumpData[] jumps) {
			var writePositions = jumps.Select(j => j.WritePosition).ToArray();
			bool changed;
			do {
				changed = false;

				foreach (var jump in jumps) {
					int left, right, toWrite = jump.ToWriteOriginal;
					if (toWrite >= 0) {
						left = Left(writePositions, jump.BeginPosition);
						right = Right(writePositions, jump.EndPosition);
						for (int i = left; i < right; ++i) {
							toWrite += jumps[i].SizeOfLabel;
						}
					} else {
						left = Right(writePositions, jump.EndPosition);
						right = Left(writePositions, jump.BeginPosition);
						for (int i = left; i < right; ++i) {
							toWrite -= jumps[i].SizeOfLabel;
						}
					}
					if (jump.ToWrite != toWrite) {
						jump.ToWrite = toWrite;
						changed = true;
					}
				}

			} while (changed);
		}

	}
}
