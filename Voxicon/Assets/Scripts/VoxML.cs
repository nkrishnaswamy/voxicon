using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

/// <summary>
/// ENTITY
/// </summary>
public class Entity {
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
public class Lex {
	public string Pred = "";
	public string Type = "";
}

/// <summary>
/// TYPE
/// </summary>
public class Component {
	[XmlAttribute]
	public string Value { get; set; }
}

public class Arg {
	[XmlAttribute]
	public string Value { get; set; }
}

public class Subevent {
	[XmlAttribute]
	public string Value { get; set; }
}

public class Type {
	public string Head = "";
	
	[XmlArray("Components")]
	[XmlArrayItem("Component")]
	public List<Component> Components = new List<Component>();
	
	public string Concavity = "";
	public string RotatSym = "";
	public string ReflSym = "";

	[XmlArray("Args")]
	[XmlArrayItem("Arg")]
	public List<Arg> Args = new List<Arg>();

	[XmlArray("Body")]
	[XmlArrayItem("Subevent")]
	public List<Subevent> Body = new List<Subevent>();

	public string Class = "";
	public string Value = "";
	public string Constr = "";
}

/// <summary>
/// HABITAT
/// </summary>
public class Intr {
	[XmlAttribute]
	public string Name { get; set; }
	
	[XmlAttribute]
	public string Value { get; set; }
}

public class Extr {
	[XmlAttribute]
	public string Name { get; set; }
	
	[XmlAttribute]
	public string Value { get; set; }
}

public class Habitat {
	[XmlArray("Intrinsic")]
	[XmlArrayItem("Intr")]
	public List<Intr> Intrinsic = new List<Intr>();
	
	[XmlArray("Extrinsic")]
	[XmlArrayItem("Extr")]
	public List<Extr> Extrinsic = new List<Extr>();
}

/// <summary>
/// AFFORD_STR
/// </summary>
public class Affordance {
	[XmlAttribute]
	public string Formula { get; set; }
}

public class Afford_Str {
	[XmlArray("Affordances")]
	[XmlArrayItem("Affordance")]
	public List<Affordance> Affordances = new List<Affordance>();
}

/// <summary>
/// EMBODIMENT
/// </summary>
public class Embodiment {
	
	public string Scale = "";
	public bool Movable = false;
}

/// <summary>
///  VOXEME
/// </summary>
public class VoxML {

	public Entity Entity = new Entity ();
	public Lex Lex = new Lex();
	public Type Type = new Type();
	public Habitat Habitat = new Habitat();
	public Afford_Str Afford_Str = new Afford_Str();
	public Embodiment Embodiment = new Embodiment();
	
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

