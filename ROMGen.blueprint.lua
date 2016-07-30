do local foo="bar"
--[[ The line above must start with "do local" to be recognized as a decompressed blueprint.
     Adjust rombase to control the first memory address the rom is generated for.
     Adjust addrsignal below to control memory address signal. Requires a compatible machine, and address signal cannot be stored.
     data is array of parameter lists for constant combinators.
     
     strings are converted to serialized frames and appended to data. No terminators are added.
     pixeldata is converted and added after strings.
  ]]

local rombase = 1001
local addrsignal = {name = "signal-A",type = "virtual"}
local data = { 
{{count = 704643295, signal = {name = 'signal-blue', type = 'virtual'}},{count = 1019, signal = {name = 'signal-S', type = 'virtual'}},{count = 1001, signal = {name = 'signal-D', type = 'virtual'}},{count = 12, signal = {name = 'signal-N', type = 'virtual'}},},
{{count = 1006633183, signal = {name = 'signal-blue', type = 'virtual'}},{count = 1, signal = {name = 'signal-S', type = 'virtual'}},{count = 7, signal = {name = 'signal-W', type = 'virtual'}},{count = 1, signal = {name = 'signal-N', type = 'virtual'}},},
{{count = 186056944, signal = {name = 'signal-blue', type = 'virtual'}},{count = 6, signal = {name = 'signal-N', type = 'virtual'}},},
{{count = 1006633183, signal = {name = 'signal-blue', type = 'virtual'}},{count = 1, signal = {name = 'signal-S', type = 'virtual'}},{count = 1, signal = {name = 'signal-W', type = 'virtual'}},{count = 1, signal = {name = 'signal-N', type = 'virtual'}},},
{{count = 186056944, signal = {name = 'signal-blue', type = 'virtual'}},{count = 6, signal = {name = 'signal-N', type = 'virtual'}},{count = -7, signal = {name = 'signal-S', type = 'virtual'}},},
{{count = 889201935, signal = {name = 'signal-blue', type = 'virtual'}},{count = -2, signal = {name = 'signal-green', type = 'virtual'}},},
{}, --[[ This space intentionally left blank ]]
{},
{},
{},
{},
{},
{},
{},
{},
{},
{},
{},
{{count = 1, signal = {name = 'inserter', type = 'item'}},},
}

local strings = { 
{color="signal-red",   text="Hello "},
{color="signal-green", text="World"},
{color="signal-blue",  text="!"},
}

local pixeldata={
{color="signal-red",     data={1,0,0,0,0,0,0,1}},
{color="signal-green",   data={0,1,0,0,0,0,1,0}},
{color="signal-blue",    data={0,0,1,0,0,1,0,0}},
{color="signal-cyan",    data={0,0,0,1,1,0,0,0}},
{color="signal-pink", data={0,0,1,0,0,1,0,0}},
{color="signal-yellow",  data={0,1,0,0,0,0,1,0}},
}


--[[ ROMGen Code begins here ]]

local chars={
["A"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'alien-artifact', type = 'item'}}},
["B"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-B', type = 'virtual'}}},
["C"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-C', type = 'virtual'}}},
["D"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-D', type = 'virtual'}}},
["E"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-E', type = 'virtual'}}},
["F"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-F', type = 'virtual'}}},
["G"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-G', type = 'virtual'}}},
["H"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-H', type = 'virtual'}}},
["I"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-I', type = 'virtual'}}},
["J"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-J', type = 'virtual'}}},
["K"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-K', type = 'virtual'}}},
["L"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-L', type = 'virtual'}}},
["M"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-M', type = 'virtual'}}},
["N"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-N', type = 'virtual'}}},
["O"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-O', type = 'virtual'}}},
["P"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-P', type = 'virtual'}}},
["Q"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-Q', type = 'virtual'}}},
["R"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-R', type = 'virtual'}}},
["S"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-S', type = 'virtual'}}},
["T"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-T', type = 'virtual'}}},
["U"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-U', type = 'virtual'}}},
["V"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-V', type = 'virtual'}}},
["W"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-W', type = 'virtual'}}},
["X"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-X', type = 'virtual'}}},
["Y"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-Y', type = 'virtual'}}},
["Z"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-Z', type = 'virtual'}}},
[" "]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}}},
["!"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-grey', type = 'virtual'}}},
["_"]={{count=1,signal={name = 'medium-electric-pole', type = 'item'}},{count=1,signal={name = 'signal-black', type = 'virtual'}}},
}

while strings[1] do
  local s = table.remove(strings,1)
  local str = string.upper(s.text)
  for i = 1, #str do
    local c = str:sub(i,i)
    local d =  util.table.deepcopy(chars[c])
    if s.color then
      table.insert(d,{count=1,signal={name=s.color,type='virtual'}})
    end
    table.insert(data,d)
  end
end

while pixeldata[1] do
  local p = table.remove(pixeldata,1)
  local d = {
    {count=1,signal={name='rail',type='item'}},
    {count=p.data[1],signal={name='transport-belt',type='item'}},
    {count=p.data[2],signal={name='fast-transport-belt',type='item'}},
    {count=p.data[3],signal={name='express-transport-belt',type='item'}},
    {count=p.data[4],signal={name='underground-belt',type='item'}},
    {count=p.data[5],signal={name='fast-underground-belt',type='item'}},
    {count=p.data[6],signal={name='express-underground-belt',type='item'}},
    {count=p.data[7],signal={name='splitter',type='item'}},
    {count=p.data[8],signal={name='express-splitter',type='item'}},dw
  }
  
  if p.color then
    table.insert(d,{count=1,signal={name=p.color,type='virtual'}})
  end
  
  table.insert(data,d)
end



local entities = {}
local count = 1

--[[ CC() Creates a constant combinator with the given data and connections ]]
function CC(position, filters, connections)
  local cc = {
		connections = connections ,
    entity_number = count,
    control_behavior={filters=filters},
    name = "constant-combinator",
		direction=2,
    position = position 
    }
  count = count+1
  return cc
end 

--[[ DC() Creates the output decider combinator with the given connections ]]
function DC(position,connections)
  local dc = {
    control_behavior={
	   decider_conditions={
		  first_signal=addrsignal,comparator="=",constant=0,
		  output_signal={type="virtual",name="signal-everything"},
		  copy_count_from_input=true
		}
	 },
	 connections = connections ,
    direction = 6,
    entity_number = count,
    name = "decider-combinator",
    position = position
  }
  count = count+1
  return dc
end

--[[ AC() Creates the index arithmetic combinator with the given connections ]]
function AC(position,connections)
  local ac = {
    control_behavior={
      arithmetic_conditions={
        first_signal=addrsignal,operation="-",constant=1,
        output_signal=addrsignal
    }
    },
    connections = connections ,
    direction = 2,
    entity_number = count,
    name = "arithmetic-combinator",
    position = position 
  }
  count = count+1
  return ac
end

--[[ indexdata() adds a sequential .index property to tables ]]
function indexdata(data)
  local indexed = {}
  local i = 1
  
  for _,d in pairs(data) do
    d.index = i
    indexed[i]=d
    i = i+1
  end

  return indexed
end

--[[ ROM0() Creates the first cell of ROM and associated support pieces ]]
function ROM0(content,baseaddr)
  table.insert(entities, { --[[ Input Connection Pole r:content g:request both connect to the first memory cell's decider ]]
    connections = {} ,
    entity_number = count,
    name = "medium-electric-pole",
    position = {x = 0,y = 1} 
  })
  count =count+1
    
  table.insert(entities, CC({x=1,y=-1}, indexdata{{count = baseaddr,signal = addrsignal}}, {}))
  table.insert(entities, CC({x=2,y=0}, indexdata(content) , {["1"] = {red = {{entity_id = 2}}}}))
  table.insert(entities, DC({x = 1.5,y = 1}, {
    ["1"] = {green = {{entity_id = 1}},red = {{entity_id = 3}}} ,
    ["2"] = {red = {{entity_id = 1}}} 
    }))

  local index={entity_id=2}
  local output=4

  return index,output
end

--[[ ROMn() Creates successive memory cells ]]
function ROMn(prevIndex,prevOutput,content,xpos)
  local index={entity_id=count,circuit_id=2}
  table.insert(entities, AC({x = xpos+0.5,y = -1},{["1"] = {green = {prevIndex}} , ["2"] = {}}))
  table.insert(entities, CC({x = xpos+1,y = 0}, indexdata(content), {["1"] = {red = {{circuit_id = 2,entity_id = count-1}}}}))
  local output=count
  table.insert(entities, DC({x = xpos+0.5,y = 1},{
      ["1"] = {
        green = {{circuit_id = 1,entity_id = prevOutput}},
        red = {{entity_id = count-1}} 
      } ,
      ["2"] = {
        red = {{circuit_id = 2,entity_id = prevOutput}} 
      } 
    }))
  return index,output
end


--[[ Generate ROM cells ]]
local index,output = ROM0(table.remove(data,1),-rombase)
local xpos=3
while data[1] do
  index,output = ROMn(index,output,table.remove(data,1),xpos)
  xpos = xpos + 2
end


--[[ Assemble and return blueprint ]]
local blueprintData = {
  entities = entities,
  icons={
    {index=1, signal={type="item",name="decider-combinator"}},
    {index=2, signal={type="item",name="constant-combinator"}}
  },
  name = "ROMGen",
  version = "0.1.2"
} 
return blueprintData

end