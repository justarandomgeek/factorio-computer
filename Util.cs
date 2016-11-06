/*
 * Created by SharpDevelop.
 * User: Thomas
 * Date: 2016-09-05
 * Time: 10:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */


using System;

namespace compiler
{
	public static class InternalSignals
	{
		public const string op = "signal-0";
		public const string opA= "signal-A";
		
		public const string r1 = "signal-R";
		public const string s1 = "signal-S";
		
		public const string r2 = "signal-T";
		public const string s2 = "signal-U";
		
		public const string rd = "signal-V";
		public const string sd = "signal-W";
		
		public const string imm1 = "signal-grey";
		public const string imm2 = "signal-white";
		
		public const string pc = "signal-blue";
		public const string ret = "signal-green";
	}
	
	public static class Util
    {   
		
		public static int CondOpCode(InputMode sIn, bool sOut, bool flags, CompSpec comp){
			int i = 0;
        	switch (comp) {
        		case CompSpec.Equal:
					i=1; break;
        		case CompSpec.Less:
        			i=2; break;
        		case CompSpec.Greater:
					i=3; break;
        	}
			
			if(flags) i+=3;
			
			switch (sIn) {
				case InputMode.Every:
					i+=0; break;
				case InputMode.Any:
					i+=6; break;
				case InputMode.Scalar:
					i+=12; break;
				case InputMode.Each:
					i+=18; break;
			}
			
			if(sOut) i+=24;
        	
        	return i;
		}
        
        public static int ArithOpCode(InputMode sIn, bool sOut, ArithSpec arith){
			int i = sOut?52:48;
			
			switch (arith) {
				case ArithSpec.Subtract:
					i+=1; break;
				case ArithSpec.Add:
					i+=2; break;
				case ArithSpec.Divide:
					i+=3; break;
				case ArithSpec.Multiply:
					i+=4; break;
			}
			
			switch (sIn) {
				case InputMode.Every:
				case InputMode.Any:
					throw new Exception();
				case InputMode.Scalar:
					i+=4; break;
				case InputMode.Each:
					i+=0; break;
			}
			
			
			
        	return i;
		}
    }
}