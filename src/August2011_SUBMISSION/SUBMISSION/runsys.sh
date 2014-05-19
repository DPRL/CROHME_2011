#!/bin/sh

GRAMMARFILE="Part2.Grammar.txt"
STATSFILE="part2.stats.csv"
PROCTIMEOUT=2
TOPN=1
PROBTHRESHOLD=0

##############################
### DO NOT EDIT BELOW THIS ###
##############################

THREADS=1
CFG="Release"
INPUTFILE=$1
OUTPUTFILE=$2
CLASSIFYURL="-"

if [ -z $1 ] || [ -z $2 ]
then
	echo "usage: runsys.sh INPUTFILE OUTPUTFILE"
	exit 1
fi

if [ ! -d tmp ]; then mkdir tmp; fi
if [ ! -e $INPUTFILE ]
then
	echo "Input file $INPUTFILE does not exist."
	exit 1
fi

INFILE_NAME=infile
echo $1 >  tmp/$INFILE_NAME
echo " INFILE_NAME: $INFILE_NAME"
echo " GRAMMARFILE: $CFG/$GRAMMARFILE"
echo " STATSFILE: $CFG/$STATSFILE"

$CFG/classify.exe $CFG/$GRAMMARFILE tmp/$INFILE_NAME tmp tmp/null $THREADS $PROCTIMEOUT $TOPN $CLASSIFYURL $PROBTHRESHOLD $CFG/$STATSFILE

BASE=`basename $INPUTFILE`
mv tmp/result.$BASE $2
rm tmp/$INFILE_NAME
