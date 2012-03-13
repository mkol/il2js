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
using MK.CodeDom.Compiler;

namespace MK.Collections {
	public class Graph<Vertice> {
		private class VerticeData {
			public readonly int Index;
			public readonly Vertice Vertice;
			public bool Visited;
			public readonly HashSet<VerticeData> Neighbours = new HashSet<VerticeData>();
			public VerticeData(int index, Vertice vertice) {
				this.Index = index;
				this.Vertice = vertice;
			}
		}

		private readonly Dictionary<Vertice, VerticeData> vertices = new Dictionary<Vertice, VerticeData>();
		private readonly Dictionary<Vertice, VerticeData> sources = new Dictionary<Vertice, VerticeData>();
		private VerticeData doAdd(Vertice v) {
			VerticeData data;
			if (!this.vertices.TryGetValue(v, out data)) {
				data = new VerticeData(this.vertices.Count, v);
				this.vertices[v] = data;
			}
			return data;
		}

		public void AddVertice(Vertice v) {
			this.sources[v] = this.doAdd(v);
		}
		public void AddEdge(Vertice parent, Vertice v) { this.doAdd(parent).Neighbours.Add(this.doAdd(v)); }

		public IEnumerable<Vertice> TopologicalInverseOrder(IEnumerable<Vertice> vertices) {
			HashSet<VerticeData> hashSet = new HashSet<VerticeData>();
			foreach (var v in vertices) {
				hashSet.Add(this.vertices[v]);
			}
			while (hashSet.Count > 0) {
				bool found = false;
				foreach (var self in hashSet) {
					bool dependent = false;
					foreach (var other in hashSet) {
						if (other != self && this.IsPathFromTo(self.Vertice, other.Vertice)) {
							dependent = true;
							break;
						}
					}
					if (!dependent) {
						yield return self.Vertice;
						hashSet.Remove(self);
						found = true;
						break;
					}
				}
				if (!found) ThrowHelper.Throw("Cyclic calls in static constructors.");
			}
		}

		public bool IsPathFromTo(Vertice v1, Vertice v2) {
			//return true;
			return this.GetComponentOf(v1).Contains(v2);
			//return this.adj[this.vertices[v1].Index, this.vertices[v2].Index];
		}

		#region DFS
		private HashSet<Vertice> GetComponentOf(Vertice v) {
			HashSet<Vertice> value;
			if (!this.componentsOfSource.TryGetValue(v, out value)) {
				value=DFSFromSource(this.vertices[v]);
			}
			return value;
		}
		private Dictionary<Vertice, HashSet<Vertice>> componentsOfSource = new Dictionary<Vertice, HashSet<Vertice>>();
		private static void DFS(HashSet<Vertice> component, VerticeData current) {
			component.Add(current.Vertice);
			current.Visited = true;
			foreach (var neighbour in current.Neighbours.Where(n => !n.Visited)) {
				DFS(component, neighbour);
			}
		}
		//public void DFSFromSources() { this.DFSFromSources(this.sources.Values); }
		private void DFSFromSources(IEnumerable<VerticeData> vs) {
			foreach (var source in vs) {
				this.DFSFromSource(source);
			}
		}
		private HashSet<Vertice> DFSFromSource(VerticeData source) {
			foreach (var item in this.vertices.Values) {
				item.Visited = false;
			}
			var component = new HashSet<Vertice>();
			this.componentsOfSource[source.Vertice] = component;
			DFS(component, source);
			return component;
		}
		#endregion

		internal string Dump() {
			StringBuilder sb = new StringBuilder();
			foreach (var item in this.vertices.Values) {
				foreach (var v2 in item.Neighbours) {
					sb.Append(item.Vertice.ToString()+" -> "+v2.Vertice.ToString()+"\n");
				}
			}
			return sb.ToString();
		}

		//private bool[,] adj;

		//public void FloydWarshall() {
		//  this.adj = new bool[this.vertices.Count, this.vertices.Count];
		//  foreach (var data in this.vertices.Values) {
		//    this.adj[data.Index, data.Index] = true;
		//    foreach (var neighbour in data.Neighbours) {
		//      this.adj[data.Index, neighbour] = true;
		//    }
		//  }
		//  for (int k = 0; k < this.vertices.Count; ++k) {
		//    for (int i = 0; i < this.vertices.Count; ++i) {
		//      for (int j = 0; j < this.vertices.Count; ++j) {
		//        this.adj[i, j] = this.adj[i, j] || (this.adj[i, k] && this.adj[k, j]);
		//      }
		//    }
		//  }
		//}

	}
}
