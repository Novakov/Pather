lexer grammar PathsFileLexer;


WS:	' ' -> skip ;

EOL: '\r'? '\n' ;

GROUP: 'group';
OF: 'of' -> mode(INSIDE_GROUP) ;

NAME: [a-zA-Z0-9]+ ;

mode INSIDE_GROUP;
IN_WS:	(' ' | '\t') -> channel(HIDDEN) ;

IN_EOL: '\r'? '\n' ;

END: 'end' -> mode(DEFAULT_MODE) ;

LINE: ~[\n\r]* ;