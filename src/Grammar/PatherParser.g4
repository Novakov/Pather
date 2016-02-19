parser grammar PatherParser;

options {
	output = AST;
	tokenVocab = PatherLexer;
}

root: group* EOF ;


group: GROUP GROUP_ID OF LIST_EOL (LIST_WS path LIST_EOL)* LIST_EOL END PATHS_EOL;

path: LIST_WS?	
      DRIVE relativePath		#localPath
	| NET_SERVER relativePath	#remotePath
	;

relativePath: (SEP name?)*;

name: (namePart | interpolation)* ;

namePart: PATH_NAME_FRAGMENT ;

interpolation: INTERPOLATION_START expression INTERPOLATION_END ;

expression: 
      ID expressionList?                         #functionCall
    | left=expression op=OP right=expression     #binaryExpression      
    | simpleExpression                           #wrappedSimpleExpression
    ;

simpleExpression:
      LPAREN expression RPAREN                      #parenthesesExpression     
    | scope=ID COLON valueName=(ID | STRING)        #valueReferenceExpression
    | STRING                                        #stringConstant
    | NUMBER                                        #numberConstant   
    ;

expressionList: simpleExpression* ;
