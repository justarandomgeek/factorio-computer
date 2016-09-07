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

      internal StatementList slVal;
}

%token <iVal> INTEGER SITE
%token <sVal> STRING LABEL SIGNAL IDENT
%token <regVal> REGISTER
%token <arithVal> ARITH
%token <compVal> COMPARE
%token <bVal> COND

%token ASSIGN APPEND
%token EXTERN JUMP RJUMP CALL RCALL BRANCH BRANCHCALL WIRE EXEC
%token DO WHILE IF ELSE END
%token PROGRAM FUNCTION RETURN
%token PUSH POP

%type <dlVal> statement datastatement ifcomp
%type <slVal> statementlist program elseblock
%type <addrVal> addrspec
%type <sigVal> signalspec

%type <dVal> dataitem
%type <dlVal> datalist sexpr vexpr

%type <dlVal> branchcomp ifcomp

%start program
%%
program: PROGRAM STRING statementlist { programData = $3; Name=$2; };

statementlist:                              { $$ = new StatementList(); };
statementlist: statementlist statement      { $$ = $1; $$.Add($2); };

statementlist: statementlist LABEL datastatement   { $$ = $1; $$.symbols.Add(new SymbolDef{name=$2,type=SymbolType.Data},$$.Count); $$.Add($3); };
statementlist: statementlist       datastatement   { $$ = $1; $$.Add($2); };
datastatement: '{' datalist '}'  { $$ = $2; };
datastatement: STRING            { $$ = DataList.BinaryString($1); };
datastatement: datastatement '&' datastatement   { $$ = $1; $$.Add($3);};
datalist: datalist dataitem      { $$ = $1; $$.Add($2); };
datalist:                        { $$ = new DataList(); };
dataitem: SIGNAL ASSIGN addrspec { $$ = new DataItem($1, $3); };

statementlist: statementlist FUNCTION LABEL statementlist END { $$ = $1; $$.Add(StatementList.Function($3, $4)); };

ifcomp: signalspec COMPARE addrspec   { $$ = DataList.IfComp($1,$2,$3); };
ifcomp: signalspec COMPARE signalspec { $$ = DataList.IfComp($1,$2,$3); };
ifcomp: signalspec                    { $$ = DataList.IfComp($1,CompSpec.Equal,0); };

statementlist: statementlist IF ifcomp statementlist elseblock END { $$ = $1; $$.Add(StatementList.If($3,$4,$5)); };
elseblock: ELSE statementlist { $$ = $2; };
elseblock: {};

statementlist: statementlist WHILE ifcomp statementlist END { $$ = $1; $$.Add(StatementList.While($3,$4)); };
//statementlist: DO statementlist WHILE ifcomp {};

signalspec: REGISTER SIGNAL { $$ = new SignalSpec($1,$2); };

addrspec: INTEGER { $$=$1; };
addrspec: IDENT   { $$=$1; };

statement: PUSH INTEGER REGISTER { $$ = DataList.Push($2,$3); };
statement: POP INTEGER REGISTER  { $$ = DataList.Pop($2,$3);  };

statement: RETURN { $$ = DataList.Jump(SymbolDef.Return, true,  false); };

statement: EXEC REGISTER { $$ = DataList.Exec($2); };

statement: JUMP  signalspec { $$ = DataList.Jump($2, false, false); };
statement: RJUMP signalspec { $$ = DataList.Jump($2, true,  false); };
statement: CALL  signalspec { $$ = DataList.Jump($2, false, true ); };
statement: RCALL signalspec { $$ = DataList.Jump($2, true,  true ); };
statement: JUMP  addrspec   { $$ = DataList.Jump($2, false, false); };
statement: RJUMP addrspec   { $$ = DataList.Jump($2, true,  false); };
statement: CALL  addrspec   { $$ = DataList.Jump($2, false, true ); };
statement: RCALL addrspec   { $$ = DataList.Jump($2, true,  true ); };
statement: BRANCH     branchcomp addrspec addrspec addrspec { $$ = DataList.Branch($2, $3, $4, $5, false); };
statement: BRANCHCALL branchcomp addrspec addrspec addrspec { $$ = DataList.Branch($2, $3, $4, $5, true ); };

branchcomp: signalspec signalspec { $$ = DataList.BranchCond($1,$2); };
branchcomp: signalspec addrspec   { $$ = DataList.BranchCond($1,$2); };
branchcomp: signalspec            { $$ = DataList.BranchCond($1,0);  };

vexpr: COND signalspec COMPARE addrspec     { $$ = DataList.CondOp(false,$1,$2,$3,$4); };
vexpr: COND signalspec COMPARE signalspec   { $$ = DataList.CondOp(false,$1,$2,$3,$4); };
vexpr: REGISTER ARITH   addrspec            { $$ = DataList.ArithOp($1,$2,$3);};
vexpr: REGISTER ARITH   signalspec          { $$ = DataList.ArithOp($1,$2,$3);};
vexpr: REGISTER ARITH   REGISTER            { $$ = DataList.ArithOp($1,$2,$3);};
vexpr: REGISTER                             { $$ = DataList.ArithOp($1,ArithSpec.Add,RegSpec.rNull);};
vexpr: '[' signalspec ']'					{ $$ = DataList.ReadMemory($2); };
vexpr: '[' addrspec   ']'					{ $$ = DataList.ReadMemory($2); };

sexpr: COND signalspec COMPARE addrspec     { $$ = DataList.CondOp(true,$1,$2,$3,$4); };
sexpr: COND signalspec COMPARE signalspec   { $$ = DataList.CondOp(true,$1,$2,$3,$4); };
sexpr: signalspec ARITH   addrspec          { $$ = DataList.ArithOp($1,$2,$3);};
sexpr: signalspec ARITH   signalspec        { $$ = DataList.ArithOp($1,$2,$3);};
sexpr: signalspec                           { $$ = DataList.ArithOp($1,ArithSpec.Add,0);};
sexpr: addrspec                             { $$ = DataList.ArithOp($1,ArithSpec.Add,0);};

statement: REGISTER   ASSIGN vexpr { $$ = $3.AssignOp($1,false); };
statement: REGISTER   APPEND vexpr { $$ = $3.AssignOp($1,true);  };
statement: signalspec ASSIGN sexpr { $$ = $3.AssignOp($1,false); };
statement: signalspec APPEND sexpr { $$ = $3.AssignOp($1,true);  };

statement: WIRE REGISTER REGISTER  {$$ = DataList.Wire($2,$3);};

statement:  '[' signalspec ']'ASSIGN REGISTER { $$ = DataList.WriteMemory($2,$5); };
statement:  '[' addrspec   ']'ASSIGN REGISTER { $$ = DataList.WriteMemory($2,$5); };
