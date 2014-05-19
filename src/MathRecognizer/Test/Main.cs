using System;
using DataStructures;
using System.Collections.Generic;
using Lexer;
using Parser;
using System.IO;

namespace Test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			//Grammar gram = Grammar.Load("Part1.Grammar.txt");
			/*
			var a = gram.GetProductions("*FRAC");
			var b = gram.GetRegionLayouts(a[0][0]);
			var c = gram.GetRegionLayouts("Descender");
			var d = gram.GetRegionLayouts("y");
			var e = gram.GetRegionLayouts("-");

			var f = gram.NonTerminalCanGenerateTerminal("S", "a");
			var g = gram.NonTerminalCanGenerateTerminal("PAR", "a");
			var h = gram.NonTerminalCanGenerateTerminal("*FUNC", "\\phi");
			var i = gram.NonTerminalCanGenerateTerminal("T", "\\sin");
			 */
			Console.WriteLine();
			/*(
			// new MatrixTest().runTests();
			
			// new QDCTest().runTest();
			
			InkML file = InkML.NewFromFile( args[ 0 ] );
			if ( file == null ) {
				Console.Error.WriteLine( "error: file null ({0})", args[ 0 ] );
				return;
			}
			LBT tree = new LBT( file, LBT.DefaultAdjacentCriterion );
			
			LexerMain lex = new LexerMain( "part1.stats.csv", null );
			new ParserMain( "Part1SymbolClasses", "~/Documents/ICDAR2011/train/CROHME_training/formulaire001-equation001.inkml" );
			
			LexerResult lr1 = lex.Start( tree, 2 )[ int.Parse( args[ 1 ] ) ];
			SymbolClass sc1 = SymbolClass.Unknown;
			
			switch ( ParserMain.symbolLayoutDict[ lr1.segment.classification[ 0 ].symbol ][ 0 ] ) {
				case "ASCENDER":
					sc1 = SymbolClass.Ascender;
					break;
				case "CENTERED":
					sc1 = SymbolClass.Centered;
					break;
			}
			
			string ls1id = lex.LeftmostAdjacent( lr1.lbt, lr1.segment, sc1 );
			Console.WriteLine( ls1id );
			
			/*
			// sqrt
			tree = new LBT( lr1.lbt );			
			for ( int i = 0; i < 3; i++ ) tree.PruneNode( tree.strokes[ 0 ], LBT.DefaultAdjacentCriterion );
			LexerResult lr2 = lex.Start( tree, 2 )[ 0 ];
			
			var r = lex.Partition( lr1.lbt, lr1.segment, lr2.segment, PartitionClass.SquareRoot, PartitionClass.SquareRoot );
			
			//float writingLine = lex.GetYWritingLine( y );
			//Console.WriteLine( "writing line = {0}", writingLine );
			// lex.Start( tree, 3 );
			*/
		}
	}
}

