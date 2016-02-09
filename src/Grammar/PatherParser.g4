parser grammar PatherParser;

options {
	output = AST;
	tokenVocab = PatherLexer;
}

root: group* EOF ;


group: GROUP ID OF LIST_EOL (LIST_WS path LIST_EOL)* LIST_EOL END PATHS_EOL;

path: LIST_WS?	
      DRIVE relativePath		#localPath
	| NET_SERVER relativePath	#remotePath
	;

relativePath: (SEP name?)*;

name: (namePart | interpolation)* ;

namePart: PATH_NAME_FRAGMENT ;

interpolation: INTERPOLATION_START EXPR INTERPOLATION_END ;