grammar LabCalculator;

/*
 * Parser Rules
 */
compileUnit
    : expression EOF
    ;

expression
    : LPAREN expression RPAREN                                # ParenthesizedExpr
    | operatorToken=(ADD | SUBTRACT) expression               # UnaryExpr
    | expression EXPONENT expression                          # ExponentialExpr
    | expression operatorToken=(MULTIPLY | DIVIDE | MOD | DIV) expression  # MultiplicativeExpr
    | expression operatorToken=(ADD | SUBTRACT) expression     # AdditiveExpr
    | NUMBER                                                  # NumberExpr
    | IDENTIFIER                                              # IdentifierExpr
    ;


/*
 * Lexer Rules
 */
NUMBER      : INT ('.' INT)? ;
IDENTIFIER  : [a-zA-Z]+ [0-9]+ ;
INT         : [0-9]+ ;
EXPONENT    : '^' ;
MULTIPLY    : '*' ;
DIVIDE      : '/' ;
MOD         : 'mod' ;
DIV         : 'div' ;
SUBTRACT    : '-' ;
ADD         : '+' ;
LPAREN      : '(' ;
RPAREN      : ')' ;
WS          : [ \t\r\n] -> skip ;