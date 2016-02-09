grammar Path;


root:
	  DRIVE relativePath		
	| NET_SERVER relativePath	
	;

relativePath: (SEP name?)*;

name: 
	(NAME_CHAR | interpolation)* ;

interpolation: '${' EXPR '}' ;

/*
 * Lexer Rules
 */

WS
	:	' ' -> channel(HIDDEN)
	;

DRIVE: [a-zA-Z] ':' ;
SEP: '\\' | '/' ;
//SIMPLE_NAME: ~[\\/]+ ;
NAME_CHAR: [\\/] ;

NET_SERVER: '\\\\' ~[\\/]+ ;
EXPR: ~'}' ;

