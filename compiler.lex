%namespace compiler
%option nofiles

%x PROGRAM

alpha [a-zA-Z]
digit [0-9]

identifier [a-zA-Z]([a-zA-Z0-9_\-])*

whitespace [ \n\r\t]+

register r[0-9]+

%%

//start in PROGRAM, unless there's a comment first
.	BEGIN(PROGRAM);yyless(0);


//comments, ignore in any context
<*>	--.*						; // single line comment, like this

<*> {whitespace};       /* ignore whitespace between tokens */

<PROGRAM> "type"      {return (int)Tokens.TYPE;}
<PROGRAM> "function"  {return (int)Tokens.FUNCTION;}
<PROGRAM> "while"     {return (int)Tokens.WHILE;}
<PROGRAM> "do"        {return (int)Tokens.DO;}
<PROGRAM> "if"        {return (int)Tokens.IF;}
<PROGRAM> "else"      {return (int)Tokens.ELSE;}
<PROGRAM> "end"       {return (int)Tokens.END;}

<PROGRAM> "int"       {return (int)Tokens.INT;}

<PROGRAM> "return"       {return (int)Tokens.RETURN;}

<PROGRAM> "?1" {yylval.bVal = true; return (int)Tokens.COND;}
<PROGRAM> "?=" {yylval.bVal = false;  return (int)Tokens.COND;}

<PROGRAM> "==" {yylval.compVal = CompSpec.Equal;   return (int)Tokens.COMPARE;}
<PROGRAM> ">"  {yylval.compVal = CompSpec.Greater; return (int)Tokens.COMPARE;}
<PROGRAM> "<"  {yylval.compVal = CompSpec.Less;    return (int)Tokens.COMPARE;}

<PROGRAM> "+=" {return (int)Tokens.APPEND;}
<PROGRAM> "="  {return (int)Tokens.ASSIGN;}

<PROGRAM> "+"  {return (int)'+';}
<PROGRAM> "-"  {return (int)'-';}
<PROGRAM> "*"  {return (int)'*';}
<PROGRAM> "/"  {return (int)'/';}

<PROGRAM> "{"  {return (int)'{';}
<PROGRAM> "}"  {return (int)'}';}
<PROGRAM> "["  {return (int)'[';}
<PROGRAM> "]"  {return (int)']';}
<PROGRAM> "("  {return (int)'(';}
<PROGRAM> ")"  {return (int)')';}

<PROGRAM> "&"  {return (int)'&';}
<PROGRAM> "%"  {return (int)'%';}
<PROGRAM> "@"  {return (int)'@';}
<PROGRAM> "."  {return (int)'.';}
<PROGRAM> ","  {return (int)',';}


<PROGRAM> {register} {
	yylval.iVal=int.Parse(yytext.TrimStart('r'));
	return (int)Tokens.REGISTER;
}

<PROGRAM> \"[^\"]{0,31}\" {
	yylval.sVal = yytext.Substring(1,yytext.Length-2);
	return (int)Tokens.STRING;
}

<PROGRAM> "var" {
  yylval.sVal = yytext;
  return (int)Tokens.TYPENAME;
}

<PROGRAM> {identifier} {
	yylval.sVal = yytext;
  //Console.WriteLine("ident: {0}", yytext);
  return (int)Parser.GetIdentType(yytext);
}

<PROGRAM> -?{digit}+ {
	yylval.iVal = Int32.Parse(yytext);
    return (int)Tokens.INTEGER;
}

%%
