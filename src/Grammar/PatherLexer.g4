lexer grammar PatherLexer;

@lexer::members {
	private bool inPath = false;
}

TMP: ' ' -> channel(HIDDEN);

mode PATHS_FILE;
WS
	:	(' ' | '\r' | '\n') -> channel(HIDDEN)
	;

// PATHS_EOL: '\r'? '\n' ;
GROUP: 'group' ;
OF: 'of' -> mode(PATHS_LIST);
GROUP_ID: ~[ \n\r]+ ;

mode PATHS_LIST;
LIST_WS: [ \t] ;
LIST_EOL: '\r'? '\n' { inPath = false; } ;
END: 'end' -> mode(PATHS_FILE);

DRIVE: [a-zA-Z] ':' { inPath = true; } ;
NET_SERVER: '\\\\' ~[\\/]+ { inPath = true; } ;
SEP: '\\' | '/' ;

INTERPOLATION_START: '{' -> mode(EXPRESSION) ;

PATH_NAME_FRAGMENT: ~[\\/\n\r{]* { inPath }? ;


mode EXPRESSION;
EXPR_WS: [ \t] -> skip ;
INTERPOLATION_END: '}' -> mode(PATHS_LIST) ;

COLON: ':' ;
OP: ( '+' | '|' ) ;
LPAREN: '(' ;
RPAREN: ')' ;
COMMA: ',' ;

STRING: '\'' ~'\''* '\'' ;
NUMBER: '-'? [0-9]+ ('.' [0-9]+)? ;
ID: [a-zA-Z0-9_-]+ ;


