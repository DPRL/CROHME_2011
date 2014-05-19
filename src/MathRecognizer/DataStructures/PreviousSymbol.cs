using System;
using System.Collections.Generic;
namespace DataStructures
{
	public class PreviousSymbol
	{
		public LexerResult symbol;
		public ParseTreeNode parent;
		public List<string> regions;
		public List<string> regionNonterminals;
		public bool regionsOptional;
		
		public PreviousSymbol (ParseTreeNode p, bool optional) {
			symbol = null;
			parent = p;
			regions = new List<string>();
			regionNonterminals = new List<string>();
			regionsOptional = optional;
		}
		
		public PreviousSymbol (LexerResult s, ParseTreeNode p, List<string> r, List<string> n,
		                       bool optional)
		{
			symbol = (LexerResult) s.Clone(); // (lex results never mutated)
			parent = p; // .clone(); // Capture state of subtree on construction.
			
			if ( s.BLEFT != null ) parent.children.Add( s.BLEFT );
			if ( s.TLEFT != null ) parent.children.Add( s.TLEFT );
			
			// Avoid crashing on null values.
			if (r == null) {
				regions = new List<string>();
				regionNonterminals = new List<string>();
			} else {
				regions = r;
				regionNonterminals = n;
			}
			
			regionsOptional = optional;
		}
		
		public PreviousSymbol clone() {
			PreviousSymbol p = new PreviousSymbol();
			p.symbol = symbol;
			p.parent = parent.clone();
			p.regions = regions;
			p.regionNonterminals = regionNonterminals;
			p.regionsOptional = regionsOptional;
			
			return p;
		}
		
		public PreviousSymbol() {
		}
	}
}

