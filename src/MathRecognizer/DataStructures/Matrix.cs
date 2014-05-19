using System;
using System.Text;
using System.Collections.Generic;

namespace DataStructures
{
	public class Matrix
	{
		
		#region private_data
		
		int rows;
		int columns;
		
		public int Rows
		{
			get
			{
				return rows;	
			}
		}
		public int Columns
		{
			get
			{
				return columns;	
			}
		}
		
		double[,] data;
	
		#endregion
		
		// constructor
		
		public Matrix (int in_rows, int in_columns)
		{
			rows = in_rows;
			columns = in_columns;
			data = new double[rows,columns];
		}
		
		// copy constructor
		public Matrix(Matrix other)
		{
			rows = other.rows;
			columns = other.columns;
			data = new double[rows,columns];
			for(int i = 0; i < rows; i++)
				for(int j = 0; j < columns; j++)
					data[i,j] = other[i,j];
		}
		
		public Matrix(double[,] in_data)
		{
			rows = in_data.GetLength(0);
			columns = in_data.GetLength(1);
			data = new double[rows, columns];
			for(int i = 0; i < rows; i++)
				for(int j = 0; j < columns; j++)
					data[i,j] = in_data[i,j];
		}
		
		// index operator
		
		public double this[int i, int j]
		{
			get
			{
				return data[i,j];
			}
			set
			{
				data[i,j] = value;	
			}
		}
		
		// cast operator (for 1x1 matrix)
		
		public static explicit operator double(Matrix m)
		{
			if(m.rows == 1 && m.columns == 1)
				return m.data[0,0];
			throw new Exception("Cannot Cast " + m.rows + "x" + m.columns + " Matrix to Scalar");
		}
		
		// multiply operator
		
		public static Matrix operator*(Matrix left, Matrix right)
		{	
			if(left.columns != right.rows)
				throw new Exception("Cannot multiply " + left.rows + "x" + left.columns + " Matrix by a " + right.rows + "x" + right.columns + " Matrix");
			
			Matrix result = new Matrix(left.rows, right.columns);
			
			for(int i = 0; i < left.rows; i++)
			{
				for(int j = 0; j < right.columns; j++)
				{
					double sum = 0.0;
					for(int k = 0; k < left.columns; k++)
					{
						sum += left[i,k] * right[k,j];	
					}
					result[i,j] = sum;
				}
			}
			
			return result;
		}
	
		public static Matrix operator*(double scalar, Matrix mat)
		{
			Matrix result = new Matrix(mat);
			for(int i = 0; i < result.rows; i++)
				for(int j = 0; j < result.columns; j++)
					result[i,j] *= scalar;
			return result;
		}
		
		public static Matrix operator*(Matrix mat, double scalar)
		{
			Matrix result = new Matrix(mat);
			for(int i = 0; i < result.rows; i++)
				for(int j = 0; j < result.columns; j++)
					result[i,j] *= scalar;
			return result;
		}
		
		// addition operator
		
		public static Matrix operator+(Matrix left, Matrix right)
		{
			if(left.rows != right.rows || left.columns != right.columns)
			{
				throw new Exception("Cannot add " + left.rows + "x" + left.columns + " Matrix to " + right.rows + "x" + right.columns + " Matrix");	
			}
			
			Matrix result = new Matrix(left.rows, left.columns);
			
			for(int i = 0; i < left.rows; i++)
				for(int j = 0; j < left.columns; j++)
					result[i,j] = left[i,j] + right[i,j];
			return result;
		}
		
		// subtraction operator
		
		public static Matrix operator-(Matrix left, Matrix right)
		{
			if(left.rows != right.rows || left.columns != right.columns)
			{
				throw new Exception("Cannot subtract " + right.rows + "x" + right.columns + " Matrix from " + left.rows + "x" + left.columns + " Matrix");	
			}
			
			Matrix result = new Matrix(left.rows, left.columns);
			
			for(int i = 0; i < left.rows; i++)
				for(int j = 0; j < left.columns; j++)
					result[i,j] = left[i,j] - right[i,j];
			return result;			
		}
		
		// transpose property
		
		public Matrix Transpose
		{
			get
			{
				Matrix result = new Matrix(columns, rows);
				for(int i = 0; i < columns; i++)
					for(int j = 0; j < rows; j++)
						result[i,j] = this[j,i];
				return result;
			}
		}
		
		// code ripped from JAMA C++ linear algebra library
		private static double hypot(double a, double b)
		{
			return Math.Sqrt(a*a + b*b);
		}
		public Matrix Inverse(out double AbsDeterminant)
		{
			// first do a QR decomposition
			Matrix QR_ = new Matrix(this);
			int m = rows;
			int n = columns;
			double[] Rdiag = new double[n];
			int i = 0, j = 0, k = 0;
			
			
			for(k = 0; k < n; k++)
			{
				// Compute 2-norm of the k-th column without under/overflow	
				double nrm = 0.0;
				for(i = k; i < m; i++)
					nrm  = hypot(nrm, QR_[i,k]);
				
				if(nrm != 0.0)
				{
					// Form k0th Householder vector.
					if(QR_[k,k] < 0)
						nrm *= -1;
					for(i = k; i < m; i++)
						QR_[i,k] /= nrm;
					QR_[k,k] += 1.0;
					
					// Apply transformation to remaining columns
					for(j = k+1; j < n; j++)
					{
						double s = 0.0;
						for(i = k; i < m; i++)
							s += QR_[i,k] * QR_[i,j];
						s = -s / QR_[k,k];
						for(i = k; i < m; i++)
							QR_[i,j] += s * QR_[i,k];
					}
				}
				Rdiag[k] = -nrm;
			}
			
			// calculate |det| of this matrix
			AbsDeterminant = 1.0;
			foreach(double d in Rdiag)
				AbsDeterminant *= d;
			AbsDeterminant = Math.Abs(AbsDeterminant);
			
			// solve for identity
			int nx = m;
			Matrix X = new Matrix(m,m);
			for(i = 0; i < X.rows; i++)
				for(j = 0; j < X.columns; j++)
					X[i,j] = i == j ? 1.0 : 0.0;		
			
			i = 0; j = 0; k = 0;
			
			// Compute Y = transpose(Q)*B
			for(k = 0; k < n; k++)
			{
				for(j = 0; j < nx; j++)
				{
					double s = 0.0;
					for(i = k; i < m; i++)
						s += QR_[i,k] * X[i,j];	
					s = -s/QR_[k,k];
					for(i = k; i < m; i++)
						X[i,j] += s*QR_[i,k];
				}
			}
			// Solve R*X = Y;
			for(k = n-1; k >= 0; k--)
			{
				for(j = 0; j < nx; j++)
					X[k,j] /= Rdiag[k];
				for(i = 0; i < k; i++)
					for(j = 0; j < nx; j++)
						X[i,j] -= X[k,j] * QR_[i,k];
			}
			
			Matrix X_ = new Matrix(n, nx);
			for(i = 0; i < n; i++)
				for(j = 0; j < nx; j++)
					X_[i,j] = X[i,j];
			
			return X_;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "\n[" );
			
			for ( int i = 0; i < data.GetLength( 0 ); i++ ) {
				for ( int j = 0; j < data.GetLength( 1 ); j++ ) {
					sb.AppendFormat( "{0,7} ", data[ i, j ] );
				}
				sb.AppendLine();
			}
			
			sb.AppendLine( "]" );
			return sb.ToString();
		}
		
		public static Matrix Mean(List<Matrix> matrices, int rows, int columns)
		{
			if(matrices.Count == 0)
				throw new Exception("No Matrices in passed in List");
			
			Matrix S = new Matrix(matrices[0]);
			Matrix C = new Matrix(rows, columns);
			
			// uses Kahan summation
			for(int k = 1; k < matrices.Count; k++)
			{
				if(matrices[k].rows != rows || matrices[k].columns != columns)
					throw new Exception("Inconsistent Matrix size");
				
				Matrix Y = matrices[k] - C;
				Matrix T = S + Y;
				C = (T - S) - Y;
				S = T;
			}
			
			for(int i = 0; i < rows; i++)
				for(int j = 0; j < columns; j++)
					S[i,j] /= matrices.Count;
			
			return S;
		}

		public override bool Equals( object obj )
		{
			if ( !( obj is Matrix ) ) return false;
			Matrix m = obj as Matrix;
			if ( m.columns != columns || m.rows != rows ) return false;
			for ( int i = 0; i < rows; i++ ) {
				for ( int j = 0; j < rows; j++ ) {
					if ( this[ i, j ] != m[ i, j ] ) return false;	
				}
			}
			return true;
		}
		
		public static Matrix Identity( int dim ) {
			Matrix m = new Matrix( dim, dim );
			for ( int i = 0; i < dim; i++ ) m[ i, i ] = 1;
			return m;
		}
	}
}

