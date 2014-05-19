using System;
using System.IO;
using System.Threading;
namespace DataStructures
{
	public class SymbolTreeNode : ParseTreeNode
	{
		public LexerResult symbolData;
		
		private static int INST_ID = 0;
		private int id_num;
		public string id {
			get { return "stn_" + id_num; }	
		}
		
		public SymbolTreeNode (LexerResult selectedResult) 
		{
			nodeType = selectedResult.segment.classification[0].symbol;
			symbolData = selectedResult; // not cloning (assumption: lexer results never modified).
			lexResult = selectedResult; // redundant?
			strokes = selectedResult.segment.strokes;
			lbt = selectedResult.lbt;
			children = null;
			id_num = Interlocked.Increment(ref INST_ID);
		}
		
		public override ParseTreeNode clone() {
			// Node labels and stoke data will be fixed for a given node (ref. only), but children will
			// differ for different parse trees (new copy).
			LexerResult resultCopy = (LexerResult) symbolData.Clone();
			SymbolTreeNode nodeCopy = new SymbolTreeNode(resultCopy);
			
			nodeCopy.strokes = strokes;
			nodeCopy.lbt = null;
			if (lbt != null)
			nodeCopy.lbt = new LBT( lbt ); // copy lbt
			
			if (children != null) {
				foreach (ParseTreeNode child in children) {
					nodeCopy.children.Add(child.clone());
				}
			}
			nodeCopy.symbolData = symbolData;
			return nodeCopy;
		}
		
		public override string ToString(int indentLevel)
		{
			string indent = "";
			for (int i = 0; i < indentLevel; i++ )
				indent += "  ";
			
			string outString = string.Format("{0}{1}",indent, symbolData.segment );
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
			sout.WriteLine(string.Format("{0}{1}",indent, symbolData.segment ));
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

		public override string ChildMathML(string right_adjacent)
		{
			// fill in tracegroup id later
			
			// determine tag name
			string tagName = "mi";
			
			switch ( this.nodeType ) {
			
				case "0":
				case "1":
				case "2":
				case "3":
				case "4":
				case "5":
				case "6":
				case "7":
				case "8":
				case "9":
					tagName = "mn";
					break;
				
				case "=":
				case "\\rightarrow":
				case "\\neq":
				case "\\leq":
				case "\\geq":
				case "-":
				case "+":
				case "\\":
				case "\\pm":
				case "\\times":
				case "\\div":
                case "\\sum":
					tagName = "mo";
					break;
			
				default:
					break;
				
			}
			
			string this_symbol = String.Format( "<{2} xml:id=\"{0}\">{1}</{2}>", this.id, this.nodeType, tagName );

			if (right_adjacent == null)	// ie, nothing to our right
					return this_symbol;
			
			// something to our right
			return String.Format("<mrow>{0}{1}</mrow>", this_symbol, right_adjacent);
		}
		
	}
}