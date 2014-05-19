using System;
using System.Collections.Generic;
namespace DataStructures
{
	public class QDC
	{
		// expects each data matrix to be a column vecctor
		// notation from Kuncheva
		Matrix[] mu;
		Matrix[] Sigma;
		Matrix[] SigmaInverse;
		double[] SigmaDeterminant;
		
		// factors
		double[] w0;
		Matrix[] w;
		Matrix[] W;
		
		int classes;
		
		int data_dimension;
		
		public QDC (List<Matrix>[] data, string[] labels)
		{
			if(data.Length != labels.Length)
				throw new Exception("Inconsistent number of data-sets and labels");
			
			if(data[0][0].Columns != 1)
				throw new Exception("Data points must be Column Vectors");
			
			data_dimension = data[0][0].Rows;
			
			classes = data.Length;
			
			
			mu = new Matrix[classes];
			Sigma = new Matrix[classes];
			SigmaInverse = new Matrix[classes];
			SigmaDeterminant = new double[classes];
			
			
			w0 = new double[classes];
			w = new Matrix[classes];
			W = new Matrix[classes];
			
			for(int i = 0; i < classes; i++)
			{
				// first calculate means 
				mu[i] = Matrix.Mean(data[i], data_dimension, 1); 
				//Console.WriteLine("MU");
				//Console.WriteLine(mu[i]);
				
				// now calculate covariance matrices
				List<Matrix> temp = new List<Matrix>();
				foreach(Matrix mat in data[i])
				{
					Matrix diff = mat - mu[i];
					temp.Add(diff * diff.Transpose);
				}
				
				Sigma[i] = Matrix.Mean(temp, data_dimension, data_dimension);
				//Console.WriteLine("SIGMA");
				//Console.WriteLine(Sigma[i]);
				
				
				// inverse covariance
				SigmaInverse[i] = Sigma[i].Inverse(out SigmaDeterminant[i]);
				//Console.WriteLine("SIGMA INVERSE");
				//Console.WriteLine(SigmaInverse[i]);
				//Console.WriteLine("SIGMA DETERMINANT");
				//Console.WriteLine(SigmaDeterminant[i]);
				// calculate these parameters
				w0[i] = Math.Log(1.0 / classes)
					- (0.5 * (double)(mu[i].Transpose * SigmaInverse[i] * mu[i]))
					- 0.5 * Math.Log(SigmaDeterminant[i]);
				w[i] = (SigmaInverse[i] * mu[i]).Transpose;
				W[i] = -0.5 * SigmaInverse[i];
				
//				Console.WriteLine("w0");
//				Console.WriteLine(w0[i]);
//				Console.WriteLine("w");
//				Console.WriteLine(w[i]);
//				Console.WriteLine("W");
//				Console.WriteLine(W[i]);
//				Console.WriteLine();
			}
		}
		
		// vector is a column vector
		public double[] class_probabilities(Matrix vector)
		{
			if(vector.Rows != data_dimension)
				throw new Exception("Given Vector has incorrect dimensionality");
			
			double[] result = new double[classes];
			
			for(int i = 0; i < classes; i++)
			{
				result[i] = w0[i] + (double)(w[i] * vector) + (double)(vector.Transpose * W[i] * vector);
			}
			
			return result;
		}
		
		public int classify(Matrix vector)
		{
			double[] probs = class_probabilities(vector);
			int max = 0;
			for(int k = 1; k < probs.Length; k++)
				if(probs[max] < probs[k])
					max = k;
			return max;
		}
	}
}

