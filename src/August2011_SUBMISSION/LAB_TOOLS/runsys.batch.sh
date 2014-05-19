#!/bin/sh

GRAMMARFILE="Part2.Grammar.txt"
STATSFILE="part2.stats.csv"
PROCTIMEOUT=20
TOPN=2
PROBTHRESHOLD=0
RUNATONCE=5	# how many to run in each batch?
THREADS=5	# optimal if this is the same value as RUNATONCE
            # if sufficiently small

##############################
### DO NOT EDIT BELOW THIS ###
##############################

CFG="Release"
INPUTFILELIST=$1
CLASSIFYURL="-"
TMPDIR=`date +%m%d%y_%H%M%S`

if [ -z $1 ]
then
	echo "usage: runsys.sh INPUTFILELIST [OUTPUTDIR]"
	echo ""
	echo "NOTE: INPUTFILELIST should exist in the current directory."
	exit 1
fi

if [ ! -z $2 ]
then
	TMPDIR=$2
fi

if [ ! -e $INPUTFILELIST ]
then
	echo "Input file list $INPUTFILELIST does not exist."
	exit 1
fi

COMPLETECOUNT=0
TOTALCOUNT=`cat $INPUTFILELIST | wc -l`
BATCHNUM=1

mkdir $TMPDIR
echo "Output directory: $TMPDIR"

cp $INPUTFILELIST $TMPDIR/$INPUTFILELIST

while [ $COMPLETECOUNT -lt $TOTALCOUNT ]
do
	mkdir $TMPDIR/$BATCHNUM
	head $TMPDIR/$INPUTFILELIST -n $RUNATONCE > $TMPDIR/$BATCHNUM/.currentrunlist
	tail $TMPDIR/$INPUTFILELIST -n +$(($RUNATONCE+1)) > $TMPDIR/$INPUTFILELIST.new
	mv $TMPDIR/$INPUTFILELIST.new $TMPDIR/$INPUTFILELIST
	
	SN=$((COMPLETECOUNT+1))
	EN=$((COMPLETECOUNT+$RUNATONCE))
	if [ $EN -ge $TOTALCOUNT ]; then EN=$TOTALCOUNT; fi
	echo "Running $SN through $EN (batch $BATCHNUM) of $TOTALCOUNT ..."
	
	$CFG/classify.exe $CFG/$GRAMMARFILE $TMPDIR/$BATCHNUM/.currentrunlist $TMPDIR/$BATCHNUM $TMPDIR/$BATCHNUM/filelist.txt $THREADS $PROCTIMEOUT $TOPN $CLASSIFYURL $PROBTHRESHOLD $CFG/$STATSFILE > $TMPDIR/$BATCHNUM/classify.out 2> $TMPDIR/$BATCHNUM/classify.err
	
	BATCHNUM=$(($BATCHNUM+1))
	COMPLETECOUNT=$(($COMPLETECOUNT+$RUNATONCE))
done
