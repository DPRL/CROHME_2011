using DataStructures;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace Lexer {
	
	public enum PartitionClass {
		Regular,
		HorLine,
		SquareRoot,
	}

	public class LexerMain {
		public Dictionary<string,Segment> strokeSegmentDictionary; // **RZ: New stroke -> segment lookup.
		
		private ClassifyManager classifier; // = new ClassifyManager();

		private const int Y_WRITING_LINE_THRESHOLD = 10; // default threshold for y-writing-line algorithm
		
		//public static Dictionary< string, SymbolClass > layoutClassMap;
		
		public Dictionary< string, QDC > adjProbClassifiers;
		
		// ****RZ: adding new constructor, to make use of segmentation pre-process.
		// Note: chaining with pre-existing constructor.
		public LexerMain(string adjDataFile, string classifyUrl, double probThreshold, string segmentFile,
				List<Stroke> strokeList) : this(adjDataFile, classifyUrl, probThreshold)
		{
			strokeSegmentDictionary = new Dictionary<string, Segment>();

			// Read in segmentation data, print to standard output.
			if (!File.Exists(segmentFile)) throw new Exception("Segment data file does not exist: " + segmentFile);
			StreamReader fin = new StreamReader( segmentFile );
			//List <Segment> segments = new List<Segment>();
			string line = null;
			while ( (line = fin.ReadLine()) != null ) {
				try {

					Segment s = new Segment();
					string[] line_data = line.Split( new char[] { ',' });
					foreach (string sid in line_data) {

						// Convert string to integer index; add corresponding stroke to segment.
						int strokeIndex = Convert.ToInt32(sid);
						s.strokes.Add( strokeList[strokeIndex] );

						// Add (stroke (sid)/segment) pair to dictionary (hash table).
						strokeSegmentDictionary.Add( sid, s);
					}
				} catch ( Exception ) { 
					Console.Error.WriteLine("ERROR: reading segmentation data.");
				}
			}

			// Sanity check.
#if DEBUG
			Console.WriteLine("Segment dictionary:");
			foreach (KeyValuePair<string, Segment> pair in strokeSegmentDictionary) {
				Console.WriteLine("{0}: {1}",pair.Key, pair.Value);
				foreach (Stroke s in pair.Value.strokes) 
					Console.WriteLine("  {0}", s.stroke_id);
			}
			Console.WriteLine("done.");
#endif
		}

		public LexerMain( string adjDataFile, string classifyUrl, double probThreshold )
		{
            classifier = new ClassifyManager(classifyUrl, probThreshold);

			//layoutClassMap = inMap;
			
			adjProbClassifiers = new Dictionary< string, QDC >();
			
			// initialize QDCs
			if ( !File.Exists( adjDataFile ) ) 
				throw new Exception( "Adjacency data file does not exist: " + adjDataFile );
			
			StreamReader fin = new StreamReader( adjDataFile );
			fin.ReadLine();
			string line = null;
			
			List< Matrix > c_adj = new List< Matrix >();
			List< Matrix > c_oth = new List< Matrix >();
			
			List<Matrix> c_sup = new List<Matrix>();
			List<Matrix> c_sub = new List<Matrix>();
			
			List< Matrix > a_adj = new List< Matrix >();
			List< Matrix > a_oth = new List< Matrix >();
			
			List<Matrix> a_sup = new List<Matrix>();
			List<Matrix> a_sub = new List<Matrix>();			
			
			List< Matrix > c_adj_dash = new List<Matrix>();
			List< Matrix > c_adj_oth = new List<Matrix>();
			List< Matrix > a_adj_dash = new List<Matrix>();
			List< Matrix > a_adj_oth = new List<Matrix>();
			
			while ( ( line = fin.ReadLine() ) != null ) 
			{
				try 
				{
					string[] line_data = line.Split( new char[] { ',' } );
					string rel = line_data[ 1 ].Trim();
					string ccl = line_data[ 0 ].Trim();
					string pcl = line_data[ 4 ].Trim();
					double top = double.Parse( line_data[ 5 ].Trim() );
					double bot = double.Parse( line_data[ 6 ].Trim() );
					double[,] vec = new double[,] { { top }, { bot } };
					if ( pcl.Equals( "CENTERED" ) ) 
					{
						switch(rel)
						{
						case "Right":
							
							if ( ccl.Equals( "-" ) ) {
								c_adj_dash.Add( new Matrix( vec ) );
							} else {
								c_adj_oth.Add( new Matrix( vec ) );	
							}
							
							c_adj.Add( new Matrix( vec ) );
							break;
						case "SubScript":
							c_sub.Add( new Matrix(vec));
							goto case "Other";
						case "SuperScript":
							c_sup.Add( new Matrix(vec));
							goto case "Other";
						case "Other":
							c_oth.Add( new Matrix(vec));
							break;
						}

					} 
					else if ( pcl.Equals( "ASCENDER" ) )
					{
						switch(rel)
						{
						case "Right":
							
							if ( ccl.Equals( "-" ) ) {
								a_adj_dash.Add( new Matrix( vec ) );
							} else {
								a_adj_oth.Add( new Matrix( vec ) );	
							}
							
							a_adj.Add( new Matrix( vec ) );
							break;
						case "SubScript":
							a_sub.Add( new Matrix(vec));
							goto case "Other";
						case "SuperScript":
							a_sup.Add( new Matrix(vec));
							goto case "Other";
						case "Other":
							a_oth.Add( new Matrix(vec));
							break;
						}
					}
				} catch ( Exception ) { continue; }
			}
			fin.Close();

						
			adjProbClassifiers.Add( "Ascender", new QDC( new List< Matrix >[] { a_adj_dash, a_adj_oth, a_sup, a_sub }, new string[] { "adjacent-dash", "adj-non-dash", "super", "sub" } ) );
 			adjProbClassifiers.Add( "Centered", new QDC( new List< Matrix >[] { c_adj_dash, c_adj_oth, c_sup, c_sub }, new string[] { "adjacent-dash", "adj-non-dash", "super", "sub" } ) );
			
		}
		
		// NOTE: this assumes that all segments must contain the first stroke in the passed list.
		public List<Segment> CreateSegmentsForStrokes( LBT lbt, List<Stroke> strokes_to_segment )
		{
			// Return empty list for an empty set of strokes.
			if (strokes_to_segment.Count < 1)
				return new List<Segment>();
						
			// Construct a list of candidate segments.
			List< Segment > candidateSegments = new List<Segment>();

            // sort in draw-order
            strokes_to_segment.Sort( ( s1, s2 ) => int.Parse( s1.stroke_id ) - 
					int.Parse( s2.stroke_id ) );

            Segment curSegment = new Segment();
            foreach ( Stroke s in strokes_to_segment ) {

                // create new segment and add all previous strokes, followed by next one
                Segment newSegment = new Segment();
                foreach ( Stroke _s in curSegment.strokes ) newSegment.strokes.Add( _s );
                newSegment.strokes.Add( s );

                // make sure we don't violate constraint that two overlapping strokes must go together
                bool overlapViolation = false;
                foreach ( Stroke str in newSegment.strokes ) {

                    // if this stroke touches another and the other is not in the segment, don't consider
                    foreach ( Stroke _s in lbt.strokes ) {
                        if ( Stroke.Overlap( str, _s ) && !newSegment.strokes.Contains( _s ) ) {
                            overlapViolation = true;
                            break;
                        }
                    }
                    if ( overlapViolation ) break;
                }

                if ( !overlapViolation ) candidateSegments.Add( newSegment );
                curSegment = newSegment; // keep track of current strokes in list
            }

			
			return candidateSegments;
		}
		
		// Find leftmost symbol for a sub-expression.
		// **No special handling for different symbol types; let parser try some n-best alternatives.
		public List< LexerResult > Start( LBT tree, int k ) 
		{
			// Note: will require modification to accomodate indexed symbols (summations, etc.)
			List< LexerResult > all_results = new List< LexerResult >();
		
			foreach ( LBT.LBTNode node in tree.root.children ) 
			{
				//Console.WriteLine("START Considering: {0}", node.stroke);
				Stroke snc = node.stroke;
				
								
				// **REMOVING**
				//List< Stroke > strokes_to_segment = new List< Stroke >();
				//strokes_to_segment.Add( snc );
				//foreach ( Stroke s in tree.KNearestNeighbors( snc, k, LBT.DefaultKNNFilter ) ) 
				//	strokes_to_segment.Add( s );
				
				// Construct a list of candidate segments, and classify each.
				//List< Segment > candidateSegments = CreateSegmentsForStrokes( tree, strokes_to_segment );
				List<Segment> candidateSegments = new List<Segment>();
				candidateSegments.Add( strokeSegmentDictionary[snc.stroke_id] );
				foreach (Segment seg in candidateSegments) 
				{
					LexerResult result = new LexerResult();
					classifier.classify( seg );
					//Console.WriteLine(seg); // DEBUG
					seg.PopulateBB();
					if ( seg.classification == null ) {
						Console.Error.WriteLine("LexerResult.Start:: NO CLASSIFICATION RESULT");
						continue;
					}
					
					result.segment = seg;
					result.lbt = UpdateLBT( seg, tree );
					all_results.Add( result );
				}
			}
			
			return all_results;
		}
		
		public List< LexerResult > Next( LBT lbt, Segment S_l, string layout_class, int k ) 
		{
			// **Converts descenders to centered symbols (writing-line "hack")
			if ( S_l != null && ( S_l.classification[ 0 ].symbol == "y" 
						|| S_l.classification[ 0 ].symbol == "\\tan"  // RZ - BUG?
						|| S_l.classification[ 0 ].symbol == "\\log" ) ) {
				layout_class = "Centered";
				int[] new_lines = GetCenteredLines( S_l ); // manipulate bb-bottom
                		S_l.bb.Top = new_lines[ 0 ];
              		S_l.bb.Bottom = new_lines[ 1 ];
			}
			
			List< LexerResult > all_results = new List< LexerResult >();
			
			
#if DEBUG
			//Console.WriteLine("HOR ADJ, PREVIOUS SYMBOL: {0} ({1})",C_l,lc);
#endif
			switch (layout_class.ToUpper())
			{	
				case "ASCENDER":
				case "CENTERED":
				case "ROOT":  // WHICH
				case "*SQRT": // ONE ??
					// **REMOVING
					// List< Stroke > strokes_to_segment = new List< Stroke >();
					string n = LeftmostAdjacent( lbt, S_l, layout_class ); // DEBUG: use proper class.
#if DEBUG
					//Console.WriteLine(n == null ? "NOTHING FOUND" : string.Format("  Found stroke: {0}",n));
#endif
					if (n != null) {						
						// **REMOVING
						//strokes_to_segment.Add( lbt.strokes.Where( s => s.stroke_id.Equals( n ) ).First() );
						//List< Stroke > N = lbt.KNearestNeighbors( lbt.strokes.Where( s => s.stroke_id.Equals( n ) ).First(), k, LBT.DefaultKNNFilter );
						//foreach ( Stroke s in N ) strokes_to_segment.Add( s );
					
						//foreach ( Segment seg in CreateSegmentsForStrokes( lbt, strokes_to_segment ) ) {
						List<Segment> candidateSegments = new List<Segment>();
						candidateSegments.Add( strokeSegmentDictionary[n] );
						foreach ( Segment seg in candidateSegments ) {
							LexerResult result = new LexerResult();
							classifier.classify( seg );
							if ( seg.classification == null ) continue;
							result.segment = seg;
							result.lbt = UpdateLBT( seg, lbt );
							result.adj = HorAdj(S_l, seg, layout_class);
							all_results.Add( result );
						}
						return all_results;
					
					} else {
						return new List<LexerResult>();
					}
				
				case "NOSUPERSUB":
				case "*FRAC": // ?????
					// Start(), but remove symbols to the left
					LBT lbt1 = new LBT( lbt.strokes, LBT.DefaultAdjacentCriterion );
					if ( lbt.strokes != null ) {
						foreach ( Stroke s in lbt.strokes ) {
							if ( s.aabb.Left < S_l.bb.Right ) lbt1.PruneNode( s, LBT.DefaultAdjacentCriterion );
						}
					}
#if DEBUG
					Console.WriteLine( "[next] LAYOUT CLASS: {0}; calling Start()", layout_class.ToUpper() );
#endif
					return Start( lbt1, k );
				
				default:
					return Start( lbt, k );
			}
		}
		
		/// <summary>
		/// Finds the left-most adjacent symbol in the LBT, given the current symbol and its class.
		/// </summary>
		/// <param name="S_l">
		/// The current symbol.
		/// </param>
		/// <param name="lbt">
		/// The LBT for the remainder of the strokes.
		/// </param>
		/// <param name="lc">
		/// The current segment's layout class.
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string LeftmostAdjacent( LBT lbt, Segment S_l, string layout_class ) {
			foreach ( Stroke s in lbt.strokes.OrderBy( s => s.aabb.Left ) ) {
				Segment seg = new Segment(new List<Stroke>() { s }); // { strokes = new List< Stroke >() { s } };
				//Console.WriteLine("Top: {0}",seg.bb.Top);

				if (HorAdj(S_l, seg, layout_class))
				    return s.stroke_id;
			}
			return null; // DEBUG
		}
		
		/// <summary>
		/// Probability of horizontal adjacency.
		/// </summary>
		/// <param name="S_l">
		/// Current segment.
		/// </param>
		/// <param name="s">
		/// Next segment.
		/// </param>
		/// <param name="lc">
		/// Layout class of current segment.
		/// </param>
		public bool HorAdj( Segment S_l, Segment s, string layout_class ) {
			
			// ???? BUG ???  RZ: I do not understand why the BB goes out of scope if not created here.
			s.PopulateBB();
			
			float norm_top = ( s.bb.Top - S_l.bb.Top ) / S_l.bb.Height;
			float norm_bottom = ( s.bb.Bottom - S_l.bb.Top ) / S_l.bb.Height;

			return isAdjacentStroke(norm_top, norm_bottom, layout_class);
		}
		
		public bool isAdjacentStroke( float norm_top, float norm_bottom, string layout_class ) 
		{
			QDC classifier;
			if (adjProbClassifiers.TryGetValue(layout_class, out classifier))
			{
				int index = classifier.classify(new Matrix(new double[,] { { norm_top }, { norm_bottom } }));
				return index == 0 || index == 1;
			}
			return false;
			
		}
		
		private bool VerticalOverlap( float[] r_i, float[] r_k ) {
			return !( r_i[ 0 ] > r_k[ 1 ] || r_k[ 0 ] > r_i[ 1 ] );
		}
		
        public int[] GetCenteredLines( Segment s ) {

            // projection frequency
            Dictionary<int, int> yc = new Dictionary<int, int>();
            int ymin = int.MaxValue;

            foreach ( Stroke _s in s.strokes ) {
                for ( int i = 1; i < _s.points.Count; i++ ) {
                    int yprev = ( int ) _s.points[ i - 1 ].y;
                    int ycur = ( int ) _s.points[ i ].y;

                    if ( yprev < ymin ) ymin = yprev;
                    if ( ycur < ymin ) ymin = ycur; // this one might not be necessary

                    for ( int j = yprev; j <= ycur; j++ ) {
                        if ( !yc.ContainsKey( j ) ) {
                            yc.Add( j, 1 );
                        } else {
                            yc[ j ] = yc[ j ] + 1;
                        }
                    }
                }
            }

            // difference
            Dictionary< int, int > diff = new Dictionary< int, int >();
            float prevVal = -1;
            for ( int i = ( int ) Math.Floor( s.bb.Top ); i < ( int ) Math.Ceiling( s.bb.Bottom ); i++ ) {
                if ( !yc.ContainsKey( i ) ) continue;

                if ( prevVal == -1 ) {
                    diff.Add( i, 0 );
                    prevVal = yc[ i ];
                } else {
                    diff.Add( i, ( int ) Math.Abs( yc[ i ] - prevVal ) );
                }
            }

            // find top three: a, b, c
            int a = 0, b = 0, c = 0, a_y = 0, b_y = 0, c_y = 0;

            for ( int i = ( int ) Math.Floor( s.bb.Top ); i < ( int ) Math.Ceiling( s.bb.Bottom ); i++ ) {
                if ( !diff.ContainsKey( i ) ) continue;
                if ( diff[ i ] > a ) {
                    a = diff[ i ];
                    a_y = i;
                }
            }
            diff[ a_y ] = -1;

            for ( int i = ( int ) Math.Floor( s.bb.Top ); i < s.bb.Bottom; i++ ) {
                if ( !diff.ContainsKey( i ) ) continue;
                if ( diff[ i ] > b ) {
                    b = diff[ i ];
                    b_y = i;
                }
            }
            diff[ b_y ] = -1;
            
            for ( int i = ( int ) Math.Floor( s.bb.Top ); i < s.bb.Bottom; i++ ) {
                if ( !diff.ContainsKey( i ) ) continue;
                if ( diff[ i ] > c ) {
                    c = diff[ i ];
                    c_y = i;
                }
            }
            diff[ c_y ] = -1;

            int y_1, y_2; // the new lines

            if ( a > b ) {
                y_1 = a_y;
                if ( b > c ) {
                    y_2 = b_y;
                } else {
                    // use closest to a
                    if ( Math.Abs( b - a ) > Math.Abs( c - a ) ) {
                        y_2 = b_y;
                    } else {
                        y_2 = c_y;
                    }
                }
            } else {
                if ( b > c ) {
                    // using a and b
                    y_1 = a_y;
                    y_2 = b_y;
                } else {
                    // use closest pair
                    int d_ab = Math.Abs( a - b );
                    int d_ac = Math.Abs( a - c );
                    int d_bc = Math.Abs( b - c );
                    if ( d_ab < d_ac ) {
                        if ( d_ab < d_bc ) {
                            y_1 = a_y;
                            y_2 = b_y;
                        } else {
                            y_1 = b_y;
                            y_2 = c_y;
                        }
                    } else {
                        y_1 = a_y;
                        y_2 = c_y;
                    }
                }
            }

            if ( y_1 < y_2 ) {
                return new int[] { y_1, y_2 };
            }
            return new int[] { y_2, y_1 };
        }

        public float GetYWritingLine( Segment s, int? threshold ) {
			if ( threshold == null ) threshold = Y_WRITING_LINE_THRESHOLD;
			
			Dictionary< int, int > yc = new Dictionary< int, int >();
			int ymin = int.MaxValue;
			
			foreach ( Stroke _s in s.strokes ) { 
				for ( int i = 1; i < _s.points.Count; i++ ) {
					int yprev = ( int ) _s.points[ i - 1 ].y;
					int ycur = ( int ) _s.points[ i ].y;
					
					if ( yprev < ymin ) ymin = yprev;
					if ( ycur < ymin ) ymin = ycur; // this one might not be necessary
					
					for ( int j = yprev; j <= ycur; j++ ) {
						if ( !yc.ContainsKey( j ) ) {
							yc.Add( j, 1 );
						} else {
							yc[ j ] = yc[ j ] + 1;
						}
					}
				}
			}
			
			foreach ( int k in yc.Keys ) {
				if ( yc[ k ] == 1 && k > ( ymin + threshold ) ) return k;
			}
			
			// on failure: don't do anything 
			return s.bb.Bottom;
			
		}

        public float MaximumGapLocation( List< Stroke > strokes, Segment S_l, Segment S_r ) {
            bool all_overlap = true;
            List< AABB > total_x_axis_coordinates = new List< AABB >();
            total_x_axis_coordinates.Add( S_l.bb );
            foreach ( Stroke s in strokes ) total_x_axis_coordinates.Add( s.aabb );
            total_x_axis_coordinates.Add( S_r.bb );

            List< float > gap = new List< float >();
            for ( int i = 0; i < strokes.Count + 1; i++ ) gap.Add( 0 );

            float mgl = 0;
            for ( int i = 0; i < strokes.Count; i++ ) {
                gap[ i ] = total_x_axis_coordinates[ i + 2 ].Left - total_x_axis_coordinates[ i + 1 ].Right;
                if ( gap[ i ] > 0 ) all_overlap = false;
            }
            if ( !all_overlap ) {
                // find max gap and max gap index
                int max_gap_index = -1;
                float max_gap = -1;
                for ( int i = 0; i < gap.Count; i++ ) {
                    if ( gap[ i ] > max_gap ) {
                        max_gap_index = i;
                        max_gap = gap[ i ];
                    }
                }

                mgl = ( total_x_axis_coordinates[ max_gap_index + 1 ].Right + total_x_axis_coordinates[ max_gap_index + 2 ].Left ) / 2;
            }

            return mgl;
        }

        public PartitionResultWrapper Partition( LBT lbt, Segment S_l, Segment S_r, PartitionClass P_l, PartitionClass P_r, RelationTreeNode TLEFT, RelationTreeNode BLEFT ) {
			PartitionResult R = new PartitionResult();
			if ( TLEFT != null ) {
                foreach ( Stroke s in TLEFT.strokes ) R.TLEFT.strokes.Add( s );
            }
            if ( BLEFT != null ) {
                foreach ( Stroke s in BLEFT.strokes ) R.BLEFT.strokes.Add( s );
            }

			// Handle no right/left symbol.
			if (S_l == null && S_r == null) {
				throw new ArgumentException("Partition:: both passed segments are null.");
			}
			
			// y-writing-line hack
			if ( S_l != null && ( S_l.classification[ 0 ].symbol == "y" || S_l.classification[ 0 ].symbol == "\\log" || S_l.classification[ 0 ].symbol == "\\tan" ) ) {
				P_l = PartitionClass.Regular; // if not already?
                int[] lines = GetCenteredLines( S_l );
                S_l.bb.Top = lines[ 0 ];
				S_l.bb.Bottom = lines[ 1 ];
			}
			
			// Case for no right symbol (and default; symbols at left of current symbol).
			//List< Stroke > SP = S_r == null ? lbt.strokes : lbt.strokes.Where( _s => { if ( _s.aabb.Left < S_r.bb.Left ) { Console.WriteLine( "{0} < {1} (!)", _s.aabb.Left, S_r.bb.Left ); return true; } else { return false; } } ).ToList();
			List< Stroke > SP = new List< Stroke >();
			if ( S_r == null ) {
				SP = lbt.strokes;
			} else {
				// foreach ( Stroke _s in lbt.strokes ) {
				for ( int i = 0; i < lbt.strokes.Count; i++ ) {
					Stroke _s = lbt.strokes[ i ];
					if ( _s.aabb.Right /*Left?*/ < S_r.bb.Left ) {
						//Console.WriteLine( "{0} < {1} (!)", _s.aabb.Left, S_r.bb.Left );
						SP.Add( _s );
					}
				}
			}
			
			// Case for no left symbol; switch right to "left" symbol.

			bool noLeft = false;
			if (S_l == null ) {
				S_l = S_r;
				SP = lbt.strokes.Where( _s => _s.aabb.Right < S_r.bb.Left ).ToList();
				noLeft = true;
			}
			
			// HACK.
			bool indexed = S_l != null && ( S_l.classification[0].symbol == "\\sum" || S_l.classification[0].symbol == "\\int" ||
				S_l.classification[0].symbol == "\\lim" );
			
			foreach ( Stroke _s in SP ) {
				if (S_l != null && !noLeft) {
					bool horOverlap = !( S_l.bb.Right < _s.aabb.Left || _s.aabb.Right < S_l.bb.Left );
					bool fullContained = S_l.bb.Left < _s.aabb.Left && S_l.bb.Right > _s.aabb.Right && 
						S_l.bb.Top < _s.aabb.Top && S_l.bb.Bottom > _s.aabb.Bottom;
					
					float midRight = S_l.bb.Top + ( S_l.bb.Height / 2 );
					float dtop = midRight - _s.aabb.Top;
					float dbottom = _s.aabb.Bottom - midRight;
					
					//float topSpan = Math.Min( Math.Abs( S_l.bb.Top - _s.aabb.Bottom ), Math.Abs( midRight - _s.aabb.Top ) );
					//float bottomSpan = Math.Min( Math.Abs( S_l.bb.Bottom - _s.aabb.Top ), Math.Abs( midRight - _s.aabb.Bottom ) );
					
					// Be lenient here; invalid partitions will be pruned
					bool added = false;
					if ( P_l == PartitionClass.SquareRoot && fullContained ) {
						R.CONTAINS.strokes.Add( _s );
						added = true;
					} else if ( /*P_l == PartitionClass.HorLine &&*/ horOverlap ) {
                        /*
						if ( _s.aabb.Bottom <= S_l.bb.Top ) {
							R.ABOVE.strokes.Add( _s );
							added = true;
						} else if ( _s.aabb.Top > S_l.bb.Bottom ) {
							R.BELOW.strokes.Add( _s );
							added = true;
						}
                        */

                        // "less strict" constraints on top/bottom
                        if ( _s.aabb.Top < S_l.bb.Top && _s.aabb.Bottom <= S_l.bb.Bottom ) {
                            R.ABOVE.strokes.Add( _s );
                            added = true;
                        } else if ( _s.aabb.Bottom > S_l.bb.Bottom && _s.aabb.Top >= S_l.bb.Top ) {
                            R.BELOW.strokes.Add( _s );
                            added = true;
                        }

					} else if ( dtop > dbottom ) {
						R.SUPER.strokes.Add( _s );
						added = true;
					} else {
						R.SUBSC.strokes.Add( _s );
						added = true;
					}					
#if DEBUG
					if ( !added ) {
						Console.Error.WriteLine( "Stroke ({0}) did not get partitioned in {1}.", _s, S_l );
					}
#endif				
				} else {
					// HACK** There is nothing at left. Create TLEFT/BLEFT regions.
					double midPoint = (S_r.bb.Top + S_r.bb.Bottom) / 2.0;
					if ( _s.aabb.Bottom <= midPoint) {
						R.TLEFT.strokes.Add( _s );
					} else if  ( _s.aabb.Top > midPoint) {
						R.BLEFT.strokes.Add( _s );
					}
				}
			}				
			
            // split SUPER/SUBSC by finding maximum gap location
            if ( S_l != null && S_r != null ) {
                float super_gap = MaximumGapLocation( R.SUPER.strokes, S_l, S_r );
                float subsc_gap = MaximumGapLocation( R.SUBSC.strokes, S_l, S_r );

                // super
                foreach ( Stroke s in R.SUPER.strokes ) {
                    if ( s.aabb.Left > super_gap ) R.TLEFT.strokes.Add( s );
                }
                foreach ( Stroke s in R.TLEFT.strokes ) {
                    if ( R.SUPER.strokes.Contains( s ) ) R.SUPER.strokes.Remove( s );
                }

                // subsc
                foreach ( Stroke s in R.SUBSC.strokes ) {
                    if ( s.aabb.Left > subsc_gap ) R.BLEFT.strokes.Add( s );
                }
                foreach ( Stroke s in R.BLEFT.strokes ) {
                    if ( R.SUBSC.strokes.Contains( s ) ) R.SUBSC.strokes.Remove( s );
                }
            }

			// if nosupersub, add super/sub to TLEFT/BLEFT
			if ( P_l == PartitionClass.HorLine ) {
				foreach ( Stroke s in R.SUPER.strokes ) R.TLEFT.strokes.Add( s );
				R.SUPER.strokes.Clear();
				
				foreach ( Stroke s in R.SUBSC.strokes ) R.BLEFT.strokes.Add( s );
				R.SUBSC.strokes.Clear();
			}
			
			// Create LBTs.
			R.ABOVE.lbt = new LBT( R.ABOVE.strokes, LBT.DefaultAdjacentCriterion );
			R.BELOW.lbt = new LBT( R.BELOW.strokes, LBT.DefaultAdjacentCriterion );
			R.CONTAINS.lbt = new LBT( R.CONTAINS.strokes, LBT.DefaultAdjacentCriterion );
			R.SUPER.lbt = new LBT( R.SUPER.strokes, LBT.DefaultAdjacentCriterion );
			R.SUBSC.lbt = new LBT( R.SUBSC.strokes, LBT.DefaultAdjacentCriterion );
			R.TLEFT.lbt = new LBT( R.TLEFT.strokes, LBT.DefaultAdjacentCriterion );
			R.BLEFT.lbt = new LBT( R.BLEFT.strokes, LBT.DefaultAdjacentCriterion );
			
			// combine above/super/tleft and below/subsc/bleft for horline
			if ( indexed ) {
				if ( R.ABOVE.strokes.Count > 0 && ( R.SUPER.strokes.Count > 0 || R.TLEFT.strokes.Count > 0 ) ) {
					foreach ( Stroke s in R.SUPER.strokes ) R.ABOVE.strokes.Add( s );
					R.SUPER.strokes.Clear();
					R.SUPER.lbt = new LBT( R.SUPER.strokes, LBT.DefaultAdjacentCriterion );
					
					foreach ( Stroke s in R.TLEFT.strokes ) R.ABOVE.strokes.Add( s );
					R.TLEFT.strokes.Clear();
					R.TLEFT.lbt = new LBT( R.TLEFT.strokes, LBT.DefaultAdjacentCriterion );
					
                    R.ABOVE.strokes.Sort( ( s1, s2 ) => int.Parse( s1.stroke_id ) - int.Parse( s2.stroke_id ) );
					R.ABOVE.lbt = new LBT( R.ABOVE.strokes, LBT.DefaultAdjacentCriterion );
				}
				if ( R.BELOW.strokes.Count > 0 && ( R.SUBSC.strokes.Count > 0 || R.BLEFT.strokes.Count > 0 ) ) {
					foreach ( Stroke s in R.SUBSC.strokes ) R.BELOW.strokes.Add( s );
					R.SUBSC.strokes.Clear();
					R.SUBSC.lbt = new LBT( R.SUBSC.strokes, LBT.DefaultAdjacentCriterion );
					
					foreach ( Stroke s in R.BLEFT.strokes ) R.BELOW.strokes.Add( s );
					R.BLEFT.strokes.Clear();
					R.BLEFT.lbt = new LBT( R.BLEFT.strokes, LBT.DefaultAdjacentCriterion );

                    R.BELOW.strokes.Sort( ( s1, s2 ) => int.Parse( s1.stroke_id ) - int.Parse( s2.stroke_id ) );
					R.BELOW.lbt = new LBT( R.BELOW.strokes, LBT.DefaultAdjacentCriterion );
				} 
			}
			
			
			//Console.WriteLine("SUPER STROKES: {0}",R.SUPER.lbt.strokes.Count);
			
			return new PartitionResultWrapper(R, UpdateLBT( new Segment { strokes = SP }, lbt ));
			                                  //lbt = UpdateLBT( new Segment { strokes = SP }, lbt ) };
		}		
		
		public LBT UpdateLBT( Segment seg, LBT old_tree ) {
			LBT new_lbt = new LBT( old_tree );
			foreach ( Stroke s in seg.strokes ) new_lbt.PruneNode( s, LBT.DefaultAdjacentCriterion );
			return new_lbt;
		}
	}
}
