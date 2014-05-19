using System;
using System.Collections.Generic;
namespace DataStructures
{
	public class ValidParseTree : IComparable
	{
		public ParseTreeNode root;
		public double score;
		
		// Stroke-level recognition score.
		private double scoreTree(ParseTreeNode p) {

            // weighted harmonic mean
            /*
            List< SymbolTreeNode > all_children = new List< SymbolTreeNode >();
            p.PopulateChildrenLexerResults( all_children );
            int totalStrokes = 0;
            double harmonicSum = 0;
            foreach ( SymbolTreeNode stn in all_children ) {
                totalStrokes += stn.symbolData.segment.strokes.Count;
                harmonicSum += stn.symbolData.segment.strokes.Count / ( stn.symbolData.segment.classification[ 0 ].probability );
            }
            return totalStrokes / harmonicSum;
            */

            
			if (p is SymbolTreeNode) {
				SymbolTreeNode s = (SymbolTreeNode)p;
				return s.symbolData.segment.strokes.Count * s.symbolData.segment.classification[ 0 ].probability;
			}
			double sum = 0.0;
			foreach (ParseTreeNode child in p.children) {
				sum += scoreTree(child);
			}
			return sum;
            
		}
		
		public int CompareTo(object obj) 
		{
			if (obj is ValidParseTree) {
				ValidParseTree t = (ValidParseTree) obj;
				if (t.score > score ) 
					return 1;
				else if (t.score < score) 
					return -1;
				else 
					return 0;
			}
			throw new ArgumentException("ValidParseTree::CompareTo(): object is not a ValidParseTree.");
		}
		
		public ValidParseTree (ParseTreeNode rootNode)
		{
			root = rootNode.clone();
			score = scoreTree(root);
		}
	}
}

