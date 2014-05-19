using System.Collections.Generic;

namespace DataStructures
{
	public class ClassificationResult
	{
		public List< string > stroke_ids;
		public List< Classification > classifications;
		
		public ClassificationResult() {
			stroke_ids = new List< string >();
			classifications = new List< Classification >();
		}
	}
}

