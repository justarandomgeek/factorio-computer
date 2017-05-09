using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Linq;
using System.Text;

namespace nql.Blueprints
{
	public struct SignalID { public string type; public string name; }
	public struct Position { public double x; public double y; }
	public struct CircuitConnection
	{
		public int entity_id;
		public int? circuit_id;
		public IDictionary<string, object> Serialize()
		{
			var d = new Dictionary<string, object>();

			d.Add("entity_id", entity_id);
			if (circuit_id.HasValue) d.Add("circuit_id", circuit_id);

			return d;
		}
	}
	public class CircuitPort
	{
		public List<CircuitConnection> red = new List<CircuitConnection>();
		public List<CircuitConnection> green = new List<CircuitConnection>();

		public int Connections() { return red.Count + green.Count; }

		public IDictionary<string, object> Serialize()
		{
			var d = new Dictionary<string, object>();

			if (red.Count > 0) d.Add("red", red.ToArray());
			if (green.Count > 0) d.Add("green", green.ToArray());

			return d;
		}
	}

	public struct Filter { public SignalID signal; public int count; }

	public abstract class BlueprintEntity
	{
		private static int entity_count = 1;
		public readonly int entity_number = entity_count++ ;
		public string name;
		public Position position;
		public int? direction;

		public virtual IDictionary<string, object> Serialize()
		{
			var d = new Dictionary<string, object>();

			d.Add("entity_number", entity_number);
			d.Add("name", name);
			d.Add("position", position);
			if (direction.HasValue) d.Add("direction", direction.Value);

			return d;
		}

		
	}

	public abstract class CircuitEntity : BlueprintEntity
	{
		public Dictionary<string, CircuitPort> connections = new Dictionary<string, CircuitPort>();
		public override IDictionary<string, object> Serialize()
		{
			var d = base.Serialize();
			if (connections.Any((c) => { return c.Value.Connections() > 0; }))
			{
				d.Add("connections", connections);
			}
			return d;
		}
	}

	public class ConstantCombinator : CircuitEntity
	{
		public List<Filter> filters = new List<Filter>();
		public override IDictionary<string, object> Serialize()
		{
			var d = base.Serialize();
			
			if ( filters.Count > 0)
			{
				d.Add("control_behavior",
					new
					{
						filters = filters.Select(
						f => new
						{
							signal = f.signal,
							count = f.count,
							index = filters.IndexOf(f) + 1
						})
					});
			}

			return d;
		}
	}
	public class ArithmeticCombinator : CircuitEntity
	{
		public SignalID first_signal;
		public string operation;
		public SignalID? second_signal;
		public int? constant;
		public SignalID output_signal;

		public override IDictionary<string, object> Serialize()
		{
			var d = base.Serialize();

			dynamic arithmetic_conditions = new
			{
				first_signal = first_signal,
				operation = operation,
				output_signal = output_signal
			};

			if (second_signal.HasValue)
			{
				arithmetic_conditions.second_signal = second_signal.Value;
			}
			else if (constant.HasValue)
			{
				arithmetic_conditions.constant = constant.Value;
			}

			d.Add("control_behavior", new {
				arithmetic_conditions = arithmetic_conditions
			});
			return d;
		}
	}
	public class DeciderCombinator : CircuitEntity
	{
		public SignalID first_signal;
		public string comparator;
		public SignalID? second_signal;
		public int? constant;
		public SignalID output_signal;
		public bool copy_count_from_input;

		public override IDictionary<string, object> Serialize()
		{
			var d = base.Serialize();
			dynamic decider_conditions = new
			{
				first_signal = first_signal,
				comparator = comparator,
				output_signal = output_signal,
				copy_count_from_input = copy_count_from_input
			};

			if (second_signal.HasValue)
			{
				decider_conditions.second_signal = second_signal.Value;
			}
			else if (constant.HasValue)
			{
				decider_conditions.constant = constant.Value;
			}

			d.Add("control_behavior", new
			{
				decider_conditions = decider_conditions
			});
			return d;
		}

	}
	public class PowerPole : CircuitEntity
	{

	}

	public struct BlueprintTile
	{
		public Position position;
		public string name;
	}

	public class Blueprint
	{
		
		public SignalID?[] icons = new SignalID?[4];
		public readonly List<BlueprintEntity> entities = new List<BlueprintEntity>();
		public readonly List<BlueprintTile> tiles = new List<BlueprintTile>();
				
		public string GetBlueprintJSON()
		{
			var j = new JavaScriptSerializer();
			j.RegisterConverters(new List<JavaScriptConverter>{ new BlueprintJsonConverter() });
			return j.Serialize(this);
		}

		public byte[] GetBlueprintCompressedBytes()
		{
			using (var ms = new System.IO.MemoryStream())
			{
				// This is the ZLib header to match the compression DeflateStream does
				// https://blogs.msdn.microsoft.com/bclteam/2007/05/16/system-io-compression-capabilities-kim-hamilton/
				ms.WriteByte(0x58);
				ms.WriteByte(0x85);

				// I have to calculate my own Addler checksum, because DeflatesStream doesn't
				UInt16 AddlerA = 1;
				UInt16 AddlerB = 0;

				using (var sw = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress, true))
				{
					var byteprint = Encoding.UTF8.GetBytes(GetBlueprintJSON());

					for (int i = 0; i < byteprint.Length; i++)
					{
						AddlerA = (UInt16)((AddlerA + byteprint[i]) % 65521);
						AddlerB = (UInt16)((AddlerB + AddlerA) % 65521);

						sw.WriteByte(byteprint[i]);
					}

				}
				
				ms.WriteByte((byte)(AddlerB >> 8));
				ms.WriteByte((byte)(AddlerB & 0xFF));
				ms.WriteByte((byte)(AddlerA >> 8));
				ms.WriteByte((byte)(AddlerA & 0xFF));

				return ms.ToArray();
			}
		}

		public string GetBlueprintString()
		{
			return "0" + Convert.ToBase64String(GetBlueprintCompressedBytes());
		}

		public IDictionary<string, object> Serialize()
		{
			var d = new Dictionary<string, object>();

			var iconlist = new List<dynamic>();
			for (int i = 0; i < icons.Length; i++)
			{
				if ( icons[i] != null )
				iconlist.Add(new { index = i+1, signal = icons[i] });
			}
			if (iconlist.Count() > 0 ) d.Add("icons", iconlist);

			if (entities.Count() > 0 ) d.Add("entities", entities);
			if (tiles.Count() > 0) d.Add("tiles", tiles);
			d.Add("item", "blueprint");
			d.Add("version", (UInt64)((15l << 32) + (9l << 16)));
			return new Dictionary<string, object> { ["blueprint"] = d };
			
		}
				
		class BlueprintJsonConverter : JavaScriptConverter
		{
			public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
			{
				if (obj is Blueprint) return ((Blueprint)obj).Serialize();
				if (obj is BlueprintEntity) return ((BlueprintEntity)obj).Serialize();
				if (obj is CircuitPort) return ((CircuitPort)obj).Serialize();
				if (obj is CircuitConnection) return ((CircuitConnection)obj).Serialize();

				throw new NotImplementedException();
			}

			public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
			{
				throw new NotImplementedException();
			}

			public override IEnumerable<Type> SupportedTypes
			{
				get
				{
					yield return typeof(Blueprint);
					yield return typeof(BlueprintEntity);
					yield return typeof(CircuitPort);
					yield return typeof(CircuitConnection);
				}
			}
		}

	}
}
