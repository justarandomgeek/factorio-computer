do local script=true
  --[[The line above allows Foreman to recognize this as a script.]]

  if not defines then defines={direction={east=2,south=4}} end

  local addrsignal = {name = "signal-black",type = "virtual"}
  local baseaddr = 10000

  local map={
    --[[signal-each stands in for "sum of all signals"]]
    {name='signal-each',type='virtual'}
  }

  for _,v in pairs(game.virtual_signal_prototypes) do if (not v.special) and v.name~="signal-black" then table.insert(map,{name=v.name,type="virtual"}) end end
  for _,f in pairs(game.fluid_prototypes) do table.insert(map,{name=f.name,type="fluid"}) end
  for _,i in pairs(game.item_prototypes)  do if not i.has_flag("hidden") then table.insert(map,{name=i.name,type="item"}) end end

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

  --[[Generate a constant-combinator configured with the desginated data]]
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

  --[[Generate the output filter for a given ROM site]]
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

  --[[Generate a power pole wtih the given connetions]]
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


  function charsig(c)
  	local charmap={
  		["A"]='signal-A',["B"]='signal-B',["C"]='signal-C',["D"]='signal-D',["E"]='signal-E',
  		["F"]='signal-F',["G"]='signal-G',["H"]='signal-H',["I"]='signal-I',["J"]='signal-J',
  		["K"]='signal-K',["L"]='signal-L',["M"]='signal-M',["N"]='signal-N',["O"]='signal-O',
  		["P"]='signal-P',["Q"]='signal-Q',["R"]='signal-R',["S"]='signal-S',["T"]='signal-T',
  		["U"]='signal-U',["V"]='signal-V',["W"]='signal-W',["X"]='signal-X',["Y"]='signal-Y',
  		["Z"]='signal-Z'
  	}
  	return charmap[c]
  end


  pole({x=0,y=0},{})

  local prevIn = {entity_id=1}
  local prevOut = {entity_id=1}

  local xpos = 1
  local ypos = 0
  for i,signal in pairs(map) do

    local filters={
      {index=1,count=-(baseaddr+i),signal=addrsignal}
    }

    local s = string.upper(string.sub(signal.name,1,30))
    local letters = {}
    local i=1
    while s do
      local c
      if #s > 1 then
        c,s=s:sub(1,1),s:sub(2)
      else
        c,s=s,nil
      end
      letters[c]=(letters[c] or 0)+i
      i=i*2
    end

    for c,i in pairs(letters) do
      local sig = charsig(c)
      if sig then
        filters[#filters+1]={
          index=#filters+1,
          count=i,signal={name=sig,type="virtual"}
        }
      end
    end

    local cc_id = count
    local cc,extrafilters
    cc,extrafilters = CC({x=xpos,y=ypos-1},dir,filters)
    while extrafilters and #extrafilters > 0 do
      cc,extrafilters = extendCC(cc,extrafilters)
    end
    filterDC({x=xpos,y=ypos+0.5},dir,
      {["1"]={green={prevIn},red={{entity_id=cc_id}}},["2"]={red={prevOut}}})
    prevIn = {entity_id=count-1,circuit_id=1}
    prevOut = {entity_id=count-1,circuit_id=2}
    xpos=xpos+1
    if xpos > 100 then
      xpos = xpos-100
      ypos = ypos+4
    end
  end

  --[[ Assemble and return blueprint ]]
  local blueprintData = {
    entities = entities,
    icons={
      {index=1, signal={type="item",name="constant-combinator"}}
    },
    name = "StringGen",
    version = "0.2.5"
  }

  return blueprintData

end
