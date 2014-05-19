using System;
namespace DataStructures
{
	public class LexerResult : ICloneable, IComparable
	{
		public Segment segment;
		public LBT lbt; // pruned
		public bool adj;
		
		public RelationTreeNode TLEFT;
		public RelationTreeNode BLEFT;
		
		public override string ToString ()
		{
			return string.Format ("LexerResult::{0}",segment);
		}
		
		public object Clone() {
			LexerResult r = new LexerResult();
			r.segment = segment.Clone() as Segment;
			r.lbt = new LBT( lbt );
			r.adj = adj;
            r.TLEFT = TLEFT;
            r.BLEFT = BLEFT;
			return r;
		}
		
		public int CompareTo(object obj) 
		{
			// Need to negate comparison to sort by decreasing probability.
			if(obj is LexerResult) {
				LexerResult t = (LexerResult) obj;
				if (t.segment.classification.Count > 0 && segment.classification.Count > 0) {
					return -(segment.classification[0].probability.
				         CompareTo(t.segment.classification[0].probability));
				} else {
					throw new ArgumentException("LexerResult::CompareTo applied where object or" +
						" comparison has no associated classification results.");
				}
			}
			throw new ArgumentException("LexerResult::CompareTo(): object is not a LexerResultn.");
		}
		
		public override bool Equals ( object obj ) {
			if ( obj == null || !( obj is LexerResult ) ) return false;
			LexerResult o = obj as LexerResult;
			
			if ( !segment.GetUniqueName().Equals( o.segment.GetUniqueName() ) ) return false;
			
			// test classifications
			if ( segment.classification.Count != o.segment.classification.Count ) return false;
			for ( int i = 0; i < segment.classification.Count; i++ ) {
				Classification c1 = segment.classification[ i ];
				Classification c2 = o.segment.classification[ i ];
				if ( !c1.Equals( c2 ) ) return false;
			}
			
			return true;
		}
	}
}

