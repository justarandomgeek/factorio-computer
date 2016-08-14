%namespace compiler
%option nofiles

%x PROGRAM

alpha [a-zA-Z]
digit [0-9]

identifier {alpha}({alpha}|{digit}|"-")*

whitespace [ \n\r\t]+

%%

//start in PROGRAM, unless there's a comment first
[^/]	BEGIN(PROGRAM);yyless(0);

//comments, ignore in any context
<*>	\/\*([^*]|\*[^\/])*\*\/		;	// block-style comments /**/
<*>	\/\/.*						;	// single line comment, like this

<*> {whitespace};       /* ignore whitespace between tokens */

<PROGRAM> "extern"        {return (int)Tokens.EXTERN;}

<PROGRAM> "jump"|"jmp"    {return (int)Tokens.JUMP;}
<PROGRAM> "rjump"|"rjmp"  {return (int)Tokens.RJUMP;}
<PROGRAM> "call"          {return (int)Tokens.CALL;}
<PROGRAM> "rcall"         {return (int)Tokens.RCALL;}

<PROGRAM> "branch"        {return (int)Tokens.BRANCH;}
<PROGRAM> "branchcall"    {return (int)Tokens.BRANCHCALL;}

<PROGRAM> "wire"        {return (int)Tokens.WIRE;}
<PROGRAM> "memcpy"        {return (int)Tokens.MEMCPY;}

<PROGRAM> "?1" {yylval.bVal = true; return (int)Tokens.COND;}
<PROGRAM> "?=" {yylval.bVal = false;  return (int)Tokens.COND;}

<PROGRAM> "==" {yylval.compVal = CompSpec.Equal;   return (int)Tokens.COMPARE;}
<PROGRAM> ">"  {yylval.compVal = CompSpec.Greater; return (int)Tokens.COMPARE;}
<PROGRAM> "<"  {yylval.compVal = CompSpec.Less;    return (int)Tokens.COMPARE;}

<PROGRAM> "+" {yylval.arithVal = ArithSpec.Add;      return (int)Tokens.ARITH;}
<PROGRAM> "-" {yylval.arithVal = ArithSpec.Subtract; return (int)Tokens.ARITH;}
<PROGRAM> "*" {yylval.arithVal = ArithSpec.Multiply; return (int)Tokens.ARITH;}
<PROGRAM> "/" {yylval.arithVal = ArithSpec.Divide;   return (int)Tokens.ARITH;}

<PROGRAM> "="  {return (int)Tokens.ASSIGN;}
<PROGRAM> "+=" {return (int)Tokens.APPEND;}

<PROGRAM> "{"  {return (int)'{';}
<PROGRAM> "}"  {return (int)'}';}
<PROGRAM> "["  {return (int)'[';}
<PROGRAM> "]"  {return (int)']';}
<PROGRAM> "("  {return (int)'(';}
<PROGRAM> ")"  {return (int)')';}
<PROGRAM> "&"  {return (int)'&';}
<PROGRAM> "%"  {return (int)'%';}


<PROGRAM> "r"("Null"|[0-9]+|"Red"|"Green"|"Stat"|"Op"|"Nixie") {
	if(!Enum.TryParse<RegSpec>(yytext,out yylval.regVal))
	{
		yylval.regVal=(RegSpec)int.Parse(yytext.TrimStart('r'));
	}
	return (int)Tokens.REGISTER;
}

<PROGRAM> \"[^\"]*\" {
	yylval.sVal = yytext.Substring(1,yytext.Length-2);
	return (int)Tokens.STRING;
}

<PROGRAM> \.{identifier} {
	yylval.sVal = yytext.Substring(1);
	return (int)Tokens.SIGNAL;
}

<PROGRAM> {identifier}":" {
	yylval.sVal = yytext.TrimEnd(':');
	return (int)Tokens.LABEL;
}

<PROGRAM> {identifier}\+{digit}+ {
	var s = yytext.Split('+');
	yylval.addrVal = new AddrSpec{ identifier=s[0], identifierOffset=Int32.Parse(s[1]) };
	return (int)Tokens.IDENT;
}

<PROGRAM> {identifier} {
	yylval.addrVal = new AddrSpec{identifier=yytext};
	return (int)Tokens.IDENT;
}

<PROGRAM> {digit}+\@    {
	yylval.iVal = Int32.Parse(yytext.Trim('@'));
    return (int)Tokens.SITE;
}

<PROGRAM> -?{digit}+ {
	yylval.iVal = Int32.Parse(yytext);
    return (int)Tokens.INTEGER;
}

%%
