using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace compiler
{

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class TypeInfo:Dictionary<string,string>
	{
		public bool hasString;
		
		public void Add(FieldInfo fi)
		{
			this.Add(fi.name,fi.basename);
		}

		public void Allocate()
		{
			if (!this.ContainsValue(null)) return;

			string nextSignal = hasString ? "signal-red" : "signal-A";

			if(this.Any(f=>f.Value != null))
			{
				
				var lastField = this.OrderBy(
					f =>
					Program.CurrentProgram.NativeFields.IndexOf(f.Value)
					).Last();

				if (Program.CurrentProgram.NativeFields.IndexOf(lastField.Value) >
					Program.CurrentProgram.NativeFields.IndexOf(nextSignal))
					nextSignal = Program.CurrentProgram.NativeFields[
						Program.CurrentProgram.NativeFields.IndexOf(lastField.Value)+1
						];

			}

			int nextIndex = Program.CurrentProgram.NativeFields.IndexOf(nextSignal);

			foreach (var field in this.Where(f => f.Value == null).Select(f => f.Key).ToList())
			{
				this[field] = Program.CurrentProgram.NativeFields[nextIndex++];
			}



		}
	}

}