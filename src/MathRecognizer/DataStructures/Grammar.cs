using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DataStructures
{
	public class Grammar
	{
		// grammar mappings
		// can map from non-terminal to terminal or from
		// non-terminal to symbolclass
		Dictionary<string, List<string[]>> production_rules;

		Dictionary<string, HashSet<string>> leftmost_nonterminal_to_parent_nonterminals;

		// all our non terminals
		HashSet<string> nonterminals;
		//  all our terminals
		HashSet<string> terminals;
		// layout data
		HashSet<string> layout_classes;
		// the set of all valid regions
		HashSet<string> region_set;
		// maps symbolclass to a set of possible layouts
		Dictionary<string, string[][]> layoutclass_to_regionlayout;
		// formerly, layoutClassDict
		Dictionary<string, HashSet<string>> layoutclass_to_terminals;
		// formerly, symbolLayoutDict
		Dictionary<string, HashSet<string>> terminal_to_layoutclasses;
		// formerly, classSymbolDict
		Dictionary<string, HashSet<string>> nonterminal_to_terminals;
		// formerly symbolClassDict
		// maps terminals back up to non-terminals
		Dictionary<string, HashSet<string>> terminal_to_nonterminals;
		
		private Grammar()
		{
			production_rules = new Dictionary<string, List<string[]>>();
			leftmost_nonterminal_to_parent_nonterminals = new Dictionary<string, HashSet<string>>();

			layout_classes = new HashSet<string>();
			region_set = new HashSet<string>();
			layoutclass_to_regionlayout = new Dictionary<string, string[][]>();
			terminals = new HashSet<string>();
			nonterminals = new HashSet<string>();

			layoutclass_to_terminals = new Dictionary<string, HashSet<string>>();
			terminal_to_layoutclasses = new Dictionary<string, HashSet<string>>();
			nonterminal_to_terminals = new Dictionary<string, HashSet<string>>();
			terminal_to_nonterminals = new Dictionary<string, HashSet<string>>();
		}

		private void BuildLookaheads()
		{
			HashSet<string> nodes_visited = new HashSet<string>();
			Queue<string> nodes = new Queue<string>();
			nodes.Enqueue("S");
			nodes_visited.Add("S");

			// breadth first traversal througoh rules

			while (nodes.Count > 0)
			{
				string nonterminal = nodes.Dequeue();
				if (nonterminal[0] == '*') continue;	// thsi maps to a terminal

				// parents of this nonterminal that will be added to the children
				HashSet<string> grandparents;
				leftmost_nonterminal_to_parent_nonterminals.TryGetValue(nonterminal, out grandparents);

				List<string[]> mappings;
				// find all the rules this nonterminal maps to
				if (production_rules.TryGetValue(nonterminal, out mappings))
				{
					foreach (string[] rule in mappings)
					{
						// add this nonterminal as a parent to the first nonterminal in this rule
						HashSet<string> parents;
						if (leftmost_nonterminal_to_parent_nonterminals.TryGetValue(rule[0], out parents) == false)
						{
							parents = new HashSet<string>();
							leftmost_nonterminal_to_parent_nonterminals.Add(rule[0], parents);
						}
						parents.Add(nonterminal);
						// also add the ancestors of this parent to the child
						if(grandparents != null)
							foreach (string ancestor in grandparents)
								parents.Add(ancestor);
						// finally, enqueue nodes (so long as they haven't been enqueued before)	
						foreach (string nt in rule)
						{
							if(nodes_visited.Contains(nt)) continue;
							nodes.Enqueue(nt);
							nodes_visited.Add(nt);
						}
					}
				}
			}
		}

		public static Grammar Parse(string raw_grammar)
		{
			Grammar result = new Grammar();
			// remove any carriage returns
			StringReader sr = new StringReader(raw_grammar.Replace("\r",""));
			List<string> lines = new List<string>();
			for (string line = sr.ReadLine(); line != null; line = sr.ReadLine())
			{
				line = line.Trim();
				// remove lines which aren't necessary
				// comment
				if (line.IndexOf("##") == 0) continue;
				// empty line
				if (line.Trim() == "") continue;
				lines.Add(line);
			}

			int grammar_start = lines.IndexOf("GRAMMAR");
			int layout_start = lines.IndexOf("LAYOUT");

			if(grammar_start < 0 || layout_start < 0)
				throw new Exception("Problem parsing grammar, both a GRAMMAR section and a LAYOUT section are required");
			grammar_start++;
			layout_start++;
	
			// figure out grammar and layout sections
			int grammar_end, layout_end = -1;
			if (grammar_start < layout_start)
			{
				grammar_end = layout_start - 2;
				layout_end = lines.Count - 1;
			}
			else
			{
				grammar_end = lines.Count - 1;
				layout_end = grammar_start - 2;
			}

		// parse grammar first
		#region PARSE_GRAMMAR
		//for (string line = sr.ReadLine(); line != null; line = sr.ReadLine())
		for(int k = grammar_start; k <= grammar_end; k++)
		{
			string line = lines[k];
			// commnet
			if (line.Trim().IndexOf("##") == 0) continue;
			// empty line
			if (line.Trim() == "") continue;

			string[] left_arrow_right = line.Split(new string[] {"->"}, StringSplitOptions.RemoveEmptyEntries);
			string left = left_arrow_right[0].Trim();
			string[] right_tokens = left_arrow_right[1].Split('|');
			if (result.production_rules.ContainsKey(left))
				throw new Exception("This Grammer already contains a row for " + left_arrow_right[0]);

			List<string[]> expansions = new List<string[]>();

			for (int j = 0; j < right_tokens.Length; j++)
				expansions.Add(right_tokens[j].Trim().Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries));

			result.production_rules.Add(left, expansions);
		}
		// populate nonterminals_to_terminals and terminals_to_nonterminals
		foreach (string nonterminal in result.production_rules.Keys)
		{
			if (nonterminal[0] == '*')
			{
				List<string[]> production = result.production_rules[nonterminal];
				foreach (string[] terminal in production)
				{
					if (terminal.Length != 1) throw new Exception("Non-Terminal to Terminal mapping contains multiple characters");
					HashSet<string> terminal_set;
					if (result.nonterminal_to_terminals.TryGetValue(nonterminal, out terminal_set) == false)
					{
						terminal_set = new HashSet<string>();
						result.nonterminal_to_terminals.Add(nonterminal, terminal_set);
					}
					terminal_set.Add(terminal[0]);

					HashSet<string> nonterminal_set;
					if (result.terminal_to_nonterminals.TryGetValue(terminal[0], out nonterminal_set) == false)
					{
						nonterminal_set = new HashSet<string>();
						result.terminal_to_nonterminals.Add(terminal[0], nonterminal_set);
					}
					nonterminal_set.Add(nonterminal);

					result.terminals.Add(terminal[0]);
				}
			}
			result.nonterminals.Add(nonterminal);
		}

		result.BuildLookaheads();

		#endregion
			
					// now parse layouts
		#region PARSE_LAYOUTS
			//for (string line = sr.ReadLine(); line != null; line = sr.ReadLine())
			for(int k = layout_start; k <= layout_end; k++)
			{
				string line = lines[k];
				if (line.IndexOf("Regions") == 0)
				{
					string[] regions_colon_list = line.Split(new string[] { ":=" }, StringSplitOptions.RemoveEmptyEntries);
					string[] regions = regions_colon_list[1].Split(new string[] {" ", "\t", "\r"}, StringSplitOptions.RemoveEmptyEntries);
					foreach (string s in regions)
						result.region_set.Add(s);
				}
			}

			for(int k = layout_start; k <= layout_end; k++)
			{
				string line = lines[k];

				if (line.IndexOf("Regions") == 0) continue;

				// parse class to region line
				if(line.Contains("->"))
				{
					string[] left_arrow_right = line.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
					string left = left_arrow_right[0].Trim();

					if (result.layoutclass_to_regionlayout.ContainsKey(left))
						throw new Exception("Class->Relationship mapping for " + left + " already defined as: " + result.layoutclass_to_regionlayout[left]);
					if (result.layout_classes.Contains(left))
						throw new Exception("LayoutClass \"" + left + "\" already defined");
					result.layout_classes.Add(left);

					string[][] relationship_expansions;

					if (left_arrow_right.Length < 2)
					{
						relationship_expansions = new string[1][];
						relationship_expansions[0] = new string[0];
					}
					else
					{

						string right = left_arrow_right[1];
						string[] relationships = right.Split('|');

						relationship_expansions = new string[relationships.Length][];
						for (int j = 0; j < relationships.Length; j++)
						{
							relationship_expansions[j] = relationships[j].Trim().Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
							foreach (string token in relationship_expansions[j])
							{
								if ((token.IndexOf('_') == 0 && result.region_set.Contains(token.Substring(1)) == false) ||
									(token.IndexOf('@') == 0 && result.region_set.Contains(token.Substring(1)) == false))
									throw new Exception("Region token \"" + token + "\" not found in Regions section");
							}
						}
					}
					result.layoutclass_to_regionlayout[left] = relationship_expansions;
				}
				else if(line.Contains(":="))
				{
					string[] left_colon_right = line.Split(new string[] {":="}, StringSplitOptions.RemoveEmptyEntries);
					string layout_class = left_colon_right[0].Trim();
					string right = left_colon_right[1];
					string[] terminals = right.Split(new string[] {" ", "\t"}, StringSplitOptions.RemoveEmptyEntries);
					foreach (string t in terminals)
					{
						// add entry to map layout class to possible terminals
						HashSet<string> possible_terminals;
						if(result.layoutclass_to_terminals.TryGetValue(layout_class, out possible_terminals) == false)
						{
							possible_terminals = new HashSet<string>();
							result.layoutclass_to_terminals.Add(layout_class, possible_terminals);
						}
						possible_terminals.Add(t);

						HashSet<string> possible_layoutclass;
						if (result.terminal_to_layoutclasses.TryGetValue(t, out possible_layoutclass) == false)
						{
							possible_layoutclass = new HashSet<string>();
							result.terminal_to_layoutclasses.Add(t, possible_layoutclass);
						}
						possible_layoutclass.Add(layout_class);
					}
				}
			}
		#endregion

			return result;
		}

		public static Grammar Load(string filename)
		{
			FileStream fs = new FileStream(filename, FileMode.Open);
			StreamReader sr = new StreamReader(fs);
			Grammar result = Grammar.Parse(sr.ReadToEnd());
			sr.Close();
			return result;
		}

		/*
		 * Returns an array of possible grammar expansions given a non-terminal
		 * Returns null if there is no mapping from the given non-terminal
		 */
		public List<string[]> GetProductions(string nonterminal)
		{
			List<string[]> result = null;
			if (production_rules.TryGetValue(nonterminal, out result))
				return result;
			return null;
		}

	
		/*
		 * Returns an array of arrays of layout tokens
		 * A layout token can be either a non-terminal in the grammar or a layout region.
		 * Layout regions which start with @ are optional, while regions which start
		 * with an _ are required
		 * 
		 * The input to this method is either a terminal symbol (a, \cos, +, etc) or a layout class 
		 * (Ascender, Descender, etc).  For this reason, symbol classes and terminal symbols cannot have
		 * identical names.
		 * 
		 * throws an exceptioin when there is no mapping from either a terminal or a symbol class to a layout
		 */

		public List<string[]> GetRegionLayouts(string in_terminal_or_layoutclass)
		{
			// returns a list of region layouts

			List<string[]> result = new List<string[]>();

			// we got a layout class directly
			if (layout_classes.Contains(in_terminal_or_layoutclass))
			{
				string[][] region_layouts;
				if (layoutclass_to_regionlayout.TryGetValue(in_terminal_or_layoutclass, out region_layouts))
				{
					foreach (string[] rl in region_layouts)
						result.Add(rl);
				}
				else
					throw new Exception("LayoutClass \"" + in_terminal_or_layoutclass + "\" not found");
			}
			else
			{
				HashSet<string> lcs;
				if (terminal_to_layoutclasses.TryGetValue(in_terminal_or_layoutclass, out lcs))
				{
					foreach (string lc in lcs)
					{
						string[][] region_layouts;
						if (layoutclass_to_regionlayout.TryGetValue(lc, out region_layouts))
						{
							foreach (string[] rl in region_layouts)
								result.Add(rl);
						}
						else
							throw new Exception("LayoutClass \"" + lc + "\" not found");
					}
				}
				else
					throw new Exception("Terminal \"" + in_terminal_or_layoutclass + "\" does not map to a LayoutClass");
			}

			return result;
		}

		public bool NonTerminalMapsToTerminal(string non_terminal, string terminal)
		{
			HashSet<string> terminal_set;
			return nonterminal_to_terminals.TryGetValue(non_terminal, out terminal_set) && terminal_set.Contains(terminal);
		}

		public List<string> GetTerminalsFromNonTerminal(string non_terminal)
		{
			List<string> result = new List<string>();
			HashSet<string> terminal_set;
			if (nonterminal_to_terminals.TryGetValue(non_terminal, out terminal_set))
			{
				foreach (string s in terminal_set)
					result.Add(s);
			}
			return result;
		}

		public bool TerminalMapsToLayoutClass(string terminal, string layout_class)
		{
			HashSet<string> layout_class_set;
			return terminal_to_layoutclasses.TryGetValue(terminal, out layout_class_set) && layout_class_set.Contains(layout_class);
		}

		public List<string> GetLayoutClasses()
		{
			List<string> result = new List<string>();
			foreach (string s in layout_classes)
				result.Add(s);
			return result;
		}

		public bool NonTerminalCanGenerateTerminal(string nonterminal, string terminal)
		{
			HashSet<string> terminal_parents;
			if(terminal_to_nonterminals.TryGetValue(terminal, out terminal_parents))
			{
				foreach (string nt in terminal_parents)
				{
					if (nt == nonterminal) return true;
					HashSet<string> parents;
					if (leftmost_nonterminal_to_parent_nonterminals.TryGetValue(nt, out parents))
						if (parents.Contains(nonterminal)) return true;
				}
			}

            // if this NT doesn't directly derive terminals, try its children
            if ( !nonterminal.StartsWith( "*" ) ) {
                List< string[] > productions = GetProductions( nonterminal );
                foreach ( string[] prod in productions ) {
                    if ( prod.Length > 1 ) continue;
                    if ( NonTerminalCanGenerateTerminal( prod[ 0 ], terminal ) ) return true;
                }
            }

			return false;
		}

		public List<string> GetLayoutClassesFromTerminal(string terminal)
		{
			List<string> result = new List<string>();
			HashSet<string> lcs;
			if (terminal_to_layoutclasses.TryGetValue(terminal, out lcs))
			{
				foreach (string s in lcs)
					result.Add(s);
			}
			return result;
		}

		public bool IsTerminal(string s)
		{
			return terminals.Contains(s);
		}

		public bool IsNonTerminal(string s)
		{
			return nonterminals.Contains(s);
		}

		public bool IsLayoutClass(string s)
		{
			return layout_classes.Contains(s);
		}
	}
}

