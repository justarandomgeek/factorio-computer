%namespace compiler
%partial
%union {
			internal int iVal;
			internal string sVal;
			internal bool bVal;

			internal CompSpec compVal;
}

%token <iVal> INTEGER
%token <sVal> STRING SIGNAL
%token <sVal> UNDEF TYPENAME FIELD FUNCNAME VAR INTVAR ARRAY INTARRAY
%token <iVal> REGISTER
%token <compVal> COMPARE
%token <bVal> COND

%token ASSIGN APPEND
%token DO WHILE IF ELSE END
%token TYPE FUNCTION RETURN



%start program
%%
program: definition {};
program: program definition {};




definition: functiondef;
definition: datadef;
definition: typedef;

functiondef: FUNCTION UNDEF '(' paramdeflist ')' block END {};

paramdeflist: ;
paramdeflist: paramdef;
paramdeflist: paramdeflist ',' paramdef;
paramdef: TYPENAME UNDEF; //typename undef


datadef: TYPENAME '@' INTEGER  UNDEF { CreateMemVar($1,$4,$3); };
datadef: TYPENAME '@' REGISTER UNDEF { CreateRegVar($1,$4,$3); };
datadef: TYPENAME              UNDEF { CreateMemVar($1,$2);    };

typedef: TYPE UNDEF '{' fielddeflist '}'     { RegisterType($2,null); };
typedef: TYPE UNDEF '{' fielddeflist ',' '}' { RegisterType($2,null); }; // allow trailing comma

fielddeflist: fielddef;
fielddeflist: fielddeflist ',' fielddef ;
fielddef: '@' FIELD UNDEF { Console.WriteLine("field: {0} {1}", $3, $2); };
fielddef:           UNDEF { Console.WriteLine("field: {0}", $1); };


block: vassign;
block: sassign;
block: statement {};
block: datadef {};
block: block block {};
block: ifblock{};
block: whileblock{};


ifcomp: sexpr COMPARE sexpr   ;
ifcomp: sexpr                 ;

ifblock: IF ifcomp block elseblock END {};
elseblock: ELSE block { $$ = $2; };
elseblock: {};

whileblock: WHILE ifcomp DO block END {};

statement: RETURN returnlist {};
returnlist: ;

arith: '+'|'-'|'*'|'/' {};

sexpr: sexpr arith sexpr {};
sexpr: INTEGER {};
sexpr: sref {};
//sexpr: sum(vexpr)
sexpr: '&' VAR {};
sexpr: '&' ARRAY {};
sexpr: '&' ARRAY '[' sexpr ']' {};
sexpr: vexpr '[' sexpr ']' ;
sexpr: FUNCNAME '(' ')';

vexpr: vexpr arith vexpr {};
vexpr: vexpr arith sexpr {};
vexpr: vexpr '&' vexpr {};
vexpr: '{' '}'; //TODO: table literals
vexpr: '*' sexpr {};
vexpr: STRING {};
vexpr: vref {};
vexpr: FUNCNAME '(' ')'; //TODO: function arguments

sref: vref '.' {/*ExpectFieldType=nqltypeof($1)*/} FIELD {};
sref: INTVAR;

vref: VAR;
vref: REGISTER;

vassign: vref ASSIGN vexpr ;
vassign: vref APPEND vexpr ;

sassign: sref ASSIGN sexpr ;
sassign: sref APPEND sexpr ;

// */
