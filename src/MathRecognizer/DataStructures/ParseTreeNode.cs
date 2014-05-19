using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace DataStructures
{
	public class ParseTreeNode
	{
		public static ParseTreeNode EndOfBaseline = new ParseTreeNode( "END-OF-BASELINE" );
		
		public string nodeType;
		public List<ParseTreeNode> children;
		
		// NOTE: these record lbt on construction of a node (constants).
		// "PreviousSymbol" class objects are used to update LBTs and available strokes during
		// parsing.
		public List<Stroke> strokes; //**LBT contains its associated strokes.
		public LBT lbt;
		public LexerResult lexResult;
		
		// * Watch management of LBT
		public ParseTreeNode(string nodeLabel)
		{
			// Assign label, create empty list of child nodes. No LBT.
			nodeType = nodeLabel;
			children = new List<ParseTreeNode>();
			lbt = null;
			strokes = null;
		}
		
		public ParseTreeNode(string nodeLabel, LBT tree)
		{ 
			nodeType = nodeLabel;
			children = new List<ParseTreeNode>();
			lbt = new LBT(tree); // create a copy.
			strokes = lbt.strokes;
		}
		
		public virtual ParseTreeNode clone() {
			// Node labels and stoke data will be fixed for a given node (ref. only), but children will
			// differ for different parse trees (new copy).
			ParseTreeNode nodeCopy = new ParseTreeNode(nodeType);
			nodeCopy.strokes = strokes;
			nodeCopy.lbt = null;
			if (lbt != null)
				nodeCopy.lbt = new LBT( lbt ); // copy lbt
			foreach (ParseTreeNode child in children) {
		 		nodeCopy.children.Add(child.clone());
			}
			return nodeCopy;
		}
		
		public ParseTreeNode() {
			children = new List<ParseTreeNode>();
		}
		
		public virtual string ToString(int indentLevel)
		{
			string indent = "";
			for (int i = 0; i < indentLevel; i++ )
				indent += "  ";
			
			string outString = string.Format("{0}{1}",indent,nodeType);
			foreach (ParseTreeNode child in children) {
				outString += "\n" + child.ToString(indentLevel + 1);
			}
			return outString;
		}
		
		public virtual void ShowTree(int indentLevel, TextWriter sout) {
			if ( sout == null ) sout = Console.Out;
			string indent = "";
			for (int i = 0; i < indentLevel; i++ )
				indent += "  ";
			sout.WriteLine(string.Format("{0}{1}",indent,nodeType));
			
			if (children != null) {
				foreach (ParseTreeNode child in children) {
					if (child == this) {
						throw new ArgumentException(
						                            string.Format("ERROR: ParseTreeNode has child that is same as parent ({0}).",nodeType));
					}
					child.ShowTree( indentLevel + 1, sout );
				}
			}
		}
		
		// this node assumed to be the root!
		public string ToInkML(string ui_annotation) 
		{
			StringBuilder sb = new StringBuilder();
			List< SymbolTreeNode > children = new List< SymbolTreeNode >();
			PopulateChildrenLexerResults( children );
			List< Stroke > all_strokes = new List< Stroke >();
			Dictionary< string, InkML.Trace > traces = new Dictionary< string, InkML.Trace >();
			InkML inkml = new InkML();
			inkml.mathml_expression = new InkML.MathML();
			inkml.mathml_expression.mathml = "<math xmlns='http://www.w3.org/1998/Math/MathML'><mrow>";
						
			foreach ( SymbolTreeNode stn in children ) {
				foreach ( Stroke s in stn.symbolData.segment.strokes ) all_strokes.Add( s );
			}
			foreach ( Stroke s in all_strokes.OrderBy( s => int.Parse( s.stroke_id ) ) ) {
				InkML.Trace t = new InkML.Trace { id = s.stroke_id, points = s.points };
				traces.Add( s.stroke_id, t );
				inkml.traces.Add( t );
			}
			foreach ( SymbolTreeNode stn in children ) {
				InkML.TraceGroup tg = new InkML.TraceGroup();
				foreach ( var s in stn.symbolData.segment.strokes ) {
					tg.trace_views.Add( traces[ s.stroke_id ] );
				}
				tg.truth = stn.nodeType;
				tg.mathml_href = stn.id;
				
				inkml.trace_groups.Add( tg );
				// inkml.classification_attributes.Add( 
				// just add to one mathml row for now
				
				//inkml.mathml_expression.mathml += "<mi>" + tg.truth + "</mi>";
			}
			
			inkml.mathml_expression.mathml = ToMathML();
			inkml.annotations["UI"] = ui_annotation;

			return inkml.ToInkML( null );
		}

        public string ToMathML()
        {
            if (this is RelationTreeNode || this is SymbolTreeNode)
                throw new Exception("This Must be called on the root ParseTreeNode");

            return String.Format("<math xmlns='http://www.w3.org/1998/Math/MathML'>{0}</math>", ChildMathML(null));
        }

		// include generated mathml from right nodes to siblings to their left (because of mrow)
        virtual public string ChildMathML(string right)
        {
		if (this is RelationTreeNode || this is SymbolTreeNode)
			throw new Exception("Child classes must re-implement this method");
		
		if ( this.children.Count > 0 && ( this.children[ 0 ] is RelationTreeNode || this.children[ 0 ] is SymbolTreeNode ) ) {
		
			string this_symbol = null;
			
			if ( this.children.Count == 2 ) {
				// one relation
				string relationship = (this.children[1] as RelationTreeNode).relationType;
				switch (relationship)
				{
					// sqrt
					case "CONTAINS":
						this_symbol = String.Format("<msqrt>{0}</msqrt>", this.children[1].ChildMathML(null));
						break;
					// supersscript
					case "SUPER":
						this_symbol = String.Format("<msup>{0}{1}</msup>", this.children[ 0 ].ChildMathML( null ), this.children[1].ChildMathML(null));
						break;
					// subscript
					case "SUBSC":
						this_symbol = String.Format("<msub>{0}{1}</msub>", this.children[ 0 ].ChildMathML( null ), this.children[1].ChildMathML(null));
						break;
				}
			} else if ( this.children.Count == 3 ) {
				// frac/sum/etc.
                string above = null, below = null, this_sym = null;
                bool is_frac = false;
				foreach ( ParseTreeNode ptn in this.children )
				{
					if ( ptn is RelationTreeNode ) {
						RelationTreeNode rtn = null;
						rtn = ptn as RelationTreeNode;
						
						if (rtn.relationType == "ABOVE")
							above = rtn.ChildMathML(null);
						if (rtn.relationType == "BELOW")
							below = rtn.ChildMathML(null);
                    } else if ( ptn is SymbolTreeNode ) {
                        SymbolTreeNode stn = ptn as SymbolTreeNode;
                        this_sym = stn.ChildMathML( null );
                        if ( stn.nodeType.Equals( "-" ) ) is_frac = true;
                    }
				}

                if ( is_frac ) {
                    this_symbol = String.Format( "<mfrac>{0}{1}</mfrac>", above, below );
                } else {
                    this_symbol = String.Format( "<msubsup>{0}{1}{2}</msubsup>", this_sym, below, above );
                }

			} else {
			
				for(int k = this.children.Count - 1; k >= 0; k--)
				{
					right = this.children[k].ChildMathML(right);
				}
			
			}
						
			if ( right == null ) return this_symbol;
			if ( this_symbol == null ) return right;
			
			// something to our right
			return String.Format("<mrow>{0}{1}</mrow>", this_symbol, right);
				
		} else {
			
			for(int k = this.children.Count - 1; k >= 0; k--)
			{
				right = this.children[k].ChildMathML(right);
			}
			return right;
				
		}
        }

		public void PopulateChildrenLexerResults( List< SymbolTreeNode > children ) {
			if ( this.children == null ) return;
			foreach ( ParseTreeNode n in this.children ) {
				if ( n is SymbolTreeNode ) children.Add( n as SymbolTreeNode );
				n.PopulateChildrenLexerResults( children );
			}
		}
	}
}

