using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace nql
{

	public enum ArithSpec
	{
		Add,
		Subtract,
		Multiply,
		Divide,

		Modulo,
		Power,

		LShift,
		RShift,

		BAnd,
		BOr,
		BXor,
	}

}