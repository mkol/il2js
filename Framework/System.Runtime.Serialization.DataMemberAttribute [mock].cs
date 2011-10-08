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
//#define MONO
#if MONO

namespace System.Runtime.Serialization {

	// Summary:
	//     Specifies that the type defines or implements a data contract and is serializable
	//     by a serializer, such as the System.Runtime.Serialization.DataContractSerializer.
	//     To make their type serializable, type authors must define a data contract
	//     for their type.
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
	public sealed class DataContractAttribute : Attribute {
		// Summary:
		//     Initializes a new instance of the System.Runtime.Serialization.DataContractAttribute
		//     class.
		public DataContractAttribute() { }

		// Summary:
		//     Gets or sets a value that indicates whether to preserve object reference
		//     data.
		//
		// Returns:
		//     true to keep object reference data using standard XML; otherwise, false.
		//     The default is false.
		public bool IsReference { get; set; }
		//
		// Summary:
		//     Gets or sets the name of the data contract for the type.
		//
		// Returns:
		//     The local name of a data contract. The default is the name of the class that
		//     the attribute is applied to.
		public string Name { get; set; }
		//
		// Summary:
		//     Gets or sets the namespace for the data contract for the type.
		//
		// Returns:
		//     The namespace of the contract.
		public string Namespace { get; set; }
	}


	// Summary:
	//     When applied to the member of a type, specifies that the member is part of
	//     a data contract and is serializable by the System.Runtime.Serialization.DataContractSerializer.
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class DataMemberAttribute : Attribute {
		// Summary:
		//     Initializes a new instance of the System.Runtime.Serialization.DataMemberAttribute
		//     class.
		public DataMemberAttribute() { }

		// Summary:
		//     Gets or sets a value that specifies whether to serialize the default value
		//     for a field or property being serialized.
		//
		// Returns:
		//     true if the default value for a member should be generated in the serialization
		//     stream; otherwise, false. The default is true.
		public bool EmitDefaultValue { get; set; }
		//
		// Summary:
		//     Gets or sets a value that instructs the serialization engine that the member
		//     must be present when reading or deserializing.
		//
		// Returns:
		//     true, if the member is required; otherwise, false.
		//
		// Exceptions:
		//   System.Runtime.Serialization.SerializationException:
		//     the member is not present.
		public bool IsRequired { get; set; }
		//
		// Summary:
		//     Gets or sets a data member name.
		//
		// Returns:
		//     The name of the data member. The default is the name of the target that the
		//     attribute is applied to.
		public string Name { get; set; }
		//
		// Summary:
		//     Gets or sets the order of serialization and deserialization of a member.
		//
		// Returns:
		//     The numeric order of serialization or deserialization.
		public int Order { get; set; }
	}
}

#endif