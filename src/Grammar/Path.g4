grammar Path;

/*
 * Parser Rules
 */

root:
	  DRIVE relativePath		
	| NET_SERVER relativePath	
	;

relativePath: (SEP NAME?)*;

/*
 * Lexer Rules
 */

WS
	:	' ' -> channel(HIDDEN)
	;

DRIVE: [a-zA-Z] ':' ;
SEP: '\\' | '/' ;
NAME: ~[\\/]+ ;

NET_SERVER: '\\\\' ~[\\/]+ ;