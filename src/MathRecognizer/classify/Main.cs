using System;
using DataStructures;
using System.Collections.Generic;
using System.IO;

using System.Threading;
using Parser;
using System.ComponentModel;
using System.Text;


namespace classify
{
	class MainClass
	{
		static int thread_count_default = 10;
		static int top_n = 3;
		static int neighbors_default = 2;
        static int proc_timeout_default = 5; // minutes
        static double prob_threshold_default = 0;
        static string classify_url_default = "http://saskatoon.cs.rit.edu:1500/";

		public delegate void run_parser(ParserMain pm, out InkML inkml);

		public static void Main (string[] args)
		{
		
			if(args.Length < 10)
			{
				Console.WriteLine("Usage: classify grammar.txt filelist.txt output_dir result_list.txt");
				Console.WriteLine("  grammar.txt         String - filename for grammar to use");        
				Console.WriteLine("  filelist.txt        String - list of files");
				Console.WriteLine("  output_dir          String - destination to save result");
				Console.WriteLine("  result_list.txt     String - file to save pairs of inputs/outputs for evaluation");
                Console.WriteLine("  thread_count           int - number of threads to use for execution");
                Console.WriteLine("  proc_timeout           int - maximum time a thread can run (in minutes)");
                Console.WriteLine("  neighbors (k)          int - number to use when doing k-nearest-neighbor analyses");
                Console.WriteLine("  classify_url        String - url of classifier (use \"-\" for default, saskatoon)");
                Console.WriteLine("  prob_threshold      double - classification probability pruning threshold");
                Console.WriteLine("  layout_stats_file   String - filename for stats file to use");
				return;
			}



			string grammar_txt = args[0];
			string file_list = args[1];
			string output_dir = Path.GetFullPath(args[2]);
			string result_list = args[3];

            string thread_count_str = args[ 4 ];
            int thread_count;
            if ( !int.TryParse( thread_count_str, out thread_count ) ) {
                Console.Error.WriteLine( "Could not parse argument thread_count, using default value of {0}.", thread_count_default );
                thread_count = thread_count_default;
            }

            string proc_timeout_str = args[ 5 ];
            int proc_timeout;
            if ( !int.TryParse( proc_timeout_str, out proc_timeout ) ) {
                Console.Error.WriteLine( "Could not parse argument proc_timeout, using default value of {0}.", proc_timeout_default );
                proc_timeout= proc_timeout_default;
            }

            string neighbors_str = args[ 6 ];
            int neighbors;
            if ( !int.TryParse( neighbors_str, out neighbors ) ) {
                Console.Error.WriteLine( "Could not parse argument neighbors, using default value of {0}.", neighbors_default );
                neighbors = neighbors_default;
            }

            string classify_url = args[7];
            if ( classify_url == "-" ) classify_url = classify_url_default;

            string prob_threshold_str = args[8];
            double prob_threshold;
            if ( !double.TryParse( prob_threshold_str, out prob_threshold ) ) {
                Console.Error.WriteLine( "Could not parse argument prob_threshold, using default value of {0}.", prob_threshold_default );
                prob_threshold = prob_threshold_default;
            }

            string stats_file = args[ 9 ];

			Grammar grammar = Grammar.Load(grammar_txt);
			
			List<string> input_files = new List<string>();
			using (FileStream fs = new FileStream(file_list, FileMode.Open))
			{
				StreamReader sr = new StreamReader(fs);
				for (string s = sr.ReadLine(); s != null; s = sr.ReadLine())
					input_files.Add(s);
			}

			Semaphore sema = new Semaphore(thread_count, thread_count);

			Dictionary<string, string> inputfile_to_output_inkml = new Dictionary<string,string>();

			// number of files completed
			int complete_counter = 0;

			for (int k = 0; k < input_files.Count; k++)
			{
				string filename = input_files[k];
				Console.WriteLine("Evaluating " + filename);	
				sema.WaitOne();	
				
				Thread thread = new Thread(delegate()
					{
						InkML inkml_file = InkML.NewFromFile( filename );
						ParserMain pm = new ParserMain( grammar, inkml_file, top_n, neighbors, classify_url, prob_threshold, stats_file );
						Thread threadToKill = null;
						Action action = delegate()
							{
								threadToKill = Thread.CurrentThread;
								pm.topLevelParser();
							};
						IAsyncResult async_result = action.BeginInvoke(null, null);

						// wait minutes
						if ( async_result.AsyncWaitHandle.WaitOne( proc_timeout * 60 * 1000 ) ) // proc_timeout is in minutes
						{
							action.EndInvoke(async_result);
							
							if (pm.validParses.Count > 0)
							{
								string inkml_string = pm.validParses[0].root.ToInkML(inkml_file.annotations["UI"]);
								string new_inkml = output_dir + Path.DirectorySeparatorChar + "result." + Path.GetFileName(filename);

								inputfile_to_output_inkml.Add(filename, new_inkml);

								using (FileStream fs = new FileStream(new_inkml, FileMode.Create))
								{
									StreamWriter sw = new StreamWriter(fs);
									sw.Write(inkml_string);
									sw.Close();
								}

								Console.WriteLine("Done with " + filename + "; valid parse:\n" + pm.validParseTreeString(1));
							}
							else
								Console.WriteLine("Done with " + filename + "; invalid parse");
							
						}
						else
						{
							threadToKill.Abort();
							Console.WriteLine("Done with " + filename + "; aborted");
						}
						sema.Release();
						Interlocked.Increment(ref complete_counter);
					});
				//thread.
				thread.Start();
				while (thread.IsAlive == false) ;
			}
			
			while (complete_counter != input_files.Count) Thread.Sleep(100);


			using (FileStream fs = new FileStream(result_list, FileMode.Create))
			{
				StreamWriter sw = new StreamWriter(fs);
				foreach (string filename in input_files)
				{
					string new_inkml;
					if (inputfile_to_output_inkml.TryGetValue(filename, out new_inkml))
					{
						sw.Write(filename);
						sw.Write('\t');
						sw.WriteLine(new_inkml);
					}
				}
				sw.Close();
			}


		}
	}
}


