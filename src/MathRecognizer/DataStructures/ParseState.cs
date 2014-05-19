using System;
using System.Collections.Generic;
	
namespace DataStructures
{
	public class ParseState
	{
		public PreviousSymbol currentSymbol;
		public int unusedInputStrokes;
		public int unusedStrokes;
		public int minRequiredStrokes;

		// Attributes fixed on construction.
		public ParseTreeNode rootNode;
		public bool completesParse;
		
		// Attributes updated during parsing (through synthesisis, i.e. bottom-up).		
		public List<LexerResult> candidateSymbols;
		public PreviousSymbol previous;
		public int numberStrokes;
	
		
		public ParseTreeNode GetNode() {
			return rootNode;
		}
		
		public void AddNode(ParseTreeNode node) {
			rootNode.children.Add(node);
		}
		
		public void RemoveLastChild() {
			int lastIndex = rootNode.children.Count - 1;
			rootNode.children.RemoveAt(lastIndex);
		}
		
		public void SetChildren(ParseTreeNode node) {
			RemoveSubtrees();
			AddNode(node);
		}
		
		public void SetChildren(List<ParseTreeNode> nodelist) {
			RemoveSubtrees();
			foreach (ParseTreeNode node in nodelist)
				rootNode.children.Add(node);
		}
		
		public void RemoveSubtrees() {
			rootNode.children.Clear();
		}
		
		
		/*public List<ParseState> Parse(List<string> nonterminals, int k, bool finishesParse) {
			RemoveSubtrees;
			
			List<ParseState> stateList = new List<ParseState>();
			foreach (string nonterminal in nonterminals) {
				ParseState nextChild = AddNonterminal(nonterminal);
				nextChild.
			return stateList
		}*/
		
		// Add a node to current tree in this state, generate new state for the child.
		public ParseState AddNonterminal(string nonterminal, bool final) {
			ParseTreeNode Symbol = new ParseTreeNode(nonterminal);
			ParseState newState = new ParseState(this, Symbol);
			AddNode(Symbol); 
			newState.completesParse = final;
			return newState;
		}
		
		// Attach nested region to the child.
		// **Need to be careful to set attributes correctly here.
		public ParseState AddNestedRegion(RelationTreeNode relationNode, SymbolTreeNode snode) {
			// Create new child node. *Maintain previous node (depth-first
			// parse).
			ParseState newState = new ParseState();
			
			// Attributes..
			newState.rootNode = relationNode;
			newState.numberStrokes = relationNode.lbt.strokes.Count; // Only need to consume strokes in this sub-region.
			newState.completesParse = false; // This branch cannot be the final NT for a rule.
			
			// Wipe out candidate and previous symbols.
			newState.candidateSymbols = null;
			newState.previous = new PreviousSymbol();
			newState.previous.symbol = new LexerResult();
			newState.previous.regionsOptional = true; 
			
			/*if (relationNode.lbt.strokes == null) 
				Console.WriteLine("RELATION NODE LBT: NULL!!");
			else
				Console.WriteLine("RELATION NODE LBT: {0} strokes", relationNode.lbt.strokes.Count);
			*/
			newState.previous.symbol.lbt = relationNode.lbt;

			
			// COMPLETING PARSE?
			//newState.completesParse = previous.parent.
			
			// Attach region to parent node.
			previous.parent.children.Add(relationNode);
			
			return newState;
		}
		
		public ParseState RemoveNestedRegion(RelationTreeNode relationNode) {
			previous.parent.children.Remove(relationNode);
			return this;
		}
		                                  
		
		
		public void Update(ParseState childState) {
			candidateSymbols = childState.candidateSymbols;
			previous = childState.previous;
			
			// Handle nested regions differently than other symbols.
			if (childState.rootNode is RelationTreeNode) {
				if (childState.numberStrokes == 0) 
					// Reduce by number of strokes.
					numberStrokes = numberStrokes - childState.rootNode.strokes.Count;
			}
			else 
				numberStrokes = childState.numberStrokes;
			
			// Original root node and "completes parse" attributes should be unchanged.
			
		}
		
		public ParseState clone() {
			ParseState s = new ParseState();
			s.candidateSymbols = candidateSymbols;
			s.rootNode = rootNode;   // Do not clone.  
			s.numberStrokes = numberStrokes;
			s.completesParse = completesParse;
			
			if (previous == null)
				s.previous = null;
			else 
				s.previous = previous; //.clone(); : LexerResult are never modified!
			
			return s;
		}
		
		// **NOTE: we do not search for symbols at this point.
		public ParseState(ParseState parent, PreviousSymbol p) {
			candidateSymbols = null;
			previous = p;
			
			completesParse = parent.completesParse;
			numberStrokes = parent.numberStrokes - p.symbol.segment.strokes.Count;
			rootNode = new SymbolTreeNode(p.symbol);
		}
		
		// Inherit + modify root node constructor.
		public ParseState(ParseState pstate, ParseTreeNode node) {
			candidateSymbols = pstate.candidateSymbols;
			numberStrokes = pstate.numberStrokes;
			completesParse = pstate.completesParse;
			previous = pstate.previous;
			
			// Inherit all attributes, but modify the root node.
			rootNode = node;
		}
		
		public ParseState(ParseState pstate, RelationTreeNode node) {
			candidateSymbols = pstate.candidateSymbols;
			numberStrokes = pstate.numberStrokes;
			completesParse = pstate.completesParse;
			previous = pstate.previous;
			
			// Inherit all attributes, but modify the root node.
			rootNode = node;
		}
		
		public ParseState() {
		}
		
		// Note: this constructor is primarily for testing.
		public ParseState(List<LexerResult> symbolList, ParseTreeNode node, int strokes, bool final) {
			candidateSymbols = symbolList;
			rootNode = node;
			previous = null;
			numberStrokes = strokes;
			completesParse = final;   // BUG?
			//regionsOptional = true;
		}
		
		
		// Constructor for root node.
		public ParseState(ParseTreeNode inNode, LBT tree) {

			// Create root node for parser.
			rootNode = inNode;

			// Construct previous node.
			// **Note: currently a number of fields will be uninitialized.
			previous = new PreviousSymbol();
			previous.symbol = new LexerResult();
			previous.symbol.lbt = tree;
			previous.regionsOptional = true; 
			
			// Remaining attribute definitions.			
			candidateSymbols = null; // no symbols considered.
			completesParse = true;  // root node.
			numberStrokes = tree.strokes.Count;
		}
		
		public ParseState(ParseTreeNode inNode, LBT tree, int fixedStrokeNumber) {
			
			// Create root node for parser.
			rootNode = inNode;

			// Construct previous node.
			// **Note: currently a number of fields will be uninitialized.
			previous = new PreviousSymbol();
			previous.symbol = new LexerResult();
			previous.symbol.lbt = tree;
			previous.regionsOptional = true; 
			
			// Remaining attribute definitions.			
			candidateSymbols = null; // no symbols considered.
			completesParse = true;  // root node.
			numberStrokes = tree.strokes.Count;
			
			// Hard-code number of strokes (for testing).
			numberStrokes = fixedStrokeNumber;
		}
		
		// Set number of strokes using the LBT directly.
		public ParseState(List<LexerResult> symbolList, ParseTreeNode node, bool final) {
			candidateSymbols = symbolList;
			rootNode = node;
			previous = null;
			numberStrokes = node.lbt.strokes.Count; // Obtain from node itself. 
			completesParse = final; 
		}
		
		public override string ToString ()
		{
			string candidateString = "none";
			string candidateTop = "none";
			if (candidateSymbols != null && candidateSymbols.Count > 0) {
				candidateString = string.Format("{0}",candidateSymbols.Count);
				candidateTop = candidateSymbols[0].segment.ToString();
			}
			
			
			string previousSymbol = "none";
			if (previous != null && previous.symbol != null && previous.symbol.segment != null)
				previousSymbol = previous.symbol.segment.ToString();
			
			string childString = "none";
			if (rootNode.children != null)
				childString = string.Format("{0}",rootNode.children.Count);
			
			return string.Format ("  :: ParseState({0}) ::\n     Previous: {2}\n     Top Cand.: {6}, Alternatives: {1}\n     Children: {5}, Strokes Available: {3}, " +
				"Completes Parse: {4}", rootNode.nodeType, candidateString, previousSymbol, 
			                      numberStrokes, completesParse, childString, candidateTop);
		}
	}
}