using System.Collections.Generic;
using System.Net;
using DataStructures;
using System.IO;
using System;
using System.Xml.Linq;
using System.Linq;

namespace Lexer {
	
	public class ClassifyManager 
	{
        private double prob_threshold;
        private string URL;

		//public static string URL = "http://saskatoon.cs.rit.edu:1500/";
		public static Dictionary<string, string> lei_to_icdar = new Dictionary<string, string>();

		public Dictionary<string, List<Classification>> cached_results = new Dictionary<string, List<Classification>>();

		private static void build_dictionary()
		{
			lei_to_icdar["_dash"] = "-";
			lei_to_icdar["_equal"] = "=";
			lei_to_icdar["_excl"] = "!";
			lei_to_icdar["_lparen"] = "(";
			lei_to_icdar["_plus"] = "+";
			lei_to_icdar["_rparen"] = ")";
			lei_to_icdar["0"] = "0";
			lei_to_icdar["1"] = "1";
			lei_to_icdar["2"] = "2";
			lei_to_icdar["3"] = "3";
			lei_to_icdar["4"] = "4";
			lei_to_icdar["5"] = "5";
			lei_to_icdar["6"] = "6";
			lei_to_icdar["7"] = "7";
			lei_to_icdar["8"] = "8";
			lei_to_icdar["9"] = "9";
			lei_to_icdar["a_lower"] = "a";
			lei_to_icdar["A_upper"] = "A";
			lei_to_icdar["alpha"] = "\\alpha";
			lei_to_icdar["b_lower"] = "b";
			lei_to_icdar["B_upper"] = "B";
			lei_to_icdar["beta"] = "\\beta";
			lei_to_icdar["c_lower"] = "c";
			lei_to_icdar["C_upper"] = "C";
			lei_to_icdar["cos"] = "\\cos";
			lei_to_icdar["d_lower"] = "d";
			lei_to_icdar["div"] = "\\div";
			lei_to_icdar["e_lower"] = "e";
			lei_to_icdar["F_upper"] = "F";
			lei_to_icdar["gamma"] = "\\gamma";
			lei_to_icdar["geq"] = "\\geq";
			lei_to_icdar["i_lower"] = "i";
			lei_to_icdar["infty"] = "\\infty";
			lei_to_icdar["int"] = "\\int";
			lei_to_icdar["j_lower"] = "j";
			lei_to_icdar["k_lower"] = "k";
			lei_to_icdar["ldots"] = "\\ldots";
			lei_to_icdar["leq"] = "\\leq";
			lei_to_icdar["lim"] = "\\lim";
			lei_to_icdar["log"] = "\\log";
			lei_to_icdar["lt"] = "\\lt";
			lei_to_icdar["n_lower"] = "n";
			lei_to_icdar["neq"] = "\\neq";
			lei_to_icdar["phi"] = "\\phi";
			lei_to_icdar["pi"] = "\\pi";
			lei_to_icdar["pm"] = "\\pm";
			lei_to_icdar["rightarrow"] = "\\rightarrow";
			lei_to_icdar["sin"] = "\\sin";
			lei_to_icdar["sqrt"] = "\\sqrt";
			lei_to_icdar["sum"] = "\\sum";
			lei_to_icdar["tan"] = "\\tan";
			lei_to_icdar["theta"] = "\\theta";
			lei_to_icdar["times"] = "\\times";
			lei_to_icdar["x_lower"] = "x";
			lei_to_icdar["y_lower"] = "y";
			lei_to_icdar["z_lower"] = "z";
		}

		public ClassifyManager( string URL, double prob_treshold )
		{
            this.prob_threshold = prob_treshold;
            this.URL = URL;

			lock (lei_to_icdar)
			{
				if (lei_to_icdar.Count == 0)
					build_dictionary();
			}
		}

		public void classify( Segment segment ) 
		{
			List<Classification> classification;
			if(cached_results.TryGetValue(segment.GetUniqueName(), out classification))
			{
				segment.classification = new List<Classification>(classification);
				return;
			}

			string requestUrl = URL + "?segment=false&segmentList=<SegmentList>";
			foreach ( Stroke s in segment.strokes ) {
				requestUrl += "<Segment type=\"pen_stroke\" instanceID=\"" + s.stroke_id + "\" scale=\"1,1\" translation=\"0,0\" ";
				requestUrl += "points=\"" + s.coordsHttpRequest() + "\" />";
			}
			requestUrl += "</SegmentList>";
			
			string result = httpRequest( requestUrl );			
			XDocument doc;			
			try {
				doc = XDocument.Parse( result );
			} catch ( Exception ) {
				Console.Error.WriteLine( "Could not parse classifcation response as XML. The returned response was: \"{0}\".", result );
				return;
			}
			
			var rec_result = doc.Descendants( "RecognitionResults" ).FirstOrDefault();
			if ( rec_result == null ) {
				Console.Error.WriteLine( "No recognition results returned.", result );
				return;
			}
			
			segment.classification = new List< Classification >();
			foreach ( var result_el in rec_result.Elements( "Result" ) ) 
			{
				Classification c = new Classification { symbol = lei_to_icdar[result_el.Attribute( "symbol" ).Value],
														probability = double.Parse( result_el.Attribute( "certainty" ).Value ) };
				if ( c.probability > prob_threshold ) 
					segment.classification.Add( c );
			}

			cached_results[segment.GetUniqueName()] = new List<Classification>(segment.classification);
		}
		
		private static string httpRequest( string url ) 
		{
			while(true)
			{
				try
				{
					HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
					var response = req.GetResponse();
					using (StreamReader sr = new StreamReader(response.GetResponseStream()))
					{
						return sr.ReadToEnd();
					}
				}
				catch (WebException)
				{

				}
			}
		}
		
	}
	
}