using System;
using System.Collections.Generic;
using System.Text;

namespace DataStructures
{
	/// <summary>
	/// Defines a stroke as a vector of coordinate pairs.
	/// </summary>
	public class Stroke
	{
		public AABB aabb;
		
		/// <summary>
		/// The vector of coordinates.
		/// </summary>
		public List< Vector2 > points;
		
		/// <summary>
		/// Constructor.
		/// </summary>
		public Stroke() 
		{
			points = new List< Vector2 >();
		}

		
		public override string ToString ()
		{
			return stroke_id;
		}
		
		/// <summary>
		/// Unique stroke ID assigned in the InkML file.
		/// </summary>
		public string stroke_id;
		
		/// <summary>
		/// Format the coordinates for output in InkML.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> containing an InkML-ready rendering of the coordinates for this stroke.
		/// </returns>
		public string PointsToInkML() {
			StringBuilder sb = new StringBuilder();
			
			for ( int i = 0; i < points.Count; i++ ) {
				sb.Append( points[ i ].x + " " + points[ i ].y );
				if ( i != points.Count - 1 ) sb.Append( ", " );
			}
			
			return sb.ToString();
		}
		
		public string coordsHttpRequest() {
			StringBuilder sb = new StringBuilder();
			
			for ( int i = 0; i < points.Count; i++ ) {
				sb.Append( points[ i ].x + "," + points[ i ].y );
				if ( i != points.Count - 1 ) sb.Append( "|" );
			}
			
			return sb.ToString();
		}
		
		public void PopulateBB() {
			aabb = new AABB();
			Vector2 min = new Vector2( float.MaxValue, float.MaxValue );
			Vector2 max = new Vector2( float.MinValue, float.MinValue );
			
			foreach ( Vector2 p in points ) {
				min.x = Math.Min( p.x, min.x );
				min.y = Math.Min( p.y, min.y );
				max.x = Math.Max( p.x, max.x );
				max.y = Math.Max( p.y, max.y );
			}
			
			this.aabb.Top = min.y;
			this.aabb.Right = max.x;
			this.aabb.Bottom = max.y;
			this.aabb.Left = min.x;
		}
		
		/// <summary>
		/// Computes the distance between two strokes. (The distance between two strokes is the minimum distance between any two points on either stroke.)
		/// </summary>
		/// <param name="s1">
		/// The first <see cref="Stroke"/>.
		/// </param>
		/// <param name="s2">
		/// The second <see cref="Stroke"/>.
		/// </param>
		/// <returns>
		/// The distance (as a <see cref="System.Double"/>) between the two strokes.
		/// </returns>
		public static float distance( Stroke s1, Stroke s2 ) {
			float min_dist = float.MaxValue;
			foreach ( Vector2 p1 in s1.points ) {
				foreach ( Vector2 p2 in s2.points ) {
					float dx = p1.x - p2.x;
					float dy = p1.y - p2.y;
					float dist =  dx * dx + dy * dy;
					if ( dist < min_dist ) min_dist = dist;
				}
			}
			return (float)Math.Sqrt(min_dist);
		}
		
		public override bool Equals ( object obj ) {
			if ( !( obj is Stroke ) ) return false;
			return stroke_id == ( obj as Stroke ).stroke_id;
		}
		
		public static bool Overlap( Stroke s1, Stroke s2 ) {
			if ( s1.Equals( s2 ) ) return true;
			
			if ( s1.points.Count == 0 || s2.points.Count == 0 ) return false;
			
			Vector2 s1_1 = s1.points[ 0 ];
			for ( int i = 1; i < s1.points.Count; i++ ) {
				Vector2 s1_2 = s1.points[ i ];
				
				// do stuff
				Vector2 s2_1 = s2.points[ 0 ];
				for ( int j = 1; j < s2.points.Count; j++ ) {
					Vector2 s2_2 = s2.points[ j ];
										
					// http://mathworld.wolfram.com/Line-LineIntersection.html
					float x1 = s1_1.x;
					float y1 = s1_1.y;
					float x2 = s1_2.x;
					float y2 = s1_2.y;
					float x3 = s2_1.x;
					float y3 = s2_1.y;
					float x4 = s2_2.x;
					float y4 = s2_2.y;					
					float x_num = ( ( x1 * y2 - x2 * y1 ) * ( x3 - x4 ) ) - ( ( y4 * x3 - x4 * y3 ) * ( x1 - x2 ) );
					float y_num = ( ( x1 * y2 - x2 * y1 ) * ( y3 - y4 ) ) - ( ( y4 * x3 - x4 * y3 ) * ( y1 - y2 ) );
					float den = ( y3 - y4 ) * ( x1 - x2 ) - ( x3 - x4 ) * ( y1 - y2 );
					
					if ( den != 0 ) {
						float xint = x_num / den;
						float yint = y_num / den;
							
						// TODO: make this faster/better?
						//List< float > xvals = new List< float > { x1, x2, x3, x4 };
						//xvals.Sort();
						//List< float > yvals = new List< float > { y1, y2, y3, y4 };
						//yvals.Sort();
						
						// "middle" two coordinates for each determine space
						// where intersection must lie?????
						if ( ( x1 < x2 ? ( x1 <= xint && xint <= x2 ) : ( x2 <= xint && xint <= x1 ) ) &&
						     ( y1 < y2 ? ( y1 <= yint && yint <= y2 ) : ( y2 <= yint && yint <= y1 ) ) &&
						     ( x3 < x4 ? ( x3 <= xint && xint <= x4 ) : ( x4 <= xint && xint <= x3 ) ) &&
						     ( y3 < y4 ? ( y3 <= yint && yint <= y4 ) : ( y4 <= yint && yint <= y3 ) ) ) {
#if DEBUG
							//Console.WriteLine( "{0} {1}", s1, s2 );
							//Console.WriteLine( "{0} {1}\n{2} {3}\n{4} {5}\n{6} {7}", x1, y1, x2, y2, x3, y3, x4, y4 );
#endif
							return true;
						}
					}
						    
					s2_1 = s2_2; // set up for next iteration
				}
				
				s1_1 = s1_2; // set up for next iteration				
			}
			
			return false;
		}
	}
}

