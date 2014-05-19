using System;

namespace DataStructures
{
	public class Classification : IComparable
	{
		public string symbol;
		public double probability;
		public string note;
		
		public Classification(string id, double p) {
			symbol = id;
			probability = p;
			note = null;
		}
		
		public Classification(string id, double p, string anote) {
			symbol = id;
			probability = p;
			note = anote;
		}
		
		public Classification(){}
		
		public override string ToString ()
		{
			string outString;
			if (note == null) {
				outString = String.Format("({0}, {1:0.000000000e-0})",symbol,probability);
			} else {
				outString =  String.Format("({0}, {1:0.000000000e-0}, {2})",symbol,probability,note);
			}
			return outString;
		}
		
		public int CompareTo(object obj) 
		{
			// Need to negate comparison to sort by decreasing probability.
			if(obj is Classification) {
				Classification t = (Classification) obj;
				return -(probability.CompareTo(t.probability));
			}
			throw new ArgumentException("Classification::CompareTo(): object is not a Classification.");
		}
	
	}
}

