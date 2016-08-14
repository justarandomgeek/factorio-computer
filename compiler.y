%namespace compiler
%partial
%union {
			internal int iVal;
			internal string sVal;
			internal bool bVal;

			internal RegSpec regVal;
			internal ArithSpec arithVal;
			internal CompSpec compVal;

			internal AddrSpec addrVal;
			internal SignalSpec sigVal;

			internal DataItem dVal;
			internal DataList dlVal;
}

%token <iVal> INTEGER SITE
%token <sVal> STRING LABEL SIGNAL
%token <addrVal> IDENT
%token <regVal> REGISTER
%token <arithVal> ARITH
%token <compVal> COMPARE
%token <bVal> COND

%token ASSIGN APPEND
%token EXTERN JUMP RJUMP CALL RCALL BRANCH BRANCHCALL WIRE MEMCPY

%type <dlVal> statement datastatement
%type <addrVal> addrspec
%type <sigVal> signalspec

%type <dVal> dataitem
%type <dlVal> datalist sexpr vexpr

%start program
%%
program: STRING            { Name=$1;      };
program: program statement { Add($2);      };
program: program EXTERN    { AddExtern();  };
program: program STRING    { Add($2);      };
program: program LABEL     { AddLabel($2); };
program: program SITE      { SetNext($2);  };

program: program datastatement '&' STRING    { Add($4,$2);      };

signalspec: REGISTER SIGNAL { $$ = new SignalSpec($1,$2); };

addrspec: INTEGER { $$=$1; };
addrspec: IDENT   { $$=$1; };

statement: JUMP  signalspec { $$ = DataList.Jump($2, false, false); };
statement: RJUMP signalspec { $$ = DataList.Jump($2, true,  false); };
statement: CALL  signalspec { $$ = DataList.Jump($2, false, true ); };
statement: RCALL signalspec { $$ = DataList.Jump($2, true,  true ); };
statement: JUMP  addrspec   { $$ = DataList.Jump($2, false, false); };
statement: RJUMP addrspec   { $$ = DataList.Jump($2, true,  false); };
statement: CALL  addrspec   { $$ = DataList.Jump($2, false, true ); };
statement: RCALL addrspec   { $$ = DataList.Jump($2, true,  true ); };
statement: BRANCH     signalspec signalspec { $$ = DataList.Branch($2, $3, false); };
statement: BRANCHCALL signalspec signalspec { $$ = DataList.Branch($2, $3, true ); };
statement: BRANCH     signalspec addrspec   { $$ = DataList.Branch($2, $3, false); };
statement: BRANCHCALL signalspec addrspec   { $$ = DataList.Branch($2, $3, true ); };
statement: BRANCH     signalspec            { $$ = DataList.Branch($2,  0, false); };
statement: BRANCHCALL signalspec            { $$ = DataList.Branch($2,  0, true ); };

vexpr: COND signalspec COMPARE addrspec     { $$ = DataList.CondOp(false,$1,$2,$3,$4); };
vexpr: COND signalspec COMPARE signalspec   { $$ = DataList.CondOp(false,$1,$2,$3,$4); };
vexpr: MEMCPY REGISTER REGISTER;
vexpr: REGISTER ARITH   addrspec            { $$ = DataList.ArithOp($1,$2,$3);};
vexpr: REGISTER ARITH   signalspec          { $$ = DataList.ArithOp($1,$2,$3);};
vexpr: REGISTER                             { $$ = DataList.ArithOp($1,ArithSpec.Add,0);};
vexpr: '[' signalspec ']'					{ $$ = DataList.ReadMemory($2); };
vexpr: '[' addrspec   ']'					{ $$ = DataList.ReadMemory($2); };

sexpr: COND signalspec COMPARE addrspec     { $$ = DataList.CondOp(true,$1,$2,$3,$4); };
sexpr: COND signalspec COMPARE signalspec   { $$ = DataList.CondOp(true,$1,$2,$3,$4); };
sexpr: signalspec ARITH   addrspec          { $$ = DataList.ArithOp($1,$2,$3);};
sexpr: signalspec ARITH   signalspec        { $$ = DataList.ArithOp($1,$2,$3);};
sexpr: signalspec                           { $$ = DataList.ArithOp($1,ArithSpec.Add,0);};
sexpr: addrspec                             { $$ = DataList.ArithOp(new SignalSpec(RegSpec.rNull,""),ArithSpec.Add,$1);};

statement: REGISTER   ASSIGN vexpr { $$ = $3.AssignOp($1,false); };
statement: REGISTER   APPEND vexpr { $$ = $3.AssignOp($1,true);  };
statement: signalspec ASSIGN sexpr { $$ = $3.AssignOp($1,false); };
statement: signalspec APPEND sexpr { $$ = $3.AssignOp($1,true);  };

statement: WIRE REGISTER REGISTER  {$$ = DataList.Wire($2,$3);};

statement:  '[' signalspec ']'ASSIGN REGISTER { $$ = DataList.WriteMemory($2,$5); };
statement:  '[' addrspec   ']'ASSIGN REGISTER { $$ = DataList.WriteMemory($2,$5); };


dataitem: SIGNAL ASSIGN addrspec { $$ = new DataItem($1, $3); };
datalist: datalist dataitem      { $$ = $1; $$.Add($2); };
datalist: dataitem               { $$ = $1; };
datastatement: '{' datalist '}'      { $$ = $2; };
datastatement: '{' '}'               { $$ = new DataList(); };
datastatement: '%' STRING            { $$ = DataList.BinaryString($2); };
statement: datastatement { $$ = $1; };
