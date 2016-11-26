-- This file is called by compiler.exe to generate a ROM.
-- Globals:
-- parser - the parser instance
--   .romdata - List<Table>
-- serpent - the included serpent library


--print(serpent.block(romdata))


signaltypes["signal-black"]="virtual"

local addrsignal={name="signal-black",type="virtual"}
local dir = 4

local entities = {}
local count = 1

function splitFilters(infilters)
  local filters,extrafilters={},{}
  for _,f in pairs(infilters) do
    if f.index <= 15 then
      table.insert(filters,f)
    else
      f.index = f.index - 15
      table.insert(extrafilters,f)
    end
  end
  if #extrafilters == 0 then extrafilters = nil end
  return filters,extrafilters
end

function extendCC(cc,filters)
  local filters,extrafilters=splitFilters(filters)

  local ecc = CC(
    {x=cc.position.x,y=cc.position.y-1},
    cc.direction,
    filters,
    {["1"]={red={{entity_id=cc.entity_number}}}}
  )

  return ecc,extrafilters
end

-- Generate a constant-combinator configured with the desginated data
function CC(position, direction, filters,connections)
  local filters,extrafilters=splitFilters(filters)
  local cc = {
    connections = (connections or {}) ,control_behavior={filters=filters}, entity_number=count,
    name = "constant-combinator", direction=direction, position = position
    }
  table.insert(entities,cc)
  count = count+1
  return cc,extrafilters
end

-- Generate the output filter for a given ROM site
function filterDC(position,dir,connections)
  local dc = {
    control_behavior={
      decider_conditions={
        first_signal=addrsignal, comparator="=", constant=0,
        output_signal={name="signal-everything",type="virtual"},
        copy_count_from_input=true}},
    connections = connections,
    direction = dir,
    name = "decider-combinator",
    position = position
  }
  table.insert(entities,dc)
  count = count+1
  return dc
end

-- Generate a buffer AC to allow injecting relocation address
function bufferAC(position,dir,connections)
  local ac = {
    control_behavior={arithmetic_conditions={first_signal={name="signal-each",type="virtual"},operation="+",constant=0,output_signal={name="signal-each",type="virtual"}}},
    connections = connections, direction = dir, entity_number = count, name = "arithmetic-combinator", position = position
  }
  table.insert(entities,ac)
  count = count+1
  return ac
end

-- Generate a power pole wtih the given connetions
function pole(pos,connections)
  local p = {
    connections = connections,
    name = "medium-electric-pole",
    position = pos
  }
  table.insert(entities,p)
  count = count+1
  return p
end



-- Place the connecting pole for the ROM
pole({x=0,y=0},{})

local prevIn = {entity_id=1}
local prevOut = {entity_id=1}
CC({x=0,y=-1},dir,{{index=1,count=-1000,signal=addrsignal}})
local prevOffset = {entity_id=2}
local xpos = 1
for addr,data in pairs(romdata) do
  -- For each ROM site, generate a constant-combinator and an output filter
  local cc_id = count
  local cc,extrafilters
  data[addrsignal.name]= -addr
  local filters = {}
  for sig,val in pairs(data) do
  	filters[#filters+1]=
  		{
  			index = #filters+1,
  			count=val,
  			signal={type=signaltypes[sig],name=sig}
  		}
  end
  
  cc,extrafilters = CC({x=xpos,y=-3},dir,filters)
  while extrafilters and #extrafilters > 0 do
    cc,extrafilters = extendCC(cc,extrafilters)
  end
  local ac_id = count
  bufferAC({x=xpos,y=-1.5},dir,
  {["1"]={green={prevOffset},red={{entity_id=cc_id}}}})
  prevOffset={entity_id=ac_id,circuit_id=1}
  filterDC({x=xpos,y=0.5},dir,
    {["1"]={green={prevIn},red={{entity_id=ac_id,circuit_id=2}}},["2"]={red={prevOut}}})
  prevIn = {entity_id=count-1,circuit_id=1}
  prevOut = {entity_id=count-1,circuit_id=2}
  xpos=xpos+1
end



parser:returnBlueprint(
  parser.Name,
  serpent.dump(
  {
    name=parser.Name,
    entities=entities,
    icons={{index=1, signal={type="item",name="constant-combinator"}}}
  }))

