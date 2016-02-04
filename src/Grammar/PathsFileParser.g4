parser grammar PathsFileParser;

options {
	output=AST;
	tokenVocab=PathsFileLexer;
}

compileUnit: group* ;

group: GROUP NAME OF IN_EOL line* END IN_EOL ;

line returns [string text]: IN_WS* LINE IN_WS* IN_EOL { $text = $LINE.text.Trim(); };