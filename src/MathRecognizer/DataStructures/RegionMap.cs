using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DataStructures
{
	public class RegionMap
	{
		Dictionary<string, string[][]> class_to_regions;
		Dictionary<string, string> symbol_to_class;
		HashSet<string> region_set;

		public RegionMap()
		{
			class_to_regions = new Dictionary<string, string[][]>();
			symbol_to_class = new Dictionary<string, string>();
			region_set = new HashSet<string>();
		}

		public static RegionMap Parse(string raw_data)
		{
			RegionMap result = new RegionMap();

			// first find the Region line
			StringReader sr = new StringReader(raw_data);
			for (string line = sr.ReadLine(); line != null; line = sr.ReadLine())
			{
				// commnet
				if (line.IndexOf("##") == 0) continue;
				// empty line
				if (line.Trim() == "") continue;

				if (line.IndexOf("Regions") >= 0)
				{
					string[] regions_colon_list = line.Split(new string[] { ":=" }, StringSplitOptions.RemoveEmptyEntries);
					string[] regions = regions_colon_list[1].Split(new string[] {" ", "\t", "\r"}, StringSplitOptions.RemoveEmptyEntries);
					foreach (string s in regions)
						result.region_set.Add(s);
				}
			}

			sr = new StringReader(raw_data);
			
			for (string line = sr.ReadLine(); line != null; line = sr.ReadLine())
			{
				// comment
				if (line.Trim().IndexOf("##") == 0) continue;
				// empty line
				if (line.Trim() == "") continue;

				if (line.IndexOf("Regions") >= 0) continue;

				// parse class to region line
				if(line.Contains("->"))
				{
					string[] left_arrow_right = line.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
					string left = left_arrow_right[0].Trim();

					if (result.class_to_regions.ContainsKey(left))
						throw new Exception("Class->Relationship mapping for " + left + " already defined as: " + result.class_to_regions[left]);
		
					string right = left_arrow_right[1];
					string[] relationships = right.Split('|');

					string[][] relationship_expansions = new string[relationships.Length][];
					for(int k = 0; k < relationships.Length; k++)
					{
						relationship_expansions[k] = relationships[k].Trim().Split(new string[] { " ", "\t", "\r" }, StringSplitOptions.RemoveEmptyEntries);
						foreach (string token in relationship_expansions[k])
						{
							if ((token.IndexOf('_') == 0 && result.region_set.Contains(token.Substring(1)) == false) ||
								(token.IndexOf('@') == 0 && result.region_set.Contains(token.Substring(1)) == false))
								throw new Exception("Region token \"" + token + "\" not found in Regions section");
						}
					}
					result.class_to_regions[left] = relationship_expansions;
				}
				else if(line.Contains(":="))
				{
					string[] left_colon_right = line.Split(new string[] {":="}, StringSplitOptions.RemoveEmptyEntries);
					string left = left_colon_right[0].Trim();
					string right = left_colon_right[1];
					string[] classes = right.Split(new string[] {" ", "\t"}, StringSplitOptions.RemoveEmptyEntries);
					foreach (string s in classes)
					{
						if (result.symbol_to_class.ContainsKey(s))
							throw new Exception("Symbol->Class mapping  for " + s + " already defined for " + result.symbol_to_class[s]);
						result.symbol_to_class[s] = left;
					}
				}
			}
			return result;
		}

		public static RegionMap Load(string filename)
		{
			FileStream fs = new FileStream(filename, FileMode.Open);
			StreamReader sr = new StreamReader(fs);
			RegionMap result = RegionMap.Parse(sr.ReadToEnd());
			sr.Close();
			return result;
		}

		/**
		 * Returns an array of arrays of tokens. 
		 */
		public string[][] GetRegions(string in_symbol)
		{
			string[][] result;
			string symbol_class;
			if (symbol_to_class.TryGetValue(in_symbol, out symbol_class))
			{
				if (class_to_regions.TryGetValue(symbol_class, out result))
					return result;
				else
					throw new Exception("Passed in symbol's (" + in_symbol + ") symbol class (" + symbol_class + ") has no associated regions");
			}
			else
				throw new Exception("Passed in symbol \"" + in_symbol + "\" does not have mapping to a symbol class");
		}
	}
}
