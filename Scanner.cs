using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using NLua;
using SourceRconLib;
using CommandLine;

namespace compiler
{

	partial class Scanner
    {
    	public Parser Parser;
        public override void yyerror(string format, params object[] args)
        {
            //base.yyerror(format, args);
            Console.Error.WriteLine("{0} At line:{1} char:{2}",format, yyline, yycol);
        }

		Stack<BufferContext> fileStack = new Stack<BufferContext>();
		public void IncludeSource(string text)
		{
			fileStack.Push(MkBuffCtx());
			SetSource(text, 0);
		}

		protected override bool yywrap()
		{
			if(fileStack.Count > 0 )
			{
				RestoreBuffCtx(fileStack.Pop());
				return false;
			} else {
				return true;
			}
			
		}

	}

}