using Godot;

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Galatime;

public static class ElementManager
{
	private static Dictionary<string, GalatimeElement> _elements = new Dictionary<string, GalatimeElement>();

	public static void LoadElements(string filePath)
	{
		var json = File.ReadAllText(filePath);
		var elements = JsonConvert.DeserializeObject<List<GalatimeElement>>(json);
		GD.Print($"Loaded {elements.Count} elements from JSON");

		foreach (var element in elements)
		{
			GD.Print($"Element: {element.Name}, ID: {element.Id}, DamageMultipliers count: {element.DamageMultipliers.Count}");
			_elements[element.Id] = element;
		}
	}

	public static GalatimeElement GetById(string id)
	{
		if (_elements.TryGetValue(id, out var element))
		{
			return element;
		}
		return null;
	}

	// Static properties that load their data from JSON
	public static GalatimeElement Aqua => GetById("aqua");
	public static GalatimeElement Caeli => GetById("caeli");
	public static GalatimeElement Chaos => GetById("chaos");
	public static GalatimeElement Ignis => GetById("ignis");
	public static GalatimeElement Lapis => GetById("lapis");
	public static GalatimeElement Lux => GetById("lux");
	public static GalatimeElement Naturalea => GetById("naturalea");
	public static GalatimeElement Corporis => GetById("corporis");
	public static GalatimeElement Spatium => GetById("spatium");
	public static GalatimeElement Tenebris => GetById("tenebris");
	public static GalatimeElement Magic => GetById("magic");
	public static GalatimeElement Vetus => GetById("vetus");
	public static GalatimeElement Inanus => GetById("inanus");
}
