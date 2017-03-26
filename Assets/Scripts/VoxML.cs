﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Vox {
	/// <summary>
	/// ENTITY
	/// </summary>
	public class VoxEntity {
		public enum EntityType
		{
			None,
			Object,
			Program,
			Attribute,
			Relation,
			Function
		}
		[XmlAttribute]
		public EntityType Type { get; set; }
	}

	/// <summary>
	/// LEX
	/// </summary>
	public class VoxLex {
		public string Pred = "";
		public string Type = "";
	}

	/// <summary>
	/// TYPE
	/// </summary>
	public class VoxTypeComponent {
		[XmlAttribute]
		public string Value { get; set; }
	}

	public class VoxTypeArg {
		[XmlAttribute]
		public string Value { get; set; }
	}

	public class VoxTypeSubevent {
		[XmlAttribute]
		public string Value { get; set; }
	}

	public class VoxType {
		public string Head = "";
		
		[XmlArray("Components")]
		[XmlArrayItem("Component")]
		public List<VoxTypeComponent> Components = new List<VoxTypeComponent>();
		
		public string Concavity = "";
		public string RotatSym = "";
		public string ReflSym = "";

		[XmlArray("Args")]
		[XmlArrayItem("Arg")]
		public List<VoxTypeArg> Args = new List<VoxTypeArg>();

		[XmlArray("Body")]
		[XmlArrayItem("Subevent")]
		public List<VoxTypeSubevent> Body = new List<VoxTypeSubevent>();

		public string Class = "";
		public string Value = "";
		public string Constr = "";
	}

	/// <summary>
	/// HABITAT
	/// </summary>
	public class VoxHabitatIntr {
		[XmlAttribute]
		public string Name { get; set; }
		
		[XmlAttribute]
		public string Value { get; set; }
	}

	public class VoxHabitatExtr {
		[XmlAttribute]
		public string Name { get; set; }
		
		[XmlAttribute]
		public string Value { get; set; }
	}

	public class VoxHabitat {
		[XmlArray("Intrinsic")]
		[XmlArrayItem("Intr")]
		public List<VoxHabitatIntr> Intrinsic = new List<VoxHabitatIntr>();

		[XmlArray("Extrinsic")]
		[XmlArrayItem("Extr")]
		public List<VoxHabitatExtr> Extrinsic = new List<VoxHabitatExtr>();
	}

	/// <summary>
	/// AFFORD_STR
	/// </summary>
	public class VoxAffordAffordance {
		[XmlAttribute]
		public string Formula { get; set; }
	}

	public class VoxAfford_Str {
		[XmlArray("Affordances")]
		[XmlArrayItem("Affordance")]
		public List<VoxAffordAffordance> Affordances = new List<VoxAffordAffordance>();
	}

	/// <summary>
	/// EMBODIMENT
	/// </summary>
	public class VoxEmbodiment {
		public string Scale = "";
		public bool Movable = false;
		//public int Density = 0;
	}

	/// <summary>
	///  VOXEME
	/// </summary>
	public class VoxML {

		public VoxEntity Entity = new VoxEntity ();
		public VoxLex Lex = new VoxLex();
		public VoxType Type = new VoxType();
		public VoxHabitat Habitat = new VoxHabitat();
		public VoxAfford_Str Afford_Str = new VoxAfford_Str();
		public VoxEmbodiment Embodiment = new VoxEmbodiment();

		public void Save(string path)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(VoxML));
			using(var stream = new FileStream(path, FileMode.Create))
			{
				serializer.Serialize(stream, this);
			}
		}
		
		public static VoxML Load(string path)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(VoxML));
			using(var stream = new FileStream(path, FileMode.Open))
			{
				return serializer.Deserialize(stream) as VoxML;
			}
		}
		
		//Loads the xml directly from the given string. Useful in combination with www.text.
		public static VoxML LoadFromText(string text) 
		{
			XmlSerializer serializer = new XmlSerializer(typeof(VoxML));
			return serializer.Deserialize(new StringReader(text)) as VoxML;
		}	
	}
}
