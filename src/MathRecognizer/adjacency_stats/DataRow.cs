using System;
using DataStructures;

namespace adjacency_stats
{
	public class DataRow
	{
		public MathExpression.NodeRelationship relationship = MathExpression.NodeRelationship.Unknown;
		public string parent_class = "unknown";
		public string child_class = "unknown";
		public float min_extent = float.MaxValue;
		public float max_extent = float.MinValue;
	}
}

