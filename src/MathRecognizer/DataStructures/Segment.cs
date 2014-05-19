using System.Collections.Generic;
using System.Text;
using System;

namespace DataStructures
{
	public class Segment : ICloneable
	{
		public List< Stroke > strokes;
		public AABB bb;
		public List< Classification > classification;
		
		public Segment() {
			strokes = new List< Stroke >();
			bb = new AABB();
		}
		
		public Segment(List<Stroke> inStrokes) 
        	{
			strokes = inStrokes;
			PopulateBB();
			classification = null;
		}

		public string GetUniqueName()
		{
			List<string> stroke_ids = new List<string>();
			foreach (Stroke str in strokes)
			{
				int insertion_index = stroke_ids.BinarySearch(str.stroke_id);
				if (insertion_index < 0)
					insertion_index = ~insertion_index;
				stroke_ids.Insert(insertion_index, str.stroke_id);
			}
			StringBuilder sb = new StringBuilder();
			foreach (string s in stroke_ids)
				sb.Append(s).Append(',');
			return sb.ToString();
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			
			if ( classification != null ) {
				for ( int i = 0; i < classification.Count; i++ ) {
					sb.Append("[" + classification[ i ] //.symbol + ", " + classification[ i ].probability 
					          + ", str:" + strokes.Count + "]");
					if ( i != classification.Count - 1 ) sb.Append( " " );
				}
			} else {
				sb.Append( "NO CLASSIFICATION" );	
			}
			
			return sb.ToString();
		}
		
		public string ClassificationToString() {
			StringBuilder sb = new StringBuilder();
			
			if ( classification != null ) {
				for ( int i = 0; i < classification.Count; i++ ) {
					sb.Append( classification[ i ].symbol + "," + classification[ i ].probability );
					if ( i != classification.Count - 1 ) sb.Append( "|" );
				}
			} else {
				sb.Append( "NO CLASSIFICATION" );	
			}
			
			return sb.ToString();
		}
		
		public void PopulateBB() 
		{
			Vector2 min = new Vector2( float.MaxValue, float.MaxValue );
			Vector2 max = new Vector2( float.MinValue, float.MinValue );
			foreach ( Stroke s in strokes ) {
				s.PopulateBB();
				AABB box = s.aabb;
				min.x = Math.Min( box.Left, min.x );
				min.y = Math.Min( box.Top, min.y );
				max.x = Math.Max( box.Right, max.x);
				max.y = Math.Max( box.Bottom, max.y );
			}
			this.bb = new AABB();

			
			this.bb.Top = min.y;
			this.bb.Right = max.x;
			this.bb.Bottom = max.y;
			this.bb.Left = min.x;
		}
		
		public override bool Equals( object obj ) {
			if ( !( obj is Segment ) ) return false;
			Segment o = obj as Segment;
			
			// verify stroke lists are the same
			if ( strokes.Count != o.strokes.Count ) return false;
			foreach ( Stroke s in strokes ) if ( !o.strokes.Contains( s ) ) return false;
			return true;
		}
		
		public object Clone() {
			Segment s = new Segment();
			s.bb = bb.Clone() as AABB;
			foreach ( Stroke str in strokes ) s.strokes.Add( str );
			if ( classification != null ) {
				s.classification = new List< Classification >();
				foreach ( Classification c in classification ) {
					s.classification.Add( new Classification(c.symbol, c.probability, c.note ) );
				}
			}
			return s;
		} 
	}
}

