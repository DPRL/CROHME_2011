August2011_SUBMISSION: README

CONTENTS
---
1. INTRODUCTION.
2. FILE AND FOLDER DESCRIPTIONS.


1. INTRODUCTION.

This folder contains files that compose our submission for the ICDAR 2011
CROHME competition. It contains compiled executable as well as different
scripts for running input data through the system.


2. FILE AND FOLDER DESCRIPTIONS.

	- classify_081211_1705.zip
		Compiled executable that is the main entry point for running files on
		the system. Note that this CAN be called directly but a preferable
		method is to use the scripts provided. Also note that the contents of
		this file are duplicated in both SUMBISSION/ and LAB_TOOLS/.
	
	- README
		This file.
	
	- LAB_TOOLS
		Provides a script for running input files in batch. Handles limitations
		of the system by running only a set number of files at a time. Read the
		documentation in this folder for more information.
	
	- SUBMISSION
		Provides a script for running a single input file and naming the output
		file. This is the script as required for the competition.