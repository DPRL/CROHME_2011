using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
namespace DataStructures
{
	public class LBT
	{
		public class LBTNode
		{
			public Stroke stroke;
			public List<LBTNode> children;
			
			public LBTNode()
			{
				stroke = null;
				children = new List<LBTNode>();
			}
			
			public LBTNode(LBTNode other)
			{
				stroke = other.stroke;
				children = new List<LBTNode>();
				foreach(LBTNode n in other.children)
					children.Add(new LBTNode(n));
			}
		}
		
		// list of strokes that comprise this LBT
		public List<Stroke> strokes;
		public Dictionary<Stroke, LBTNode> stroke_to_node;
		public InkML raw_data;
		
		// returns true if two AABBs are adjacent
		public delegate bool AdjacentCriterion(AABB left, AABB right);
		// returns true if the given stroke shoudl be considered as a valid choice when doing KNN
		public delegate bool KNNFilter(Stroke s, LBT tree);
		
		public static AdjacentCriterion DefaultAdjacentCriterion = ( l, r ) => true;
		public static KNNFilter DefaultKNNFilter = (s, t) => true;
		
		public LBTNode root;
		
		public LBT (InkML in_data, AdjacentCriterion adjacent)
		{
			raw_data = in_data;
			// sort boxes by left hand extent
			// starting from left, move right and evaluate which strokes have something to the left
			// use filtering function to evaluate if is valid
			strokes = new List<Stroke>();
			stroke_to_node = new Dictionary<Stroke, LBTNode>();
			
			foreach(InkML.Trace tr in raw_data.traces)
				strokes.Add(tr.ToStroke());
			
			GraphFromStrokeList( this, strokes, adjacent );
		}
		
		public LBT( List< Stroke > inStrokes, AdjacentCriterion adjacent ) 
		{
			// DEBUG: input parameter was getting updated ('inStrokes' was 'strokes'),
			// not actual stroke data structure.
			strokes = new List<Stroke>();
			
			if ( inStrokes != null ) {
				foreach ( Stroke s in inStrokes ) strokes.Add( s );
			}
			
			stroke_to_node = new Dictionary<Stroke, LBTNode>();
			GraphFromStrokeList( this, strokes, adjacent );
		}
		
		public LBT(LBT other)
		{
			if ( other != null ) {
				// clone stroke list
				strokes = new List<Stroke>();
				if ( other.strokes != null ) {
					foreach(Stroke s in other.strokes)
					{
						strokes.Add(s);
					}
				}
				// clone tree
				root = new LBTNode(other.root);
	
				// rebuild dictionary
				stroke_to_node = new Dictionary<Stroke, LBTNode>();
				Stack stack = new Stack();
				stack.Push(root);
				while(stack.Count > 0)
				{
					LBTNode node = (LBTNode)stack.Pop();
					if(node != root)
						stroke_to_node.Add(node.stroke, node);
					foreach(LBTNode ln in node.children)
						stack.Push(ln);
				}
				raw_data = null;
			}
		}
		
		public void PruneNode(Stroke s, AdjacentCriterion adjacent)
		{
			strokes.Remove(s);
			stroke_to_node.Clear();
			GraphFromStrokeList( this, strokes, adjacent );
		}
		
		public void GraphFromStrokeList( LBT lbt, List< Stroke > strokes, AdjacentCriterion adjacent ) {
			strokes.Sort(delegate(Stroke a, Stroke b) 
			{


				return Math.Sign(a.aabb.Left - b.aabb.Left);

				//if(a.aabb.Left < b.aabb.Left) return -1; 
				//if(a.aabb.Left > b.aabb.Left) return 1; 
				//return 0;
			});
					
			lbt.root = new LBTNode();
			lbt.root.stroke = null;
			for(int k = 0; k < strokes.Count; k++)
			{
				LBTNode node = new LBTNode();
				node.stroke = strokes[k];
				lbt.stroke_to_node[strokes[k]] = node;
				if(k == 0)
				{
					lbt.root.children.Add(node);
					continue;
				}
				
				// iterate over boxes to the left of this box
				for(int j = k - 1; j >= 0; j--)
				{
					// if strokes don't overlap on y
					if(Math.Abs(strokes[k].aabb.Center.y - strokes[j].aabb.Center.y) > (strokes[k].aabb.Radius.y + strokes[j].aabb.Radius.y))
					{
						if(j == 0)
							lbt.root.children.Add(node);
						continue;
					}
					
					if(adjacent(strokes[j].aabb, strokes[k].aabb))
					{
						LBTNode parent = lbt.stroke_to_node[strokes[j]];
						parent.children.Add(node);
						break;
					}
				}
			}
		}
	
		public override bool Equals( object obj ) {
			if ( !( obj is LBT ) ) return false;
			LBT o = obj as LBT;
			
			// verify stroke lists are the same
			if ( strokes.Count != o.strokes.Count ) return false;
			foreach ( Stroke s in strokes ) if ( !o.strokes.Contains( s ) ) return false;
			return true;
		}
		
		public string ToDOT()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("digraph g");
			sb.AppendLine("{");
			sb.AppendLine("\trankdir=LR;");
			
			//foreach(KeyValuePair<string,string> kvp in raw_data.stroke_truths)
			foreach(var pair in raw_data.trace_id_to_tracegroup)
			{
				string label = pair.Value.truth;
				if(label[0] == '\\')
					label = label.Substring(1);
				
				sb.Append("\t").Append("_" + pair.Key).Append(" [label=\"").Append(label).AppendLine("\"];");	
			}
			
			print_node(sb, root);
			
			sb.AppendLine("}");
			
			return sb.ToString();
		}
		
		private void print_node(StringBuilder sb, LBTNode node)
		{
			string node_name = "";
			if(node.stroke == null)
				node_name = "root";	
			else
			{
				node_name = "_" + node.stroke.stroke_id;
			}
			
			for(int k = 0; k < node.children.Count; k++)
			{
				sb.Append("\t").Append(node_name).Append(" -> ").Append("_" + node.children[k].stroke.stroke_id).AppendLine(";");
				print_node(sb, node.children[k]);
			}
		}
		
				
		public List<Stroke> KNearestNeighbors(Stroke in_stroke, int k, KNNFilter filter)
		{
			List<float> distances = new List<float>();
			// index buffer for soring strokes by distance to in stroke
			List<int>index_buffer = new List<int>();
			for(int i = 0; i < strokes.Count; i++)
			{
				// ensure myself is at the end of the list
				if(in_stroke == strokes[i])
					distances.Add(float.PositiveInfinity);
				else	
					distances.Add(Stroke.distance(strokes[i], in_stroke));
				index_buffer.Add(i);
			}
			
			// sort index buffer by stroke distances
			index_buffer.Sort(delegate(int left, int right) {if(distances[left] < distances[right]) return -1; if(distances[left] > distances[right]) return 1; return 0;});
			
			List<Stroke> result = new List<Stroke>();
			
			// stop searching when we reach the last element (in_stroke)
			// or when we have all our results
			for(int i = 0; i < strokes.Count - 1 && result.Count < k; i++)
			{
				Stroke s = strokes[index_buffer[i]];
				if(filter(s, this) == true)
					result.Add(s);
			}
			
			/*
			foreach ( int i in index_buffer ) {
				Console.WriteLine( i + " " + distances[ i ] );	
			}
			*/
			
			return result;
		}
	}
}

