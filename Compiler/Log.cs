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
using MK.JavaScript.CodeDom.Compiler;

namespace MK.JavaScript {

}


namespace MK.CodeDom.Compiler {

	partial class MethodCompiler {


		internal void DumpLogs() {
			if (!Settings.DebugCompilation) return;

			Program.XmlLog.WriteStartElement("mapping");

			int nextJump = 0;
			int cumulativeOffset = 0;

			for (int i = 1; i < this.PositionMappings.Length; ++i) {
				if (this.PositionMappings[i] == 0) {
					this.PositionMappings[i] = this.PositionMappings[i - 1];
				} else {
					if (nextJump < this.jumpData.Length && this.jumpData[nextJump].WritePosition >= this.PositionMappings[i]) {
						cumulativeOffset += this.jumpData[nextJump].SizeOfLabel;
						++nextJump;
					}
					this.PositionMappings[i] += cumulativeOffset;
				}
			}



			Program.XmlLog.WriteStartAttribute("map");

			foreach (var position in this.PositionMappings) {
				Program.XmlLog.WriteValue(position);
				Program.XmlLog.WriteString("|");
			}

			Program.XmlLog.WriteEndAttribute();


			//foreach (var item in this.il2js_2_ilMapping) {
			//  Program.XmlLog.WriteStartElement("map");
			//  Program.XmlLog.WriteAttributeString("il2js", item.Key.ToString());
			//  Program.XmlLog.WriteAttributeString("il", item.Value.ToString());
			//  Program.XmlLog.WriteEndElement();
			//}

			Program.XmlLog.WriteEndElement();
		}
	}


	partial class AssemblyCompiler {

		private void dumpLogs() {
			if (!Settings.DebugCompilation) return;

			if (this.staticFields.Count > 0) {
				Program.XmlLog.WriteStartElement("statics");
				foreach (var sf in this.staticFields.Values.OrderBy(sf => sf.Index)) {
					Program.XmlLog.WriteStartElement("field");

					Program.XmlLog.WriteAttributeString("name", sf.Field.DeclaringType.FullName + "." + sf.Field.Name);
					Program.XmlLog.WriteAttributeString("metadataToken", "0x" + sf.Field.MetadataToken.ToString("X"));
					Program.XmlLog.WriteAttributeString("index", sf.Index.ToString());
					Program.XmlLog.WriteEndElement();//field
				}
				Program.XmlLog.WriteEndElement();//statics
			}



			//problem z meta

			
			var types = from t in this.typeData.Values
									where !NativesManager.Instance.IsNative(t.Type)
									&& t.Type.IsClass
									orderby t.Index
									select t;
			if (types.Any()) {

				Program.XmlLog.WriteStartElement("types");

				foreach (var type in types) {
					TypeMeta meta;

					if (TypeMeta.metas.TryGetValue(type.Type, out meta)) {
						if (meta.Fields.Count > 0) {
							Program.XmlLog.WriteStartElement("type");
							Program.XmlLog.WriteAttributeString("name", type.Type.FullName);
							Program.XmlLog.WriteAttributeString("metadataToken", "0x" + type.Type.MetadataToken.ToString("X"));
							foreach (var field in meta.Fields.Values) {
								Program.XmlLog.WriteStartElement("member");
								Program.XmlLog.WriteAttributeString("name", field.FieldInfo.Name);
								Program.XmlLog.WriteAttributeString("metadataToken", "0x" + field.FieldInfo.MetadataToken.ToString("X"));
								Program.XmlLog.WriteAttributeString("index", field.Token);
								Program.XmlLog.WriteEndElement();//element
							}
							Program.XmlLog.WriteEndElement();//type
						}
					}
				}

				Program.XmlLog.WriteEndElement();//types

			}

		}
	}
}