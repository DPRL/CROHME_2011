using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;

namespace DataStructures
{
	/// <summary>
	/// Represents the contents of an InkML file, including some common annotation elements as well as
	/// the complete stroke ("trace") and segment ("traceGroup") data.
	/// </summary>
	/// <remarks>
	/// - The annotation fields will be null if that annotation was not present in the file.
	/// - Any error processing stroke or segment data will result in that stroke or segment not being recorded
	/// in this structure.
	/// </remarks>
	public class InkML
	{
		public static string GetUniqueID() 
		{
			return Path.GetRandomFileName();	
		}
		
		public class Trace
		{
			public string id;
			public List<Vector2> points;
			
			public AABB aabb
			{
				get
				{
					Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
					Vector2 max = new Vector2(float.MinValue, float.MinValue);
					
					foreach(Vector2 p in points)
					{
						min.x = Math.Min(p.x, min.x);
						min.y = Math.Min(p.y, min.y);
						max.x = Math.Max(p.x, max.x);
						max.y = Math.Max(p.y, max.y);
					}
					
					AABB result = new AABB();

					result.Left = min.x;
					result.Right = max.x;
					result.Top = min.y;
					result.Bottom = max.y;
					return result;
				}
			}
			
			public Trace()
			{
				points = new List<Vector2>();
				id = "";
				

			}
			
			public Stroke ToStroke()
			{
				Stroke result = new Stroke();
				result.stroke_id = id;
				// build AABB and add points
				float min_x = float.PositiveInfinity;
				float min_y = float.PositiveInfinity;
				float max_x = float.NegativeInfinity;
				float max_y = float.NegativeInfinity;
				foreach(Vector2 v in points)
				{
					result.points.Add(v);
					min_x = Math.Min(min_x, v.x);
					min_y = Math.Min(min_y, v.y);
					max_x = Math.Max(max_x, v.x);
					max_y = Math.Max(max_y, v.y);
				}
				// construct aabb
				result.aabb = new AABB();
				result.aabb.Left = min_x;
				result.aabb.Right = max_x;
				result.aabb.Top = min_y;
				result.aabb.Bottom = max_y;
				
				return result;
			}
		}
		
		public class TraceGroup
		{
			private static int INST_ID = 0;
			
			public string truth;
			public string mathml_href;
			public List< Trace > trace_views;
			private int id_num;
			
			public string id 
			{
				get {
					return "tg_" + id_num;
				}
			}
			
			public AABB aabb
			{
				get
				{
					Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
					Vector2 max = new Vector2(float.MinValue, float.MinValue);
					foreach(Trace t in trace_views)
					{
						AABB box = t.aabb;
						min.x = Math.Min(box.Left, min.x);
						min.y = Math.Min(box.Top, min.y);
						max.x = Math.Max(box.Right, max.x);
						max.y = Math.Max(box.Bottom, max.y);
					}
					AABB result = new AABB();
					result.Left = min.x;
					result.Right = max.x;
					result.Top = min.y;
					result.Bottom = max.y;

					return result;
				}
			}
			
			public TraceGroup()
			{
				truth = "";
				mathml_href = "";
				trace_views = new List<Trace>();
				id_num = Interlocked.Increment(ref INST_ID);
			}
			
			
		}

		public class MathML
		{
			public string mathml;	
		}
		
		public string filename;
		public Dictionary<string, string> annotations;
		public Dictionary<string, Trace> trace_id_to_trace;
		public Dictionary<string, TraceGroup> trace_id_to_tracegroup;
		public Dictionary<string, TraceGroup> mathml_id_to_tracegroup;
		public Dictionary< string, string > classification_attributes;
		public List<Trace> traces;
		public List<TraceGroup> trace_groups;
		public MathML mathml_expression;

		/// <summary>
		/// List of highlight rendering data to produce. 
		/// </summary>
		public List< HighlightRender > highlights;
		
		/// <summary>
		/// The InkML namespace.
		/// </summary>
		private static XNamespace _ns = "http://www.w3.org/2003/InkML";
		
		/// <summary>
		/// The MathML namespace. 
		/// </summary>
		private static XNamespace _mathml_ns = "http://www.w3.org/1998/Math/MathML";
		
		/// <summary>
		/// Constructor. Initializes required fields. 
		/// </summary>
		public InkML()
		{
			traces = new List<Trace>();
			trace_groups = new List<TraceGroup>();
			annotations = new Dictionary<string, string>();
			trace_id_to_trace = new Dictionary<string, Trace>();
			trace_id_to_tracegroup = new Dictionary<string, TraceGroup>();
			highlights = new List< HighlightRender >();
			classification_attributes = new Dictionary< string, string >();
			mathml_id_to_tracegroup = new Dictionary<string, TraceGroup>();
		}
		
		/// <summary>
		/// Populates a new InkML object from a .inkml file.
		/// </summary>
		/// <param name="file">
		/// A <see cref="System.String" /> containing the location of the file to read.
		/// </param>
		/// <returns>
		/// An <see cref="InkML" /> object, populated with data from the file.
		/// </returns>
		/// <remarks>
		/// - The stroke data contained in the segment objects created by this method are
		/// references to the corresponding stroke objects in the stroke collection.
		/// </remarks>
		public static InkML NewFromFile( string file ) 
		{			
			if ( !File.Exists( file ) ) return null;
			
			InkML obj = new InkML();
			obj.filename = System.IO.Path.GetFileName( file );
			
			XDocument doc = XDocument.Load( file );
			
			// pull out annotations
			foreach ( var el in doc.Element( _ns + "ink" ).Elements( _ns + "annotation" ) ) 
			{
				string type = el.Attribute( "type" ).Value;
				string val = el.Value;
				
				obj.annotations[type] = val;
			}
			
			// get traces
			foreach ( var el in doc.Element( _ns + "ink" ).Elements( _ns + "trace" ) ) 
			{
				try 
				{
					Trace trace = new Trace();
					trace.id =  el.Attribute( "id" ).Value;
					
					string raw_coords = el.Value;
					foreach ( string raw_coordpair in raw_coords.Split( new char[] { ',' } ) ) 
					{						
						string[] coordpair = raw_coordpair.Trim().Split( new char[] { ' ' } );
						float x_coord = float.Parse( coordpair[ 0 ].Trim() );
						float y_coord = float.Parse( coordpair[ 1 ].Trim() );
						trace.points.Add(new Vector2(x_coord,  y_coord));
					}
					
					obj.traces.Add(trace);
					obj.trace_id_to_trace[trace.id] = trace;
				}
				catch ( Exception e ) 
				{
					Console.Error.WriteLine( e.ToString() );
				}
			}
			
			// get trace groups
            var trace_group_elements = doc.Element( _ns + "ink" ).Elements( _ns + "traceGroup" );
            if ( trace_group_elements.Count() > 0 ) {
			    var trace_groups = trace_group_elements.Last();
			    foreach ( var trace_group in trace_groups.Elements() ) 
			    {
				    TraceGroup tg = new TraceGroup();
				    tg.truth = ( from c in trace_group.Elements( _ns + "annotation" )
										    where c.Attribute( "type" ).Value.Equals( "truth" )
										    select c.Value ).LastOrDefault();
    				
    				
				    tg.mathml_href = ( from c in trace_group.Elements( _ns + "annotationXML" )
										    select c.Attribute( "href" ).Value ).LastOrDefault();
    				
    				
    				
				    if ( tg.truth == null || tg.mathml_href == null ) continue;
				    obj.mathml_id_to_tracegroup.Add(tg.mathml_href, tg);
				    foreach ( var trace_view in trace_group.Elements( _ns + "traceView" ) ) 
				    {
					    try 
					    {
						    string trace_data_ref = trace_view.Attribute( "traceDataRef" ).Value;
						    tg.trace_views.Add(obj.trace_id_to_trace[trace_data_ref]);
						    obj.trace_id_to_tracegroup[trace_data_ref] = tg;
    						
						    //obj.stroke_truths.Add( trace_data_ref, segment.ann_truth );
					    }
					    catch ( Exception e ) 
					    {
						    Console.Error.WriteLine( e.ToString() );
					    }
				    }
				    obj.trace_groups.Add(tg);
			    }
            }

			// get mathml
			var mathml = doc.Descendants( _mathml_ns + "math" ).FirstOrDefault();
			if ( mathml != null ) 
			{
				obj.mathml_expression = new MathML {mathml = mathml.ToString()};
			}
			
			return obj;
		}
		
		/// <summary>
		/// Prints annotation, stroke, and segment data in tabular form. 
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> containing a human-readable and -understandable string
		/// representation of the file data.
		/// </returns>
		public override string ToString() 
		{
			StringBuilder sb = new StringBuilder();
			
			sb.AppendLine( "ANNOTATION DATA");
			sb.AppendLine();
			sb.AppendFormat( "{0,20}\tVALUE\n", "ANNOTATION" );
			sb.AppendFormat( "{0,20}\t-----\n", "----------" );
			foreach ( var fieldinfo in GetType().GetFields() ) {
				if ( fieldinfo.FieldType.GetInterface( "System.Collections.ICollection" ) != null ) continue;
				sb.AppendFormat( "{0,20}\t{1}\n", fieldinfo.Name, fieldinfo.GetValue( this ) );
			}			
			
			sb.AppendLine();
			sb.AppendLine( "STROKE DATA" );
			sb.AppendLine();
			sb.AppendFormat( "{0,20}\tCOORDINATES\n", "STROKE ID" );
			sb.AppendFormat( "{0,20}\t-----------\n", "---------" );
//			foreach ( string stroke_id in strokes.Keys ) 
			foreach(Trace tr in traces)
			{
				//if ( stroke.coords.Count == 0 ) 
				if(tr.points.Count == 0)
				{
					sb.AppendFormat( "{0,20}\t[empty]\n", tr.id );
				}
				else 
				{
					sb.AppendFormat( "{0,20}\t{1}\n", tr.id, tr.points[ 0 ] );
				}
				
				for ( int i = 1; i < tr.points.Count; i++ ) 
				{
					sb.AppendFormat( "{0,20}\t{1}\n", null, tr.points[ i ] );
				}
				sb.AppendLine();
			}
			
			sb.AppendLine();
			sb.AppendLine( "SEGMENT DATA" );
			sb.AppendLine();
			sb.AppendFormat( "{0,20}\tSTROKE REF\n", "LABEL" );
			sb.AppendFormat( "{0,20}\t----------\n", "-----" );
			foreach ( TraceGroup tg in trace_groups ) 
			{
				sb.AppendFormat( "{0,20}\t{1}\n", tg.truth, tg.trace_views[0].id);
				for(int i = 1; i < tg.trace_views.Count; i++)
					sb.AppendFormat("{0,20}\t{1}\n", null, tg.trace_views[i].id);
				sb.AppendLine();
			}
			return sb.ToString();
		}
		
		/// <summary>
		/// Prints the data in an InkML format.
		/// </summary>
		/// <param name="hr">
		/// The highlight to include. If null, no highlights will be included.
		/// </param>
		/// <returns>
		/// A <see cref="System.String" /> containing the raw InkML output file data.
		/// </returns>
		public string ToInkML( HighlightRender hr ) 
		{
			StringBuilder sb = new StringBuilder();
			
			sb.AppendLine( "<ink xmlns=\"" + _ns.ToString() + "\">" );

			foreach(var pair in annotations)
			{
				sb.AppendLine(" \t" + annotationMarkup(pair.Key, pair.Value));	
			}
			
			sb.AppendLine( "\t<annotationXML type=\"truth\" encoding=\"Content-MathML\">" );
			foreach ( string m in mathml_expression.mathml.Split( new char[] { '\n', '\r' } ) ) sb.AppendLine( "\t\t" + m );
			sb.AppendLine( "\t</annotationXML>" );
			
			foreach(Trace t in traces)
			{
				string colorinfo = "";
				if ( hr != null && hr.trace_ids.Contains( t.id ) ) {
					colorinfo = " color=\"" + hr.color + "\"";
				}
				
				string classificationinfo = "";
				if ( classification_attributes.ContainsKey( t.id ) ) {
					classificationinfo = " classification=\"" + classification_attributes[ t.id ] + "\"";	
				}
				
				sb.AppendLine( "\t<trace id=\"" + t.id + "\"" + colorinfo + classificationinfo + ">");
				
				for(int k = 0; k < t.points.Count - 1; k++)
				{
					sb.Append(t.points[k].x).Append(' ').Append(t.points[k].y).Append(", ");
				}
				sb.AppendLine(t.points[t.points.Count - 1].x + " " + t.points[t.points.Count - 1].y);
				
				sb.AppendLine("</trace>" );
			}
			
            if ( trace_groups.Count > 0 ) {
			    sb.AppendLine( "\t<traceGroup xml:id=\"A\">" );
			    sb.AppendLine( "\t\t<annotation type=\"truth\">Segmentation</annotation>" );
			    for(int i = 0; i < trace_groups.Count; i++)
			    {
				    TraceGroup tg = trace_groups[i];
				    sb.AppendLine( "\t\t<traceGroup xml:id=\"" + tg.id + "\">" );
				    sb.AppendLine( "\t\t\t" + annotationMarkup( "truth", tg.truth ));
				    if(tg.mathml_href != null)
					    sb.AppendLine( "\t\t\t\t<annotationXML href=\"" + tg.mathml_href + "\" />" );
				    foreach(Trace tr in tg.trace_views)
					    sb.AppendLine( "\t\t\t<traceView traceDataRef=\"" + tr.id + "\" />" );
				    sb.AppendLine( "\t\t</traceGroup>" );
			    }			
			    sb.AppendLine( "\t</traceGroup>" );
			}

			sb.AppendLine( "</ink>" );
			
			return sb.ToString();
		}
		
		/// <summary>
		/// Uses all highlight data to return a set of InkML file contents.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String[]"/> whose elements are individual InkML file data, each one with a single
		/// highlight applied.
		/// </returns>
		public string[] ToInkML() {
			List< string > data = new List< string >();
			foreach ( HighlightRender hr in highlights ) {
				data.Add( ToInkML( hr ) );	
			}
			return data.ToArray();
		}
		
		private string annotationMarkup( string type, string value ) {
			return "<annotation type=\"" + type + "\">" + value + "</annotation>";	
		}
		/*
		/// <summary>
		/// Computes the nearest neighbor to the stroke corresponding to the given stroke ID. This method will use all other strokes
		/// in the file as candidates.
		/// </summary>
		/// <param name="stroke_id">
		/// A <see cref="System.String" /> containing the stroke ID of the stroke for which to find the nearest neighbor.
		/// </param>
		/// <returns>
		/// A <see cref="System.String" /> containing the stroke ID of the nearest neighbor.
		/// </returns>
		public string nearestNeighbor( string stroke_id ) {
			return nearestNeighbor( stroke_id, strokes.Keys.ToList() );
		}
		
		/// <summary>
		/// Computes the nearest neighbor to the stroke corresponding to the given stroke ID. This method will use only strokes whose IDs are
		/// in the candidate_ids argument as candidates.
		/// </summary>
		/// <param name="stroke_id">
		/// A <see cref="System.String" /> containing the stroke ID of the stroke for which to find the nearest neighbor.
		/// </param>
		/// <param name="candidate_ids">
		/// A <see cref="List<System.String>" /> containing the candidate stroke IDs.
		/// </param>
		/// <returns>
		/// A <see cref="System.String" /> containing the stroke ID of the nearest neighbor.
		/// </returns>
		/// <summary>
		public string nearestNeighbor( string stroke_id, List< string > candidate_ids ) {
			Dictionary< string, double > distances = new Dictionary< string, double >();
			Stroke stroke = strokes[ stroke_id ];
			
			foreach ( string sid in candidate_ids ) {
				if ( sid.Equals( stroke_id ) ) continue; // same stroke
				distances.Add( sid, Stroke.distance( stroke, strokes[ sid ] ) );
			}
			
			var min_dist = distances.OrderBy( k => k.Value ).FirstOrDefault(); // order by distance
			return min_dist.Key; // the stroke ID; min_dist.Value would yield the minimum distance
		}
		*/
	}
}