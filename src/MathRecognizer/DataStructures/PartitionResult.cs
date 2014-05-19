using System;
using System.Collections.Generic;
namespace DataStructures
{
	public class PartitionResultWrapper {
		public PartitionResult result;
		public LBT lbt;
		
		public PartitionResultWrapper(PartitionResult inResult,
	                                    LBT inLBT) {
			result = inResult;
			lbt = new LBT(inLBT);
		}
	}
	

	
	public class PartitionResult
	{
		public RelationTreeNode ABOVE;
		public RelationTreeNode BELOW;
		public RelationTreeNode CONTAINS;
		public RelationTreeNode SUPER;
		public RelationTreeNode SUBSC;
		
		// Additions.
		public RelationTreeNode TLEFT;
		public RelationTreeNode BLEFT;
		
		public PartitionResult ()
		{
			ABOVE = new RelationTreeNode( "ABOVE", "_partition" );
			BELOW = new RelationTreeNode( "BELOW", "_partition" );
			CONTAINS = new RelationTreeNode( "CONTAINS", "_partition" );
			SUPER = new RelationTreeNode( "SUPER", "_partition" );
			SUBSC = new RelationTreeNode( "SUBSC", "_partition" );
			
			// Additions.
			TLEFT = new RelationTreeNode( "TLEFT", "_partition" );
			BLEFT = new RelationTreeNode( "BLEFT", "_partition" );
		}
		
		public override string ToString ()
		{
			return string.Format("ABO: {0} BEL: {1} CON: {2} SUP: {3} SUB: {4} TLT: {5} BLT: {6}",
			                     ABOVE.strokes.Count,BELOW.strokes.Count,CONTAINS.strokes.Count,
			                     SUPER.strokes.Count, SUBSC.strokes.Count, TLEFT.strokes.Count, BLEFT.strokes.Count);
		}
		
		public bool empty() {
			return ABOVE.empty() && BELOW.empty() && CONTAINS.empty() && SUPER.empty() && SUBSC.empty() && TLEFT.empty() &&
				BLEFT.empty();
		}
	}
}

