RIT DPRL CROHME 2011
---------------
DPRL CROHME 2011

Copyright (c) 2011-2014 Lei Hu, Richard Pospesel, Kevin Hart, Richard Zanibbi

This file is part of DPRL CROHME 2011.

DPRL CROHME 2011 is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

DPRL CROHME 2011 is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with DPRL CROHME 2011. If not, see <http://www.gnu.org/licenses/>.

Contact:
   - Lei Hu: lei.hu@rit.edu
   - Richard Pospesel: pospeselr@gmail.com
   - Kevin Hart: kth1775@rit.edu
   - Richard Zanibbi: rlaz@cs.rit.edu 

This document is about DPRL's submission for the [CROHME 2011]. CROHME is the abbreviation of Competition on Recognition of Online Handwritten Mathematical Expression. 

The DPRL CROHME 2011 has a feed-feedforward architecture.

For the segmentation, The time-ordered sequence of strokes for an expression is broken down into groups of three; for n strokes, the system produces a list of stroke subsets, i.e. S = ((s1, s2, s3), …, (sn-2, sn-1,sn)); if n is not divisible by 3, the final stroke subset may have only one or two strokes. For each three-stroke group, an HMM classifier is used to compute the highest classification probability for each possible segment, and then the segmentation that maximizes the harmonic mean of the resulting class probabilities is selected for the stroke group.

The HMM used for classification is the same as the paper [HMM-Based Recognition of Online Handwritten Mathematical Symbols Using Segmental K-means Initialization and a Modified Pen-up/down Feature]. The details about preprocessing, feature selection and HMM algorithm can be found in the paper. The source code about the HMM classifier can be found at [HMM classifier source code].

The spatial relation between symbols is determined using two probabilistic quadratic classifiers. The details can be found in the paper [Baseline Extraction-Driven Parsing of Handwritten Mathematical Expressions].

Finally, once symbols are segmented and recognized, the DRACULAE parser recovers the expression structure using default parameters. More details about the parser can be found at [DRACULAE parser].

How to run the codes?
----
To execute our system, issue: ./runsys.sh inputfile.inkml outputfile.inkml

The input and output inkml file is in the format of CROHME and the description of the data file format can be found at [CROHME data format].

To run multiple input files in batch, issue ./runsys.batch.sh INPUTFILELIST  [OUTPUTDIRECTORY] 

INPUTFILELIST is a plain-text file that has the names of input files, one file per line. It is suggested that the input files be moved to this directory (i.e. the same directory as the script) and then just the raw file names listed in this file, without any path prefix.

OUTPUTDIRECTORY is the path to the desired output directory. All output from the run will be put here. If it is not provided, a folder in the current directory will be created; the name will be a time/datestamp. Either way, the script will report the output directory for reference.

For the two scripts, compiled executable that is the Release entry point for running files on the system. Note that this CAN be called directly but a preferable method is to use the scripts provided. 

System parameters can be set with the variables at the top of the scripts.

GRAMMARFILE is the name of the grammar file. Assumed to be in the Release/ directory.
	
STATSFILE is the name of the (layout/adjacency) statistics file. Assumed to be in the Release/ directory.

PROCTIMEOUT is the timeout to set for a single thread. Any file for which system execution time exceeds this value will be aborted and reported as such.
	
TOPN is the value for the maximum number of classification possibilities to consider. Note that the system's run time increases greatly with higher values.
	
PROBTHRESHOLD is the value at which to prune classification results (i.e. potential classifications with a probability less than this value will be immediately discarded). Set this to 0 for no pruning.
	
RUNATONCE is the number of files to run at once (i.e. number of files in one "batch"). Note that the system tends to perform poorly if this value is too high; recommended values are RUNATONCE <= ~50. Reduce this value if batches are not completing due to memory issues.
	  
THREADS is the number of threads to utilize when running a single batch. If RUNATONCE is sufficiently small, this number will optimally equal RUNATONCE. It is suggested that THREADS not exceed 10, though this may vary depending on the specifications of the machine running the system.

[CROHME 2011]:http://ieeexplore.ieee.org/xpl/articleDetails.jsp?tp=&arnumber=6065557&queryText%3DCompetition+on+Recognition+of+Online+Handwritten

[DRACULAE parser]:http://ieeexplore.ieee.org/xpls/abs_all.jsp?arnumber=1046157&tag=1

[HMM-Based Recognition of Online Handwritten Mathematical Symbols Using Segmental K-means Initialization and a Modified Pen-up/down Feature]:http://ieeexplore.ieee.org/xpl/articleDetails.jsp?tp=&arnumber=6065353&queryText%3D%5BHMM-Based+Recognition+of+Online+Handwritten+Mathematical+Symbols+Using+Segmental+K-means+Initialization+and+a+Modified+Pen-up%2Fdown+Feature%5D

[HMM classifier source code]:https://github.com/DPRL/HMM_Math_Symbol_Classifier

[Baseline Extraction-Driven Parsing of Handwritten Mathematical Expressions]:http://ieeexplore.ieee.org/xpls/abs_all.jsp?arnumber=6460138&tag=1

 
 [CROHME data format]:http://www.isical.ac.in/~crohme/data2.html
 
 [label graph file format]:http://www.cs.rit.edu/~dprl/CROHMELib_LgEval_Doc.html
 
 [DPRL_Math_Symbol_Recs]:http://www.cs.rit.edu/~dprl/Software.html
