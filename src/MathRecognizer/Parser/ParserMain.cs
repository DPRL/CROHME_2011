/* Parser.cs
 * 
 * Main data structures for math notation recognizer.
 *
 */

// Indiscriminate includes at this point.
using System;
using System.Collections.Generic;
using System.IO;
using DataStructures;
using Lexer;
using System.Collections;
using System.Text;

namespace Parser
{	
	public class ParserMain
	{
		// the grammar for math expressions
		private Grammar grammar;
		private List<string> layoutClasses;

		// To use horizontal adjacency code.
		//public static Dictionary< string, SymbolClass > symbolLayoutClassMap;
		
		// Constants, parameters (as intance attributes).
		public int MAX_NEIGHBORS;
		public int TOP_N;
		public int accesses, hits;
		
	
		// Collection of valid Parses (instance variable: one for each separate parser
		// instantiation).
		public ParseTreeNode treeRoot;
		public List< Stroke > strokeList;
		public LBT initLBT;
		public List< ValidParseTree > validParses;
		
		// Lexical analyzer (segmenter + classifier).
		public LexerMain lexer;		
		public InkML inputInkML;
		
		public List< LexerResult > candidateSymbols;
		public PreviousSymbol currentSymbol;
		public int unusedStrokes;
		public int unusedInputStrokes;

		public int minRequiredStrokes;

		public Stack parseStateStack;

		public int apply_rule_counter;
		
		// BEGIN ADDED JULY 7, 12
		
		public List< ValidParseTree > topLevelParser() {
			parse( initLBT, treeRoot, null );
			validParses.Sort( ( v1, v2 ) => v1.score < v2.score ? 1 : v1.score > v2.score ? -1 : 0 );
			return validParses;
		}
		
		// TODO: update to construct LBT here?
		public void parse( LBT tree, ParseTreeNode n, List< ParseTreeNode > additional_nodes ) {
			candidateSymbols = lexer.Start( tree, MAX_NEIGHBORS );
			currentSymbol = null;
			LBT oldtree = initLBT;
			unusedStrokes = tree.strokes.Count;
			List< ParseTreeNode > nodes = additional_nodes == null ? new List< ParseTreeNode >() : new List< ParseTreeNode >( additional_nodes );
			nodes.Insert( 0, n );
			initLBT = tree;
			applyRules( nodes );
			initLBT = oldtree;
		}
		
		public PartitionResultWrapper attachSymbol( LBT lbt, LexerResult c ) {
			// if ( currentSymbol == null ) return new PartitionResultWrapper( null, null ); // empty
			
			bool final = false; // ??
			
			PartitionResultWrapper p = lexer.Partition( new LBT( lbt ),
			                                            currentSymbol == null ? null : currentSymbol.symbol.segment,
			                                            c == null ? null : c.segment,
								              currentSymbol == null ? PartitionClass.Regular : getPartitionClass( currentSymbol.symbol.segment.classification[ 0 ].note ),
								              getPartitionClass( c == null ? null : c.segment.classification[ 0 ].note ),
                                              currentSymbol == null ? null : currentSymbol.symbol.TLEFT,
                                              currentSymbol == null ? null : currentSymbol.symbol.BLEFT );
			
			// if the symbol already has regions assigned, they need to be removed
			if ( currentSymbol != null /* && !p.result.empty() */ ) {
				List< ParseTreeNode > n_children_tmp = new List< ParseTreeNode >( currentSymbol.parent.children );
				for ( int i = 0; i < n_children_tmp.Count; i++ ) if ( n_children_tmp[ i ] is RelationTreeNode ) currentSymbol.parent.children.Remove( n_children_tmp[ i ] );
			}
			
			// if not a legal partition or leftover strokes, backtrack
			if ( !legalPartition( p, currentSymbol ) || ( final && unusedStrokes > 0 ) ) return null;
			
			// otherwise, assign new regions to the current symbol
			if ( p.result.ABOVE.lbt.strokes.Count != 0 ) {
				currentSymbol.parent.children.Add( p.result.ABOVE );
				unusedStrokes -= p.result.ABOVE.lbt.strokes.Count;
			}
			
			if ( p.result.BELOW.lbt.strokes.Count != 0 ) {
				currentSymbol.parent.children.Add( p.result.BELOW );
				unusedStrokes -= p.result.BELOW.lbt.strokes.Count;
			}
			
			if ( p.result.CONTAINS.lbt.strokes.Count != 0 ) {
				currentSymbol.parent.children.Add( p.result.CONTAINS );
				unusedStrokes -= p.result.CONTAINS.lbt.strokes.Count;
			}
			
			if ( p.result.SUBSC.lbt.strokes.Count != 0 ) {
				currentSymbol.parent.children.Add( p.result.SUBSC );
				unusedStrokes -= p.result.SUBSC.lbt.strokes.Count;
			}
			
			if ( p.result.SUPER.lbt.strokes.Count != 0 ) {
				currentSymbol.parent.children.Add( p.result.SUPER );
				unusedStrokes -= p.result.SUPER.lbt.strokes.Count;
			}
			
			// add BLEFT/TLEFT to "right" symbol
			// actual attachment delayed until the new PreviousSymbol object is created
			if ( p.result.BLEFT.lbt.strokes.Count != 0 ) {
                if ( c == null ) {
                    currentSymbol.parent.children.Add( p.result.BLEFT );
				    unusedStrokes -= p.result.BLEFT.lbt.strokes.Count;                   
                } else {
                    c.BLEFT = p.result.BLEFT;
                }
            }
			if ( p.result.TLEFT.lbt.strokes.Count != 0 ) {
                if ( c == null ) {
                    currentSymbol.parent.children.Add( p.result.TLEFT );
				    unusedStrokes -= p.result.TLEFT.lbt.strokes.Count;
                } else {
                    c.TLEFT = p.result.TLEFT;
                }
			}

			// update LBT; should this be done elsewhere???
			if ( c != null ) {
				foreach ( Stroke s in c.segment.strokes ) p.lbt.PruneNode( s, LBT.DefaultAdjacentCriterion );
				c.lbt = p.lbt;
			//} else {
				//currentSymbol.symbol.lbt = p.lbt;
			}
			
			//unusedStrokes = p.lbt.strokes.Count; // ????????
			
			return p;
		}
		
		public void applyRules( List< ParseTreeNode > arg_nlist ) {
#if DEBUG
			Console.WriteLine( "[applyRules] entered." );
			treeRoot.ShowTree( 4, null );
			Console.WriteLine("Min Required Strokes: " + minRequiredStrokes);
			Console.WriteLine("Call: " + apply_rule_counter);
#else
			//Console.Write(apply_rule_counter);
			//Console.Write('\r');
#endif
			// increment counter
			apply_rule_counter++;
 			if ( arg_nlist.Count == 0 ) return;
			if ( unusedStrokes < 1 && arg_nlist[ 0 ] != ParseTreeNode.EndOfBaseline && !( arg_nlist[ 0 ] is RelationTreeNode ) ) return;			
			
			// if ( unusedStrokes > 0 && !( arg_nlist[ 0 ].GetType().Equals( typeof( ParseTreeNode ) ) ) ) return;
			
			List< ParseTreeNode > nlist = new List< ParseTreeNode >( arg_nlist );
			ParseTreeNode n = nlist[ 0 ]; 
			nlist.RemoveAt( 0 );
			
	 		if ( n == ParseTreeNode.EndOfBaseline ) 
			{
				
				// END-OF-BASELINE CASE
				
				
				PartitionResultWrapper pr = attachSymbol( currentSymbol.symbol.lbt, null );
				
				// continue only if valid partition made
				if ( pr != null ) {
					
					if ( nlist.Count == 0 && unusedStrokes == 0 && unusedInputStrokes == 0 ) {
#if DEBUG
						Console.WriteLine( "***ACCEPT***" );
#endif
						acceptCurrentParseTree();
					}

					int nodes_added = nlist.Count;

					if ( pr.result != null && pr.result.ABOVE.lbt.strokes.Count != 0 ) nlist.Add( pr.result.ABOVE );
					if ( pr.result != null && pr.result.BELOW.lbt.strokes.Count != 0 ) nlist.Add( pr.result.BELOW );
					if ( pr.result != null && pr.result.CONTAINS.lbt.strokes.Count != 0 ) nlist.Add( pr.result.CONTAINS );
					if ( pr.result != null && pr.result.SUBSC.lbt.strokes.Count != 0 ) nlist.Add( pr.result.SUBSC );
					if ( pr.result != null && pr.result.SUPER.lbt.strokes.Count != 0 ) nlist.Add( pr.result.SUPER );
					
					// add BLEFT/TLEFT
					//if ( pr.result != null && pr.result.BLEFT.lbt.strokes.Count != 0 ) nlist.Add( pr.result.BLEFT );
					//if ( pr.result != null && pr.result.TLEFT.lbt.strokes.Count != 0 ) nlist.Add( pr.result.TLEFT );

					nodes_added = nlist.Count - nodes_added;
					minRequiredStrokes += nodes_added;

					applyRules( nlist );
				} 
				else 
				{
#if DEBUG
					Console.WriteLine( "**BACKTRACK: end-of-baseline, invalid partition" );
#endif
				}
				
			}
			else if ( n is RelationTreeNode ) 
			{
				
				// handle relation nodes by adding new parse tree node
				RelationTreeNode rtn = n as RelationTreeNode;
				ParseTreeNode ptn = new ParseTreeNode();
				ptn.strokes = rtn.strokes;
				ptn.nodeType = rtn.nodeType;
				// increment here for new production
				ptn.lbt = rtn.lbt;
				rtn.children.Clear(); // remove all before
				rtn.children.Add( ptn );
				parse( ptn.lbt, ptn, nlist );
				
			}
			else if ( n.nodeType.StartsWith( "*" ) ) 
			{ // if n generates terminal symbols
				
				List< LexerResult > C = SelectCandidateSymbols( n.nodeType );
				//candidateSymbols = C;
				foreach ( LexerResult c in C ) 
				{
					PartitionResultWrapper pr = attachSymbol( currentSymbol == null ? initLBT : currentSymbol.symbol.lbt, c );
 					if ( pr != null ) {
						pushCurrentState();
						
						// update current state
						currentSymbol = new PreviousSymbol( c, n, null, null, true );						
						unusedStrokes -= currentSymbol.symbol.segment.strokes.Count;
						unusedInputStrokes -= currentSymbol.symbol.segment.strokes.Count;						
						// remove one of the min required strokes for the current token
						minRequiredStrokes -= 1;

						// prune by  number of strokes left
						if (unusedInputStrokes < minRequiredStrokes)
						{
							popCurrentState();
							return; // continue;
						}

						List< string > layoutClasses =	grammar.GetLayoutClassesFromTerminal( c.segment.classification[ 0 ].symbol );
						
						if ( layoutClasses.Count == 0 ) continue;
						
						candidateSymbols = new List< LexerResult >();
						foreach ( string layoutClass in layoutClasses ) 
						{
							List< LexerResult > res = lexer.Next( c.lbt, c.segment, layoutClass, MAX_NEIGHBORS );
							foreach ( LexerResult r in res ) if ( !candidateSymbols.Contains( r ) ) candidateSymbols.Add( r );
						}


						if ( candidateSymbols.Count == 0 && !nlist.Contains( ParseTreeNode.EndOfBaseline ) )
								nlist.Insert( 0, ParseTreeNode.EndOfBaseline );
						// not end of baseline, so prune and backtrack if necessary
						else if (nlist.Count > 0 && (nlist[0] is RelationTreeNode == false))
						{
							// prune candidates based on the current node type
							foreach (LexerResult lr in candidateSymbols)
							{
								for (int k = 0; k < lr.segment.classification.Count; k++)
								{
									if (grammar.NonTerminalCanGenerateTerminal(nlist[0].nodeType, lr.segment.classification[k].symbol) == false)
										lr.segment.classification.RemoveAt(k--);
								}
							}
							// remove candidate symbols which contain no symbol alternatives
							for (int k = 0; k < candidateSymbols.Count; k++)
							{
								if (candidateSymbols[k].segment.classification.Count == 0)
									candidateSymbols.RemoveAt(k--);
							}

							// no valid symbols given the grammar, so bbreak out early
							if (candidateSymbols.Count == 0)
							{
								popCurrentState();
								return; // continue;
							}
						}

						SymbolTreeNode nc = new SymbolTreeNode( c );
						n.children.Add( nc );
						
						/*
						if ( nlist.Count == 0 && unusedStrokes == 0 ) {
							if ( unusedInputStrokes == 0 ) acceptCurrentParseTree();
						} else {
						*/	
							// append relation nodes to the END
							
							List< RelationTreeNode > nlist_rels = new List< RelationTreeNode >();
							if ( pr.result != null && pr.result.ABOVE.lbt.strokes.Count != 0 ) nlist_rels.Add( pr.result.ABOVE );
							if ( pr.result != null && pr.result.BELOW.lbt.strokes.Count != 0 ) nlist_rels.Add( pr.result.BELOW );
							if ( pr.result != null && pr.result.CONTAINS.lbt.strokes.Count != 0 ) nlist_rels.Add( pr.result.CONTAINS );
							if ( pr.result != null && pr.result.SUBSC.lbt.strokes.Count != 0 ) nlist_rels.Add( pr.result.SUBSC );
							if ( pr.result != null && pr.result.SUPER.lbt.strokes.Count != 0 ) nlist_rels.Add( pr.result.SUPER );
							//if ( pr.result != null && pr.result.BLEFT.lbt.strokes.Count != 0 ) nlist_rels.Add( pr.result.BLEFT );
							//if ( pr.result != null && pr.result.TLEFT.lbt.strokes.Count != 0 ) nlist_rels.Add( pr.result.TLEFT );
							foreach ( RelationTreeNode rtn in nlist_rels ) nlist.Add( rtn );

							minRequiredStrokes += nlist_rels.Count;

							applyRules( nlist );
						//}
						
						if ( nlist.Count > 0 && nlist[ 0 ] == ParseTreeNode.EndOfBaseline ) nlist.RemoveAt( 0 ); // !
							
						// remove any leftover relation nodes
						n.children.Remove( nc );
						List< ParseTreeNode > n_children_tmp = new List< ParseTreeNode >( n.children );
						for ( int i = 0; i < n_children_tmp.Count; i++ ) if ( n_children_tmp[ i ] is RelationTreeNode ) n.children.Remove( n_children_tmp[ i ] );
						
						foreach ( RelationTreeNode rtn in nlist_rels ) nlist.Remove( rtn );
						popCurrentState();
					}
				}
			}
			else
			{
				// NONTERMINALS
				//n.lexResult.segment.classification[0].symbol;
				List< string[] > productions = grammar.GetProductions( n.nodeType );
				if ( productions == null ) {
#if DEBUG
					Console.Error.WriteLine( "Error: invalid nonterminal ({0}).", n.nodeType );
#endif
					return;
				}

				// remove one for the token we are replacing with productions
				minRequiredStrokes--;

				foreach ( string[] production in productions ) 
				{
					
					minRequiredStrokes += production.Length;

					// prune by  number of strokes left
					if (unusedInputStrokes < minRequiredStrokes)
					{
						minRequiredStrokes -= production.Length;
						continue;
					}

					// prune based on the candidate symbols and the first rule in production
					bool candidate_can_be_generated = false;
					foreach (LexerResult lr in candidateSymbols)
					{
						foreach(Classification csf in lr.segment.classification)
						{
							if (grammar.NonTerminalCanGenerateTerminal(production[0], csf.symbol))
							{
								candidate_can_be_generated = true;
								break;
							}
						}
						if (candidate_can_be_generated)
							break;
					}

					if (!candidate_can_be_generated)
					{
						minRequiredStrokes -= production.Length;
						continue;
					}
					

					List< ParseTreeNode > nodes = new List< ParseTreeNode >();
					foreach ( string p in production ) 
					{
						ParseTreeNode n0 = new ParseTreeNode();
						n0.nodeType = p;
						n0.lexResult = null;
						nodes.Add( n0 );
						n.children.Add( n0 );
					}
					
					for ( int i = nodes.Count - 1; i >= 0; i-- )
						nlist.Insert( 0, nodes[ i ] );
					applyRules( nlist );

					// restore min required strokes
					minRequiredStrokes -= production.Length;

					foreach ( ParseTreeNode node in nodes ) 
					{
						n.children.Remove( node );
						nlist.Remove( node );
					}
				}
				minRequiredStrokes++;
			}
		}
		
		private void pushCurrentState() {
			parseStateStack.Push( new ParseState {
				candidateSymbols = new List< LexerResult >( candidateSymbols ),
				currentSymbol = currentSymbol,
				unusedInputStrokes = unusedInputStrokes,
				unusedStrokes = unusedStrokes,
				minRequiredStrokes = minRequiredStrokes
			} );
		}
		
		private void popCurrentState() {
			ParseState ps = parseStateStack.Pop() as ParseState;
			candidateSymbols = ps.candidateSymbols;
			currentSymbol = ps.currentSymbol;
			unusedInputStrokes = ps.unusedInputStrokes;
			unusedStrokes = ps.unusedStrokes;
			minRequiredStrokes = ps.minRequiredStrokes;
		}
		
		private void acceptCurrentParseTree() {
			validParses.Add( new ValidParseTree( treeRoot ) );
			/*
			Console.WriteLine( "*** Valid Parse Tree Found ***" );
			treeRoot.ShowTree( 4, null );
			Console.WriteLine( "***" );
			 */
		}
		
		// END ADDED JULY 7, 12
		
		public bool legalPartition( PartitionResultWrapper presult, PreviousSymbol p ) {
			
			if ( p == null ) return true; // ???
			
			// Compile non-empty regions.
			List<string> nonEmptyRegions = new List<string>();
			if (presult.result.ABOVE != null && //presult.result.ABOVE.strokes != null &&
			    presult.result.ABOVE.strokes.Count > 0) 
				nonEmptyRegions.Add("ABOVE");
			if (presult.result.BELOW != null &&// presult.result.BELOW.strokes != null &&
			    presult.result.BELOW.strokes.Count > 0) 
				nonEmptyRegions.Add("BELOW");
			if (presult.result.SUPER != null && //presult.result.SUPER.strokes != null &&
			    presult.result.SUPER.strokes.Count > 0) 
				nonEmptyRegions.Add("SUPER");
			if (presult.result.SUBSC != null && //presult.result.SUBSC.strokes != null &&
			    presult.result.SUBSC.strokes.Count > 0)
				nonEmptyRegions.Add("SUBSC");
			if (presult.result.CONTAINS != null &&// presult.result.CONTAINS.strokes != null &&
			    presult.result.CONTAINS.strokes.Count > 0) 
				nonEmptyRegions.Add("CONTAINS");

			// account for regions currently assigned
			foreach ( string reg in p.regions ) if ( !nonEmptyRegions.Contains( reg ) ) nonEmptyRegions.Add( reg );
			
			// TODO: LAYOUT CLASS?
#if DEBUG
			Console.WriteLine( "[legalPartition] p.parent.nodeType = <{0}>", p.parent.nodeType );
#endif
			
			// get layout (regions) for symbol as specified by the grammar
			// FIRST, try looking up parent NT directly as the class
			// IF THAT FAILS, look up the class(es) based on the symbol
			
			List< string[] > grammarLayout;			
			try {
				grammarLayout = grammar.GetRegionLayouts( p.parent.nodeType );
			} catch ( Exception ) {
				try {
					grammarLayout = grammar.GetRegionLayouts( p.symbol.segment.classification[ 0 ].symbol );	
				} catch ( Exception ) {
					return false; // no, just no	
				}
			}
			
			foreach ( string[] regions in grammarLayout ) {
				int matchedRegions = 0;
				bool satisfies = true;
				for ( int i = 0; i < regions.Length; i += 2 ) {
					bool optional = regions[ i ][ 0 ] == '@'; // ow, assume required
					string region = regions[ i ].Substring( 1 );
					// string nt = regions[ i + 1 ];
					if ( nonEmptyRegions.Contains( region ) ) {
						matchedRegions++;
					} else if ( !optional ) {
						satisfies = false;
					}
				}
				if ( satisfies && matchedRegions == nonEmptyRegions.Count ) {
					
					// add starting NTs to the regions
					for ( int i = 0; i < regions.Length; i += 2 ) {
						string region = regions[ i ].Substring( 1 );
						selectRegion( region, presult, regions[ i + 1 ] );
					}
					
					return true; // at least one satisfies
				}
			}
			
			return false;
		}
		

		// A method to produce a sorted list of alternate symbol recognition results.
		// Return LexerResult records, with classification decisions assigned to the classification field
		// of the corresponding segment. Makes use of layout classes as well.
		public List< LexerResult > SelectCandidateSymbols( string symbolType )
		{
			List< LexerResult > finalResult = new List< LexerResult >();
			
			HashSet< LexerResult > seen = new HashSet< LexerResult >();
			foreach ( LexerResult symbol in candidateSymbols ) {
				if ( seen.Contains( symbol ) ) continue;
				seen.Add( symbol );
				
				foreach ( Classification c in symbol.segment.classification ) {
					if ( grammar.NonTerminalMapsToTerminal( symbolType, c.symbol ) ) {
						LexerResult newResult = symbol.Clone() as LexerResult;
						newResult.segment.classification = new List< Classification >{ c };
						newResult.segment.classification[ 0 ].note = grammar.GetLayoutClassesFromTerminal( c.symbol )[ 0 ];
						
						// TODO: MAKE THIS BETTER
						bool equiv = false;
						foreach ( LexerResult lr in finalResult ) {
							if ( newResult.Equals( lr ) ) {
								equiv = true;
								continue;
							}
						}
						if ( !equiv ) finalResult.Add( newResult );
					}
				}
			}
			
			finalResult.Sort();
			if ( finalResult.Count - 1 >= TOP_N ) finalResult.RemoveRange( TOP_N, finalResult.Count - TOP_N );
			
#if DEBUG
				Console.WriteLine("---------------------");
				foreach(LexerResult fresult in finalResult) {
					string strokeLabelString = "";
					foreach (Stroke stroke in fresult.segment.strokes) strokeLabelString += stroke.ToString() + " ";
						Console.WriteLine("  Strokes: {1}\n  {0}", fresult, strokeLabelString);
				}
				Console.WriteLine("---------------------");
#endif
			
			return finalResult;
			
		}
		
		// **Will need to update for Part2.
		public static PartitionClass getPartitionClass(string s) 
		{
			PartitionClass pclass = PartitionClass.Regular;
			switch (s) {
			case "ROOT":
			case "*SQRT":
				pclass = PartitionClass.SquareRoot;
				break;
			case "NOSUPERSUB":
				pclass = PartitionClass.HorLine;
				break;
			default:
				pclass = PartitionClass.Regular;
				break;
			}
			return pclass;
		}
		
		public RelationTreeNode selectRegion(string regionName, PartitionResultWrapper partition,
		                                     string nonterminal)
		{
			// ASSUMPTION: strokes are already assigned to RelationTreeNodes.
			RelationTreeNode newRegion; 
			switch (regionName) {
			case "ABOVE":
				newRegion = partition.result.ABOVE;
				break;
			case "SUPER":
				newRegion = partition.result.SUPER;
				break;
			case "SUBSC":
				newRegion = partition.result.SUBSC;
				break;
			case "BELOW":
				newRegion = partition.result.BELOW;
				break;
			case "CONTAINS":
				newRegion = partition.result.CONTAINS;
				break;
			case "TLEFT":
				newRegion = partition.result.TLEFT;
				break;
			case "BLEFT":
				newRegion = partition.result.BLEFT;
				break;
			default:
				throw new 
					ArgumentException("CreateSymbolNodes:: invalid region type detected: "
					                  + regionName);
			}
			newRegion.nodeType = nonterminal;
			return newRegion;
		}
		
		// Convenience method: call lexer, filter for desired symbol type
		// attributes.
		public List<LexerResult> FindSymbols(bool start,
		                                             ParseState pstate,
		                                             List<string> layoutTypes,
		                                             int k)
	     {
			// DEBUG: always store LBT in previous symbol.
			LBT lbt = pstate.previous.symbol.lbt;
			
			// Acquire results for start or next symbol, as requested.
			// Check the symbol rec. table to avoid redundant requests.
			List<LexerResult> currentLex;
			//SymbolRecKey lookup = new SymbolRecKey(start, k, pstate);
			//accesses++;
			//if (!symbolRecTable.TryGetValue(lookup, out currentLex)) {
				if (start) {
					currentLex = lexer.Start(lbt, k);
				}
				else {
					// There should be a previous symbol in this case.
					currentLex = lexer.Next(lbt, 
				  	            	        pstate.previous.symbol.segment, 
				    	      	              pstate.previous.symbol.segment.classification[0].symbol, 
				     		                  k);
				}
			
			// Filter and return the results.
			// *Exploit (constrained) list of candidates for symbols that may start a baseline.
			if (start) {
				Console.WriteLine("SELECTING CANDIDATES FOR START",pstate.rootNode.nodeType);
				return SelectCandidateSymbols( "START" );
			}
			else {
				Console.WriteLine("SELECTING CANDIDATES FOR {0}",pstate.rootNode.nodeType);
				return SelectCandidateSymbols( pstate.rootNode.nodeType );
			}
		}
	

		
		public string validParseTreeString(int max)
		{
			if (max == -1)
				max = validParses.Count;

			max = Math.Min(max, validParses.Count);

			StringBuilder treeString = new StringBuilder();
			for (int i = 0; i < max; i++)
			{
				string temp = string.Format(":: Parse #{0} Score: {2:0.000000} ::\n{1}\n\n",i+1,
				    validParses[i].root.ToString(0),validParses[i].score);
				treeString.Append(temp);
			}
			
			return string.Format("\nValid Parse Count: {0}\n-------------------\n{1}",
			            validParses.Count, treeString);
		}
		
		// RZ: New constructor -> replaces the lexer.
		public ParserMain(  Grammar grammar, InkML inkml_file, int topn, int neighbors, string classify_url, double prob_threshold, string stats_file,
			string segmentFile, List<Stroke> strokeData ) : this(grammar, inkml_file, topn, neighbors, classify_url, prob_threshold, stats_file) 
		{
			lexer = new LexerMain( stats_file, classify_url, prob_threshold, segmentFile, strokeData );
		}
		
		public ParserMain( Grammar grammar, InkML inkml_file, int topn, int neighbors, string classify_url, double prob_threshold, string stats_file )
		{
			TOP_N = topn;
			MAX_NEIGHBORS = neighbors;
			
			// YUCK!! COPY AND PASTE BELOW.
			
			// Set symbol tables and input file. Instantiate the lexer. Create empty valid parse list.
			//initializeTables(symbolDefFile);
			lexer = new LexerMain( stats_file, classify_url, prob_threshold );
			validParses = new List<ValidParseTree>();

			this.grammar = grammar;
			layoutClasses = grammar.GetLayoutClasses();

			// Obtain LBT, stroke information.
			inputInkML = inkml_file;
			initLBT = new LBT( inputInkML, LBT.DefaultAdjacentCriterion );
			
			treeRoot = new ParseTreeNode();
			treeRoot.nodeType = "S";
			treeRoot.lbt = initLBT;
			
			// treeRoot.lexResult ???
			//strokeList = lbt.strokes; // unused?
			currentSymbol = null;
			minRequiredStrokes = 1;
			unusedInputStrokes = initLBT.strokes.Count;
			parseStateStack = new Stack();
			
			//Console.WriteLine(tree.ToDOT());
			
			// Create root node.
			//rootAll = new ParseTreeNode("S", tree);
		
			// HACK: note that the ParseState constructor will initialize the parser data as well.
			//ParseState initState = new ParseState(rootAll, tree); //, 5); // MAGIC
			
			accesses = 0;
			hits = 0;
				
			// Invoke the parse
#if DEBUG
			//Console.WriteLine("Starting parse...");
#endif
			//Parse(initState, true, MAX_NEIGHBORS); 
			//validParses.Sort();
		}
	}		          				                                                                        
}		                                                                        
				                                                                       
