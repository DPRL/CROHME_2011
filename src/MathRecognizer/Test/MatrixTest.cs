using System;
using DataStructures;

namespace Test
{
	public class MatrixTest
	{
		public MatrixTest () {
		}
		
		public void runTests() {
			Console.WriteLine( "Running tests ...\n" );
			
			bool result = testCreateMatrix() && testMatrixTranspose() && testMatrixMultiplication() && testMatrixAddition() && testMatrixInverse();
			
			Console.WriteLine( "\n... done." );
			if ( result ) {
				Console.WriteLine( "===\n[PASS] All tests passed." );
			} else {
				Console.WriteLine( "===\n[FAIL] One or more test cases failed." );	
			}
		}
		
		private bool testCreateMatrix() {
			Matrix A = new Matrix( 5, 5 );
			for ( int i = 0; i < 5; i++ ) {
				for ( int j = 0; j < 5; j++ ) {
					if ( A[ i, j ] != 0 ) {
						Console.Error.WriteLine( "Create zero nxn matrix failed: non-zero element." );
						return false;
					}
				}
			}
			
			Matrix B = new Matrix( 3, 8 );
			for ( int i = 0; i < 3; i++ ) {
				for ( int j = 0; j < 8; j++ ) {
					if ( B[ i, j ] != 0 ) {
						Console.Error.WriteLine( "Create zero mxn matrix failed: non-zero element." );
						return false;
					}
				}
			}
			
			Matrix C = new Matrix( new double[,] { { 2, 3 }, { 4, 5 } } );
			if ( C[ 0, 0 ] != 2 || C[ 0, 1 ] != 3 ||
			     C[ 1, 0 ] != 4 || C[ 1, 1 ] != 5 ) {
				Console.Error.WriteLine( "Create matrix with initial values failed." );
				return false;
			}
			
			Matrix D = new Matrix( C );
			if ( D[ 0, 0 ] != 2 || D[ 0, 1 ] != 3 ||
			     D[ 1, 0 ] != 4 || D[ 1, 1 ] != 5 ) {
				Console.Error.WriteLine( "Create matrix from other matrix failed." );
				return false;
			}
			
			Console.WriteLine( "[testCreateMatrix] PASS." );
			return true;
		}
		
		private bool testMatrixTranspose() {
			Matrix A = new Matrix( new double[,] { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 } } );
			Matrix AT = A.Transpose;
			for ( int i = 0; i < 9; i++ ) {
				if ( AT[ i % 3, i / 3 ] != i ) {
					Console.Error.WriteLine( "Matrix transpose failed." );
					return false;
				}
			}
			
			Console.WriteLine( "[testMatrixTranspose] PASS." );
			return true;
		}
		
		private bool testMatrixMultiplication() {
			Matrix A = new Matrix( new double[,] { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 } } );
			Matrix B = new Matrix( new double[,] { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 } } );
			Matrix AB = A * B;
			Matrix ABexp = new Matrix( new double[,] { { 15, 18, 21 }, { 42, 54, 66 }, { 69, 90, 111 } } );
			
			if ( !( AB.Equals( ABexp ) ) ) {
				Console.Error.WriteLine( "Matrix nxn * nxn multiplication failed." );
				Console.Error.WriteLine( "AB actual = {0}, AB expected = ", AB, ABexp );
				return false;
			}
			
			Console.WriteLine( "[testMatrixMultiplication] PASS." );
			return true;
		}
		
		private bool testMatrixAddition() {
			Matrix A = new Matrix( new double[,] { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 } } );
			Matrix B = new Matrix( new double[,] { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 } } );
			Matrix C = A + B;
			Matrix Cexp = new Matrix( new double[,] { { 0, 2, 4 }, { 6,  8, 10 }, { 12, 14, 16 } } );
			if ( !C.Equals( Cexp ) ) {
				Console.Error.WriteLine( "Matrix addition failed." );
				Console.Error.WriteLine( "A + B actual = {0}, A + B expected = ", C, Cexp );
				return false;	
			}
			
			Console.WriteLine( "[testMatrixAddition] PASS." );
			return true;
		}
		
		private bool testMatrixInverse() {
			
			// populate matrices for dimensions 2-100 with random values in [0,99]
			// compute inverses and check multiplication yields I
			Random rand = new Random();
			for ( int i = 2; i < 101; i++ ) {
				// Console.WriteLine( "[testMatrixInverse] dim = {0}", i );
				Matrix M = new Matrix( i, i );
				for ( int j = 0; j < i; j++ ) {
					for ( int k = 0; k < i; k++ ) {
						M[ j, k ] = rand.Next() % 100;
					}
				}
				double det;
				Matrix Minv = M.Inverse(out det);
				Matrix MMinv = M * Minv;
				Polarize( MMinv, i );
				if ( !MMinv.Equals( Matrix.Identity( i ) ) ) {
					Console.Error.WriteLine( "Matrix inverse ({0}x{0}) failed.", i );
					Console.Error.WriteLine( "M = {0}\nMinv = {1}\nMMinv = {2}", M, Minv, MMinv );
					return false;
				}
			}
			
			Console.WriteLine( "[testMatrixInverse] PASS." );
			return true;	
		}
		
		private void Polarize( Matrix m, int dim ) {
			for ( int i = 0; i < dim; i++ ) {
				for ( int j = 0; j < dim; j++ ) {
					double el = m[ i, j ];
					if ( el < 0.0000000001 ) m[ i, j ] = 0;
					if ( el > 0.9999999999 ) m[ i, j ] = 1;
				}
			}
		}
	}
}