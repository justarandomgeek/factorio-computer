%namespace compiler
%partial
%union {
  internal int iVal;
  internal string sVal;
  internal bool bVal;

  internal ArithSpec arithVal;
  internal CompSpec compVal;
  internal TypeInfo tiVal;
  internal FieldInfo fiVal;
  internal SymbolList slVal;
  internal SExpr seVal;
  internal VExpr veVal;
  internal SRef srVal;
  internal VRef vrVal;
  internal Table tabVal;
  internal TableItem tabiVal;
  internal Statement statVal;
  internal Block blockVal;
  internal Branch brVal;
  internal ExprList elVal;
  internal RefList rlVal;
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
%token INT

%type <arithVal> arith

%type <tiVal> fielddeflist
%type <fiVal> fielddef paramdef

%type <slVal> paramdeflist

%type <seVal> sexpr
%type <veVal> vexpr
%type <srVal> sref
%type <vrVal> vref
%type <tabVal> littable
%type <tabiVal> tableitem

%type <statVal> statement vassign sassign
%type <blockVal> block elseblock
%type <brVal> branch
%type <elVal> exprlist
%type <rlVal> reflist

%start program

%%

//<_aD> are there snippets of my IRC comments in the code?
//<_aD> because there bloody well should be *adjusts monocle*

program: definition {};
program: program definition {};

definition: functiondef;
definition: datadef;
definition: typedef;

functiondef: FUNCTION UNDEF '(' paramdeflist ')' { BeginFunction($2,$4); } block END { CompleteFunction($2,$7); };

paramdeflist: {$$ = new SymbolList();};
paramdeflist: paramdef {$$ = new SymbolList(); $$.AddParam($1);};
paramdeflist: paramdeflist ',' paramdef {$$=$1; $$.AddParam($3);};
paramdef: TYPENAME UNDEF {$$ = new FieldInfo{name=$2,basename=$1}; };
paramdef: INT      UNDEF {$$ = new FieldInfo{name=$2,basename="int"}; };

datadef: TYPENAME '@' INTEGER  UNDEF { CreateSym(new Symbol{name=$4,type=SymbolType.Data,datatype=$1,fixedAddr=$3}); };
datadef: TYPENAME '@' REGISTER UNDEF { CreateSym(new Symbol{name=$4,type=SymbolType.Register,datatype=$1,fixedAddr=$3}); };
datadef: TYPENAME              UNDEF { CreateSym(new Symbol{name=$2,type=InFunction!=null?SymbolType.Register:SymbolType.Data,datatype=$1}); };
datadef: INT                   UNDEF { CreateInt($2); };

datadef: TYPENAME '@' INTEGER  UNDEF '[' INTEGER ']' { CreateSym(new Symbol{name=$4,type=SymbolType.Array,size=$6,datatype=$1,fixedAddr=$3}); };
datadef: TYPENAME              UNDEF '[' INTEGER ']' { CreateSym(new Symbol{name=$2,type=SymbolType.Array,size=$4,datatype=$1}); };

typedef: TYPE UNDEF '{' fielddeflist '}'     { RegisterType($2,$4); };
typedef: TYPE UNDEF '{' fielddeflist ',' '}' { RegisterType($2,$4); }; // allow trailing comma

fielddeflist: fielddef {$$ = new TypeInfo(); $$.Add($1); };
fielddeflist: fielddeflist ',' fielddef { $$=$1; $$.Add($3); };
fielddef: '@' FIELD UNDEF { $$ = new FieldInfo{name=$3,basename=$2}; };
fielddef:           UNDEF { $$ = new FieldInfo{name=$1}; };

block: {$$=new Block();};
block: statement { $$=new Block(); $$.Add($1); };
block: block statement { $$=$1; $$.Add($2); };

statement: vassign {$$=$1;};
statement: sassign {$$=$1;};
statement: datadef;

branch: sexpr COMPARE sexpr {$$=new Branch{S1=$1,Op=$2,S2=$3};};

statement: IF branch block elseblock END {$$= new If{branch=$2,ifblock=$3,elseblock=$4}; };
elseblock: ELSE block { $$ = $2; };
elseblock: {$$=new Block();};

statement: WHILE branch DO block END {$$= new While{branch=$2,body=$4}; };

statement: FUNCNAME '(' exprlist ')' { $$ = new FunctionCall{name=$1,args=$3}; };
statement: reflist ASSIGN FUNCNAME '(' exprlist ')' { $$ = new FunctionCall{name=$3,args=$5,returns=$1}; };

exprlist: { $$=new ExprList(); };
exprlist: sexpr { $$=new ExprList(); $$.ints.Add($1); };
exprlist: exprlist ',' sexpr { $$=$1; $$.ints.Add($3); };
exprlist: vexpr { $$=new ExprList(); $$.vars.Add($1); };
exprlist: exprlist ',' vexpr { $$=$1; $$.vars.Add($3); };

reflist: { $$=new RefList(); };
reflist: sref { $$=new RefList(); $$.ints.Add($1); };
reflist: reflist ',' sref { $$=$1; $$.ints.Add($3); };
reflist: vref { $$=new RefList(); $$.vars.Add($1); };
reflist: reflist ',' vref { $$=$1; $$.vars.Add($3); };

statement: RETURN exprlist { $$ = new Return{returns=$2}; };

arith: '+' {$$ = ArithSpec.Add;};
arith: '-' {$$ = ArithSpec.Subtract;};
arith: '*' {$$ = ArithSpec.Multiply;};
arith: '/' {$$ = ArithSpec.Divide;};

sexpr: sexpr arith sexpr {$$=new ArithSExpr{S1=$1,Op=$2,S2=$3};};
sexpr: INTEGER {$$=new IntSExpr{value=$1};};
sexpr: sref {$$=$1;};
//sexpr: sum(vexpr)
//sexpr: '&' VAR {};
//sexpr: '&' ARRAY {};
//sexpr: '&' ARRAY '[' sexpr ']' {};


vexpr: vexpr arith vexpr {$$=new ArithVExpr{V1=$1,Op=$2,V2=$3};};
vexpr: vexpr arith sexpr {$$=new ArithVSExpr{V1=$1,Op=$2,S2=$3};};
vexpr: '{' littable '}'{$$=$2;};
//vexpr: '*' sexpr {};
vexpr: STRING {$$= new StringVExpr{text=$1};};
vexpr: vref{$$=$1;};

sref: VAR '.' {ExpectFieldType=GetSymbolDataType($1);} FIELD {$$ = new FieldSRef{varname=$1,fieldname=$4};};
sref: INTVAR {$$ = new IntVarSRef{name=$1};};
//sref: VAR '[' sexpr ']' ;

vref: VAR {$$=new VarVRef{name=$1};};
vref: ARRAY '[' sexpr ']' {$$=new ArrayVRef{arrname=$1,offset=$3};};

vassign: vref ASSIGN vexpr {$$=new VAssign{target=$1,append=false,source=$3};};
vassign: vref APPEND vexpr {$$=new VAssign{target=$1,append=true,source=$3};};


sassign: sref ASSIGN sexpr {$$=new SAssign{target=$1,append=false,source=$3};};
sassign: sref APPEND sexpr {$$=new SAssign{target=$1,append=true,source=$3};};



littable: {$$= new Table();};
littable: tableitem {$$= new Table();$$.Add($1);};
littable: littable ',' tableitem {$$=$1;$$.Add($3);};

tableitem: FIELD ASSIGN sexpr {$$=new TableItem($1,$3); };

// */
