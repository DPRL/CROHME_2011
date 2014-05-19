using System;
using System.Collections.Generic;
using System.IO;
namespace DataStructures
{
	public class RelationTreeNode : ParseTreeNode		
	{
		// Simply adds an additional field to identify spatial relationship.
		public string relationType;
		public ParseTreeNode parent;
		
		public RelationTreeNode(string relationLabel, string nodeLabel) : base(nodeLabel)
		{
			relationType = relationLabel;
			strokes = new List<Stroke>(); // empty stroke list.
			lbt = new LBT(new List<Stroke>(), LBT.DefaultAdjacentCriterion );
		}	
		
		public bool empty() {
			return strokes.Count == 0;
		}
		
		public override ParseTreeNode clone() {
			// Node labels and stoke data will be fixed for a given node (ref. only), but children will
			// differ for different parse trees (new copy).
			RelationTreeNode nodeCopy = new RelationTreeNode( relationType, nodeType );
			nodeCopy.strokes = strokes;
			nodeCopy.lbt = null;
			if (lbt != null)
			nodeCopy.lbt = new LBT( lbt ); // copy lbt
			foreach (ParseTreeNode child in children) {
				nodeCopy.children.Add(child.clone());
			}
			return nodeCopy;
		}
		
		public override string ToString(int indentLevel)
		{
			string indent = "";
			for (int i = 0; i < indentLevel; i++ )
				indent += "  ";
			
			string outString = string.Format( "{0}<< {1} ({2}) >>", indent, relationType, lbt.strokes.Count );
			if (children != null) {
				foreach (ParseTreeNode child in children) {
					outString += "\n" + child.ToString(indentLevel + 1);
				}
			}
			return outString;
		}
		
		public override void ShowTree( int indentLevel, TextWriter sout) {
			if ( sout == null ) sout = Console.Out;
			string indent = "";
			for (int i = 0; i < indentLevel; i++ )
				indent += "  ";
			sout.WriteLine( string.Format( "{0}<< {1} ({2}) >>", indent, relationType, lbt.strokes.Count ) );
			if (children != null) {
				foreach (ParseTreeNode child in children) {
					if (child == this) {
						throw new ArgumentException(
						                            string.Format("ERROR: ParseTreeNode has child that is same as parent ({0}).",
						                                          nodeType));
					}
					child.ShowTree( indentLevel + 1, sout );
				}
			}
		}
		
		public override string ToString()
		{
			return "Type::RelationTreeNode";
		}

		public override string ChildMathML(string right_adjacent)
		{
			string right = right_adjacent;
			for (int k = this.children.Count - 1; k >= 0; k--)
			{
				// opening tag
				//if ( nodeType == "SUPER" )
				right = this.children[k].ChildMathML(right);
				// closing tag
			}
			return right;
		}
	}
}

