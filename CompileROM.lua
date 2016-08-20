local serpent = require "serpent0272"

-- This file is called by compiler.exe to generate a ROM.
-- Globals:
-- parser - the parser instance
--   .programData - Dictionary<int,DataList>
--   :mapChar(c) - converts a character to a SignalSpec
--   :PrintCompressed(s) - Prints a string to the console gzipped and base64'd.
--      Use with serpent to produce blueprints.


local addrsignal ={name="signal-black",type="virtual"}
local dir = 4

-- globals: name romdata<int,DataList<SignalSpec,AddrSpec>>

local romsites = {}

-- This works through the .NET program data objects and builds constant combinator
-- filters in romsites. Each filterset includes the data and inverted address for
-- a single ROM location.

--TODO: make pairs() work nicely on IEnumerable to make this cleaner? or proper Lua types...
local enumerator = parser.programData:GetEnumerator()
while enumerator:MoveNext() do
  local addr = enumerator.Current.Key

  local i = 1
  local d = {}
  table.insert(d,{index=i,count=-addr,signal=addrsignal})

  local dataen = enumerator.Current.Value:GetEnumerator()
  while dataen:MoveNext() do
    local signal = dataen.Current.Key
    local val = dataen.Current.Value
    i=i+1
    table.insert(d,{index=i,count=val:resolve(addr),signal={name=signal.signal,type=signal.type}})
  end
  table.insert(romsites,d)
end

local entities = {}
local count = 1

-- Map a character to it's string or constant-combinator sign signal equvialent
function signChar(s,i)
  local csig = parser:mapChar(s)
  return {index=i,count=1,signal={name=csig.signal,type=csig.type}}

end

-- Generate a constant-combinator configured as a sign for two or four characters of text
function signCC(position, direction, s1,s2)
  local f = {}
  table.insert(f,signChar((s1 or "  "):sub(1,1),1))
  table.insert(f,signChar((s1 or "  "):sub(2,2),2))

  if s2 then
    table.insert(f,signChar(s2:sub(1,1),3))
    table.insert(f,signChar(s2:sub(2,2),4))
  end
  return CC(position, direction, f)
end

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

-- split off the first two characters of a string - retruns "st","ring"
function prefix(s)
  if not s then
    return nil,nil
  elseif s == "@@" then
    return "  ","@@"
  elseif #s<=2 then
    return s,"@@"
  else
    return s:sub(1,2), s:sub(3)
  end
end

-- Generate a constant-combinator sign
function sign(pos,line1,line2)
  local xpos = pos.x
  local ypos = pos.y
  local s1,s2
  repeat
    s1,line1 = prefix(line1)
    s2,line2 = prefix(line2)
    signCC({x = xpos,y = ypos},dir, s1, s2)
    count = count + 1
    xpos = xpos + 1
  until(
    (not line1 or line1=="@@") and
    (not line2 or line2=="@@"))
end


-- Place the connecting pole for the ROM
pole({x=0,y=0},{})

local prevIn = {entity_id=1}
local prevOut = {entity_id=1}

local xpos = 1
for _,filters in pairs(romsites) do
  -- For each ROM site, generate a constant-combinator and an output filter
  local cc_id = count
  local cc,extrafilters
  cc,extrafilters = CC({x=xpos,y=-1},dir,filters)
  while extrafilters and #extrafilters > 0 do
    cc,extrafilters = extendCC(cc,extrafilters)
  end
  filterDC({x=xpos,y=0.5},dir,
    {["1"]={green={prevIn},red={{entity_id=cc_id}}},["2"]={red={prevOut}}})
  prevIn = {entity_id=count-1,circuit_id=1}
  prevOut = {entity_id=count-1,circuit_id=2}
  xpos=xpos+1
end

--Put program name sign above the ROM
sign({x=1,y=-2},string.upper(parser.Name),nil)

parser:returnBlueprint(
  parser.Name,
  serpent.dump(
  {
    name=parser.Name,
    entities=entities,
    icons={{index=1, signal={type="item",name="constant-combinator"}}}
  }))

entities = {}
count = 1
-- Generate address table sign array
local prevsign
ypos = 0
local mapenum = map:GetEnumerator()
while mapenum:MoveNext() do
  local label = mapenum.Current.Key
  local addr = mapenum.Current.Value

  local thissign = string.upper(string.format("%4d %s", addr, label))
  if prevsign then
    sign({x=0,y=ypos},prevsign,thissign)
    ypos=ypos+1
    prevsign=nil
  else
    prevsign = thissign
  end
end
if prevsign then
  sign({x=0,y=ypos},prevsign,"@@")
end


parser:returnBlueprint(
  parser.Name .. " Addr Table",
  serpent.dump(
  {
    name=parser.Name .. " Addr Table",
    entities=entities,
    icons={{index=1, signal={type="item",name="constant-combinator"}}}
  }))
