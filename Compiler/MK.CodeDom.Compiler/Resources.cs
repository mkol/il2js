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
using System.Reflection;
using MK.CodeDom.Compiler;
using System.CodeDom.Compiler;
using System.Resources;
using System.Globalization;
using System.Threading;
using MK.JavaScript.Ajax.Serialization;
using System.IO;
using MK.JavaScript.Reflection;
using System.Xml.Linq;

namespace MK.JavaScript.CodeDom.Compiler {
	internal static class _Resources {
		[JSFramework(Code = @"
function(a,b){
	if(window.$resources$===undefined)
		$resources$={};
	if(!(a in $resources$))
		$resources$[a]=eval(new Ajax.Request($path$+
//#if guid
			$guid$+'.aspx?'
//#else
			'$resourcesFileName$.aspx?'
//#endif
			+a,{asynchronous:false}).transport.responseText);
	return $resources$[a][b];
}
")]
		private static object _LoadResource(int resourcesIndex, int memberIndex) { throw new InvalidOperationException(); }
		[JSFramework(Code = @"
function(a){
	if(window.$resources$===undefined)
		$resources$={};
	eval(new Ajax.Request($path$+
//#if guid
		$guid$+'.aspx?'
//#else
		'$resourcesFileName$.aspx?'
//#endif
		+(a=a.split(',').filter(function(b){return !(b in $resources$)})).join(','),{asynchronous:false}).transport.responseText).each(function(b,c){
		$resources$[a[c]]=b;
	});
}
")]
		private static void _LoadResources(string resources) { throw new InvalidOperationException(); }
	}
	public class Resources {

		internal static MethodInfo LoadResource = typeof(_Resources).GetMethod("_LoadResource", BindingFlags.Static | BindingFlags.NonPublic);
		internal static MethodInfo LoadResources = typeof(_Resources).GetMethod("_LoadResources", BindingFlags.Static | BindingFlags.NonPublic);

		private class ResourcesManagerData {
			public readonly int Index;
			public ResourcesManagerData(int index) {
				this.Index = index;
			}
			public Dictionary<MethodBase, int> Members = new Dictionary<MethodBase, int>();
		}
		private static readonly Dictionary<Type, ResourcesManagerData> resources = new Dictionary<Type, ResourcesManagerData>();

		public static int Resolve(Type type) {
			return GetTypeData(type).Index;
		}
		public static void Resolve(MethodBase method, out int resourcesIndex, out int memberIndex) {
			ResourcesManagerData data = GetTypeData(method.DeclaringType);
			resourcesIndex = data.Index;
			if (!data.Members.TryGetValue(method, out memberIndex)) {
				memberIndex = data.Members.Count;
				data.Members[method] = data.Members.Count;
			}
		}

		private static ResourcesManagerData GetTypeData(Type type) {
			ResourcesManagerData data;
			if (!resources.TryGetValue(type, out data)) {
				data = new ResourcesManagerData(resources.Count);
				resources[type] = data;
			}
			return data;
		}


		public static bool IsResourceClass(Type type) {
			var attribute = type.GetCustomAttribute<GeneratedCodeAttribute>();
			return attribute != null && attribute.Tool == "System.Resources.Tools.StronglyTypedResourceBuilder";
		}

		public static XElement CreateConfigurationElement(string fileName, IEnumerable<CultureInfo> cultures) {
			if (resources.Count == 0) 
				return null;

			var resourceElement = new XElement("resource",
				new XAttribute("name", fileName),
				new XAttribute("cultures", string.Join(",", cultures.Select(c => c.Name).ToArray()))
			);
			foreach (var kv in resources) {
				resourceElement.Add(new XElement("type",
					new XAttribute("name", kv.Key.FullName),
					new XAttribute("assembly", kv.Key.Assembly.GetName().Name),
					new XAttribute("members", string.Join(",", (from m in kv.Value.Members
																											orderby m.Value
																											select m.Key.Name.Substring(4)).ToArray()))
				));
			}

			return resourceElement;
		}
	}
}
