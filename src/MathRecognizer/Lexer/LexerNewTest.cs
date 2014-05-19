// Indiscriminate includes at this point.
using System;
using System.Collections.Generic;
using System.IO;
using DataStructures;
using Lexer;
using System.Collections;
using System.Text;


namespace Lexer {

	class LexerNewTest {
		static public List<Stroke> strokeList;

		public static void Main(string[] args)
	{

		if (args.Length != 4)
		{
			Console.WriteLine("Usage: ParserTest grammarFile inputInkML outputInkML segmentFile");
			return;
		}
		// Note syntax for embedding subsequent arguments in output string {<index}
		string grammarFile = args[0];
		string inputInkMLFile = args[1];
            string outputInkMLFile = args[ 2 ];
		string segmentFile = args[3];
			
		InkML inputFileML = InkML.NewFromFile(inputInkMLFile);
		if (inputFileML == null )
		{
			Console.Error.WriteLine("IO error opening file {0}", args[1]);
			return;
		}

		// The InkML type contains some iterable fields, e.g. over 'traces' (strokes)
		strokeList = new List<Stroke>();
		foreach (InkML.Trace tr in inputFileML.traces)
			strokeList.Add(tr.ToStroke());

		LexerMain lexer = new LexerMain( "part1.stats.csv", "http://saskatoon.cs.rit.edu:1500/", 0, segmentFile, strokeList);

	}
	}


}
