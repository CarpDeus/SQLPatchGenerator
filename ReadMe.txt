ReluctantDBA SQL Patch Generator
This is the source code to a utility designed to generate patch scripts for stored procedures and user defined functions.

It will basically create one of two different script segments, depending on what options are provided. 

New procedures/udfs will be in the format:
IF NOT EXISTS()
 	create

Updating existing procedures/udfs will be in the format
IF NOT EXISTS(date formatted name of object)
	rename existing object
	create new object
	