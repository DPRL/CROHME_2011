using System;
using DataStructures;
using System.Collections.Generic;
namespace Test
{
	public class QDCTest
	{
		private Random rand;
		
		public QDCTest ()
		{
			rand = new Random();
		}
		
		public void runTest() {			
			// create random data sets
			List< Matrix > ClassA = new List< Matrix >();
			List< Matrix > ClassB = new List< Matrix >();
			
			for ( int i = 0; i < 50; i++ ) {
				ClassA.Add( NewClassAMatrix() );
				ClassB.Add( NewClassBMatrix() );
			}
			
			QDC qdc = new QDC( new List< Matrix >[] { ClassA, ClassB }, new string[] { "CLASS_A", "CLASS_B" } );
			
			// classify a bunch of class A stuff
			for ( int i = 0; i < 10; i++ ) {
				Matrix a = NewClassAMatrix();
				double[] probs = qdc.class_probabilities( a );
				Console.WriteLine( a );
				Console.WriteLine( "[A]: {0} ({1})\t{2} ({3})", 'a', probs[ 0 ], 'b', probs[ 1 ] );
			}
			
			// classify a bunch of class B stuff
			for ( int i = 0; i < 10; i++ ) {
				Matrix b = NewClassBMatrix();
				double[] probs = qdc.class_probabilities( b );
				Console.WriteLine( b );
				Console.WriteLine( "[B]: {0} ({1})\t{2} ({3})", 'a', probs[ 0 ], 'b', probs[ 1 ] );
			}
		}
		
		private double RandomInRange( int start, int end ) {
			return ( rand.NextDouble() * ( end - start ) ) + start;
		}
		
		private Matrix NewClassAMatrix() {
			return new Matrix( new double[,] { { RandomInRange( 5, 7 ) }, { RandomInRange( 5, 7 ) }, { RandomInRange( 5, 7 ) } } );
		}
		
		private Matrix NewClassBMatrix() {
			return new Matrix( new double[,] { { RandomInRange( -7, -5 ) }, { RandomInRange( -7, -5 ) }, { RandomInRange( -7, -5 ) } } );
		}
	}
}

