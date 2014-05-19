#!/bin/sh
# RZ: Modified for ICPR 2012.

GRAMMARFILE="Part1.Grammar.txt"
STATSFILE="part1.stats.csv"
PROCTIMEOUT=10
TOPN=1
PROBTHRESHOLD=0

##############################
### DO NOT EDIT BELOW THIS ###
##############################

THREADS=10
CFG="Release"
INPUTFILE=$1
OUTPUTFILE=$2
CLASSIFYURL="-"

if [ -z $1 ] || [ -z $2 ]
then
	echo "usage: runsys2.sh INPUTFILE OUTPUTFILE"
	exit 1
fi

if [ ! -d tmp ]; then mkdir tmp; fi
if [ ! -e $INPUTFILE ]
then
	echo "Input file $INPUTFILE does not exist."
	exit 1
fi

INFILE_NAME=infile.inkml
cat $1 >  tmp/$INFILE_NAME
echo " ORIGINALINPUT: $1"
echo " INFILE_NAME: tmp/$INFILE_NAME"
echo " GRAMMARFILE: $CFG/$GRAMMARFILE"
echo " STATSFILE: $CFG/$STATSFILE"
echo " SEGMENTFILE: tmp/$INFILE_NAME.seg"

# 2012: First, obtain segmentation.
# Ugly: this assumes current path of: /home/rlaz/icdar2011submission/MathRecognizer/ParserTest/bin
# Using unthreaded 'ParserTest' program here.
echo ""
echo "Running preliminary stroke segmentation..."
BASE=`basename $INPUTFILE .inkml`
python ../../../Segmenter/segmentation.py get_segmentations tmp tmp

$CFG/ParserTest.exe $CFG/$GRAMMARFILE tmp/$INFILE_NAME tmp/results.$BASE $CFG/$STATSFILE  tmp/infile.seg 

# Old (slightly modified from runsys.sh)
#$CFG/classify.exe $CFG/$GRAMMARFILE tmp/$INFILE_NAME tmp tmp/null $THREADS $PROCTIMEOUT $TOPN $CLASSIFYURL $PROBTHRESHOLD $CFG/$STATSFILE tmp/$INFILE_NAME.seg

if [ ! -e tmp/results.$BASE ]
then
	echo "Parse failure (tmp/results.$BASE not generated)"
else
	mv tmp/results.$BASE $2
fi

# Clean up intermediate files (input, segments)
rm tmp/$INFILE_NAME
rm tmp/infile.seg
