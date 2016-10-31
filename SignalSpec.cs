/*
 * Created by SharpDevelop.
 * User: Thomas
 * Date: 2016-09-05
 * Time: 10:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */


using System;
using System.Collections.Generic;

namespace compiler
{
		public class SignalSpec
	{
		public static Dictionary<string,int> signalMap = new Dictionary<string, int>();
		static readonly Dictionary<char,SignalSpec> charMap = new Dictionary<char,SignalSpec>{
        	{'1',"signal-1"},{'2',"signal-2"},{'3',"signal-3"},{'4',"signal-4"},{'5',"signal-5"},
        	{'6',"signal-6"},{'7',"signal-7"},{'8',"signal-8"},{'9',"signal-9"},{'0',"signal-0"},
        	{'A',"signal-A"},{'B',"signal-B"},{'C',"signal-C"},{'D',"signal-D"},{'E',"signal-E"},
			{'F',"signal-F"},{'G',"signal-G"},{'H',"signal-H"},{'I',"signal-I"},{'J',"signal-J"},
			{'K',"signal-K"},{'L',"signal-L"},{'M',"signal-M"},{'N',"signal-N"},{'O',"signal-O"},
			{'P',"signal-P"},{'Q',"signal-Q"},{'R',"signal-R"},{'S',"signal-S"},{'T',"signal-T"},
			{'U',"signal-U"},{'V',"signal-V"},{'W',"signal-W"},{'X',"signal-X"},{'Y',"signal-Y"},
			{'Z',"signal-Z"},{'-',"fast-splitter"},{'.',"train-stop"}};
		
		public SignalSpec(char c)
        {
			if(charMap.ContainsKey(c))
			{
				this.signal = charMap[c].signal;
			}	
        }
		public SymbolRef sigval{
			get 
			{
				if(signalMap.ContainsKey(this.signal)){
					return signalMap[this.signal];
				}else{
					Console.WriteLine("Warning: undefined signal name '{0}'",this.signal);
					return 0;
				}
			}
		}
		
		public static Dictionary<string,string> typeMap = new Dictionary<string, string>();
		public string type {
			get 
			{
				if(string.IsNullOrEmpty(this.signal)) return "virtual";
				if(!typeMap.ContainsKey(this.signal))
				{
					Console.WriteLine("Warning: undefined type for signal '{0}'",this.signal);
					return "virtual";
				}
				return typeMap[this.signal];
			}
		}
		
		public int reg;
		public string signal;
		
		public static SignalSpec Imm1{get{return new SignalSpec(13,InternalSignals.imm1);}}
		public static SignalSpec Imm2{get{return new SignalSpec(13,InternalSignals.imm2);}}

		public SignalSpec(){}
		
		public static implicit operator SignalSpec(string s){return new SignalSpec(s);}
		public SignalSpec(string s){this.signal = s;}
		public SignalSpec(int r, string s){this.reg = r;this.signal = s;}
		
		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			SignalSpec other = obj as SignalSpec;
				if (other == null)
					return false;
						return this.reg == other.reg && this.signal == other.signal;
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				hashCode += 1000000007 * reg.GetHashCode();
				if (signal != null)
					hashCode += 1000000009 * signal.GetHashCode();
			}
			return hashCode;
		}

		public static bool operator ==(SignalSpec lhs, SignalSpec rhs) {
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
				return false;
			return lhs.Equals(rhs);
		}

		public static bool operator !=(SignalSpec lhs, SignalSpec rhs) {
			return !(lhs == rhs);
		}

		#endregion
		
		public override string ToString()
		{
			return string.Format("{0}.{1}", reg==0?"":reg.ToString(), signal);
		}

	}

}