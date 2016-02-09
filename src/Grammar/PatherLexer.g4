lexer grammar PatherLexer;

@lexer::members {
	private bool inPath = false;
}

WS
	:	' ' -> channel(HIDDEN)
	;


mode PATHS_FILE;
PATHS_EOL: '\r'? '\n' ;
GROUP: 'group' ;
OF: 'of' -> mode(PATHS_LIST);
ID: ~[ \n\r]+ ;

mode PATHS_LIST;
LIST_WS: [ \t] ;
LIST_EOL: '\r'? '\n' { inPath = false; } ;
END: 'end' -> mode(PATHS_FILE);

DRIVE: [a-zA-Z] ':' { inPath = true; } ;
NET_SERVER: '\\\\' ~[\\/]+ { inPath = true; } ;
SEP: '\\' | '/' ;

INTERPOLATION_START: '{' -> mode(EXPRESSION) ;

PATH_NAME_FRAGMENT: ~[\\/\n\r{]+ { inPath }? ;


mode EXPRESSION;
INTERPOLATION_END: '}' -> mode(PATHS_LIST) ;
EXPR: ~'}'+ ;

//LINE: ~[\n\r\t ] ~[\n\r]* ;