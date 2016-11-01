%namespace compiler
%partial
%union {
  internal int iVal;
  internal string sVal;
  internal bool bVal;

  internal CompSpec compVal;
  internal TypeInfo tiVal;
  internal FieldInfo fiVal;
  internal SymbolList slVal;
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

%type <tiVal> fielddeflist
%type <fiVal> fielddef paramdef

%type <slVal> paramdeflist


%start program
%%
program: definition {};
program: program definition {};




definition: functiondef;
definition: datadef;
definition: typedef;

functiondef: FUNCTION UNDEF '(' paramdeflist ')' {BeginFunction($2,$4);} block END {};

paramdeflist: {$$ = new SymbolList();};
paramdeflist: paramdef {$$ = new SymbolList(); $$.AddParam($1);};
paramdeflist: paramdeflist ',' paramdef {$$=$1; $$.AddParam($3);};
paramdef: TYPENAME UNDEF {$$ = new FieldInfo{name=$2,basename=$1}; }; //typename undef


datadef: TYPENAME '@' INTEGER  UNDEF { CreateSym(new Symbol{name=$4,type=SymbolType.Data,datatype=$1,fixedAddr=$3}); };
datadef: TYPENAME '@' REGISTER UNDEF { CreateSym(new Symbol{name=$4,type=SymbolType.Register,datatype=$1,fixedAddr=$3}); };
datadef: TYPENAME              UNDEF { CreateSym(new Symbol{name=$2,type=SymbolType.Data,datatype=$1}); };

datadef: TYPENAME '@' INTEGER  UNDEF '[' INTEGER ']' { CreateSym(new Symbol{name=$4,type=SymbolType.Array,size=$6,datatype=$1,fixedAddr=$3}); };
datadef: TYPENAME              UNDEF '[' INTEGER ']' { CreateSym(new Symbol{name=$2,type=SymbolType.Array,size=$4,datatype=$1}); };

typedef: TYPE UNDEF '{' fielddeflist '}'     { RegisterType($2,$4); };
typedef: TYPE UNDEF '{' fielddeflist ',' '}' { RegisterType($2,$4); }; // allow trailing comma

fielddeflist: fielddef {$$ = new TypeInfo(); $$.Add($1); };
fielddeflist: fielddeflist ',' fielddef { $$=$1; $$.Add($3); };
fielddef: '@' FIELD UNDEF { $$ = new FieldInfo{name=$3,basename=$2}; };
fielddef:           UNDEF { $$ = new FieldInfo{name=$1}; };

block: ;
block: statement;
block: block statement;

statement: vassign;
statement: sassign;
statement: vexpr;
statement: sexpr;
statement: datadef {};

ifcomp: sexpr COMPARE sexpr   ;
ifcomp: sexpr                 ;

statement: IF ifcomp block elseblock END {};
elseblock: ELSE block { $$ = $2; };
elseblock: {};

statement: WHILE ifcomp DO block END {};

statement: RETURN {};
statement: RETURN vexpr {};
statement: RETURN sexpr {};

arith: '+'|'-'|'*'|'/' {};

sexpr: sexpr arith sexpr {};
sexpr: INTEGER {};
sexpr: sref {};
//sexpr: sum(vexpr)
sexpr: '&' VAR {};
sexpr: '&' ARRAY {};
sexpr: '&' ARRAY '[' sexpr ']' {};
sexpr: FUNCNAME '(' ')';

vexpr: vexpr arith vexpr {};
vexpr: vexpr arith sexpr {};
vexpr: vexpr '&' vexpr {};
vexpr: '{' littable '}';
vexpr: '*' sexpr {};
vexpr: STRING {};
vexpr: VAR {};
vexpr: FUNCNAME '(' arglist ')'; //TODO: function arguments

arglist: ;
arglist: arg;
arglist: arglist ',' arg;

arg: sexpr;
arg: vexpr;

sref: VAR '.' {ExpectFieldType=GetSymbolDataType($1);} FIELD {};
sref: INTVAR ;
sref: VAR '[' sexpr ']' ;

vassign: VAR ASSIGN vexpr ;
vassign: VAR APPEND vexpr ;

vassign: ARRAY '[' sexpr ']' ASSIGN vexpr ;

sassign: sref ASSIGN sexpr ;
sassign: sref APPEND sexpr ;



littable: ;
littable: tableitem;
littable: littable ',' tableitem;

tableitem: FIELD ASSIGN sexpr;

// */
