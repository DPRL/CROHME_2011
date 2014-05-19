using System;
using DataStructures;
using System.Collections.Generic;
using Lexer;
using Parser;

namespace ParserTest
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length < 4)
			{
				Console.WriteLine("Usage: ParserTest grammarFile inputInkML outputInkML adjStats [segmentFile]");
				return;
			}
			// Note syntax for embedding subsequent arguments in output string {<index}
			string grammarFile = args[0];
			string inputInkMLFile = args[1];
         		string outputInkMLFile = args[ 2 ];
			string adjStatsFile = args[3];
			string segmentSet = null;
			if (args.Length > 4)
				segmentSet = args[4];

			InkML inputFileML = InkML.NewFromFile(inputInkMLFile);
			if (inputFileML == null )
			{
				Console.Error.WriteLine("error opening file {0}", args[1]);
				return;
			}
			// The InkML type contains some iterable fields, e.g. over 'traces' (strokes)
			List<Stroke> strokeList = new List<Stroke>();
			foreach (InkML.Trace tr in inputFileML.traces)
				strokeList.Add(tr.ToStroke());

			
			//Console.WriteLine("****************** SECOND PARSE ************************");
			// port 1501: Part 1 (parallel implementation)
			// port 1503: Part 2 (parallel implementation)
			Grammar grammar = Grammar.Load(grammarFile);
			ParserMain secondParser = null;
			if (args.Length > 3)
				secondParser = new ParserMain(grammar, inputFileML, 1, 2, "http://saskatoon.cs.rit.edu:1501/", 0, adjStatsFile, segmentSet, strokeList );
			else
				// NOTE: currently the behavior for Next() and Start() has been modified (likely will not execute properly).
            		secondParser = new ParserMain(grammar, inputFileML, 1, 2, "http://saskatoon.cs.rit.edu:1501/", 0, adjStatsFile );
			secondParser.topLevelParser();
			
			//Console.WriteLine(secondParser.validParseTreeString(10));

			if (secondParser.validParses.Count > 0)
			{
				string annotationui = inputFileML.annotations.ContainsKey( "UI" ) ? inputFileML.annotations["UI"] : "NO_UI";
				//Console.WriteLine(secondParser.validParses[0].root.ToInkML( annotationui ) );
           		     	System.IO.File.WriteAllText( outputInkMLFile, secondParser.validParses[ 0 ].root.ToInkML( annotationui ) );

			}
			
			Console.WriteLine("Applied Rules: " + secondParser.apply_rule_counter);
			
			// inkml output
			//Console.WriteLine(secondParser.treeRoot.ToInkML());
			
			
		}
	}
}

