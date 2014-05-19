using System;
using System.Text;
namespace DataStructures
{
	public class ConfusionMatrix
	{
		int count;
		int classes;
		int[,] data;
		
		public ConfusionMatrix (int in_classes)
		{
			classes = in_classes;
			count = 0;
			data = new int[classes, classes];
		}
		
		public void Add(int actual, int predicted)
		{
			data[actual,predicted]++;
			count++;
		}
		
		int TruePositive(int c)
		{
			return data[c,c];	
		}
		
		int TrueNegative(int c)
		{
			int tn = 0;
			for(int i = 0; i < classes; i++)
				for(int j = 0; j < classes; j++)
					if(i == c || j == c) continue;
					else tn += data[i,j];
			return tn;
		}
		
		int FalsePositive(int c)
		{
			int fp = 0;
			for(int i = 0; i < classes; i++)
				if(i != c) fp += data[i,c];
			return fp;
		}
		
		int FalseNegative(int c)
		{
			int fn = 0;
			for(int j = 0; j < classes; j++)
				if(j != c) fn += data[c,j];
			return fn;
		}
		
		double Accuracy()
		{
			int ac = 0;
			for(int i = 0; i < classes; i++)
				ac += data[i,i];
			return ac / (double)count;
		}
		
		double Precision(int c)
		{
			int tp = TruePositive(c);
			return (double)tp/(double)(tp + FalsePositive(c));
		}
		
		double Recall(int c)
		{
			int tp = TruePositive(c);
			return (double)tp/(double)(tp + FalseNegative(c));
		}
		
		double Specificity(int c)
		{
			int tn = TrueNegative(c);
			return (double)tn/(double)(tn + FalsePositive(c));
		}
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("ConfusionMatrix:");
			for(int i = 0; i < classes; i++)
			{
				for(int j = 0; j < classes; j++)
					sb.Append(data[i,j]).Append(',');
				sb.AppendLine();
			}
			sb.AppendLine();
			
			
			sb.Append("Accuracy:,").Append(Accuracy()).AppendLine(",");
			sb.Append("Precision:,");
			for(int i = 0; i < classes; i++)
				sb.Append(Precision(i)).Append(",");
			sb.AppendLine();
			sb.Append("Recall:,");
			for(int i = 0; i < classes; i++)
				sb.Append(Recall(i)).Append(",");
			sb.AppendLine();
			sb.Append("Specificity:,");
			for(int i = 0; i < classes; i++)
				sb.Append(Specificity(i)).Append(",");
			sb.AppendLine();
			return sb.ToString();
		}
	}
}

