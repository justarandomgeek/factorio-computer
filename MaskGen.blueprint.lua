do local script=true
  --[[The line above allows Foreman to recognize this as a script.]]

  if not defines then defines={direction={east=2,south=4}} end

  local addrsignal = {name = "signal-black",type = "virtual"}
  local opsignal = {name = "signal-0",type = "virtual"}
  local everysignal = {name = "signal-everything",type = "virtual"}

  local r1signal = {name = "signal-grey",type = "virtual"}
  local r2signal = {name = "signal-white",type = "virtual"}
  local rdsignal = {name = "signal-grey",type = "virtual"}

  local map={}

  for _,v in pairs(game.virtual_signal_prototypes) do if (not v.special) and v.name~="signal-black" then table.insert(map,{name=v.name,type="virtual"}) end end
  for _,f in pairs(game.fluid_prototypes) do table.insert(map,{name=f.name,type="fluid"}) end
  for _,i in pairs(game.item_prototypes)  do if not i.has_flag("hidden") then table.insert(map,{name=i.name,type="item"}) end end

  local entities = {}
  local count = 1

  function filterDC(position,dir,connections,selectsig,selectval,outputsig)
    local dc = {
      control_behavior={decider_conditions={first_signal=selectsig,comparator="=",constant=selectval,output_signal=outputsig,copy_count_from_input=true}},
  	   connections = connections, direction = dir, entity_number = count, name = "decider-combinator", position = position
    }
    count = count+1
    return dc
  end

  function convertAC(position,dir,connections,insig,outsig)
    local ac = {
      control_behavior={arithmetic_conditions={first_signal=insig,operation="+",constant=0,output_signal=outsig}},
      connections = connections, direction = dir, entity_number = count, name = "arithmetic-combinator", position = position
    }
    count = count+1
    return ac
  end

  --[[Generate a constant-combinator configured with the desginated data]]
  function CC(position, direction, filters,connections)
    local cc = {
      connections = (connections or {}) ,control_behavior={filters=filters}, entity_number=count,
      name = "constant-combinator", direction=direction, position = position
      }
    count = count+1
    return cc
  end

  function pole(pos,connections)
    local p = {
      connections = connections, entity_number = count, name = "big-electric-pole", position = pos
    }
    count = count+1
    return p
  end

  function bus(pos,prevpoles)
    local buspoles={}
    if not prevpoles then prevpoles={} end
    local prevconn = {}
    for k,v in pairs(prevpoles) do
      prevconn[k]={
        ["1"] = {red={v},green={v}}
      }
    end
    buspoles[1]={entity_id=count}
    table.insert(entities,pole({x=pos.x+0.5,y=pos.y+0.5},prevconn[1]))
    buspoles[2]={entity_id=count}
    table.insert(entities,pole({x=pos.x+2.5,y=pos.y+0.5},prevconn[2]))
    buspoles[3]={entity_id=count}
    table.insert(entities,pole({x=pos.x+4.5,y=pos.y+0.5},prevconn[3]))


    local r1=buspoles[3]
    local r2=buspoles[2]

    table.insert(entities, filterDC(
      {x=pos.x+3.5,y=pos.y+4}, defines.direction.east,
      {["1"] = {green = {buspoles[1]}}},
      opsignal,61,addrsignal))
    entities[#entities].control_behavior.decider_conditions.copy_count_from_input=false

    table.insert(entities, filterDC(
      {x=pos.x+3.5,y=pos.y+5}, defines.direction.west,
      {["1"] = {red = {{entity_id = count-1,circuit_id=2}}},
       ["2"] = {red = {buspoles[2]}}},
      addrsignal,0,everysignal))

    table.insert(entities,CC(
      {x=pos.x+5,y=pos.y+4}, defines.direction.west,
      {{index=1,count=-1,signal=addrsignal}},
      {["1"] = {red={{entity_id=count-2,circuit_id=2}}}}))

    local mul={entity_id=count-2,circuit_id=1}

    table.insert(entities, filterDC(
      {x=pos.x+3.5,y=pos.y+8}, defines.direction.east,
      {["1"] = {green = {{entity_id = count-3, circuit_id=1}}}},
      opsignal,62,addrsignal))
    entities[#entities].control_behavior.decider_conditions.copy_count_from_input=false

    table.insert(entities, filterDC(
      {x=pos.x+3.5,y=pos.y+9}, defines.direction.west,
      {["1"] = {red = {{entity_id = count-1,circuit_id=2}}},
       ["2"] = {red = {{entity_id = count-3,circuit_id=2}}}},
      addrsignal,0,everysignal))

    table.insert(entities,CC(
      {x=pos.x+5,y=pos.y+8}, defines.direction.west,
      {{index=1,count=-1,signal=addrsignal}},
      {["1"] = {red={{entity_id=count-2,circuit_id=2}}}}))

    local add={entity_id=count-2,circuit_id=1}
    return buspoles,r1,r2,mul,add
  end


  function doSig(pos,r1,r2,mul,add,signal)
    local xpos = pos.x
    local ypos = pos.y

    local newr1={entity_id=count,circuit_id=1}
    table.insert(entities, convertAC(
      {x=xpos,y=ypos}, defines.direction.south,
      {["1"] = {green = {r1}}},
      signal,r1signal))
    ypos=ypos+2

    local newr2={entity_id=count,circuit_id=1}
    table.insert(entities, convertAC(
      {x=xpos,y=ypos}, defines.direction.south,
      {["1"] = {green = {r2}}},
      signal,r2signal))
    ypos=ypos+2

    local mulAC = convertAC(
      {x=xpos,y=ypos}, defines.direction.south,
      {["1"] = {red = {
        {entity_id=count-1,circuit_id=2},
        {entity_id=count-2,circuit_id=2},
      }}},
      r1signal,rdsignal)
    mulAC.control_behavior.arithmetic_conditions.operation="*"
    mulAC.control_behavior.arithmetic_conditions.second_signal=r2signal
    table.insert(entities,mulAC)
    ypos=ypos+2

    local newmul={entity_id=count,circuit_id=2}
    table.insert(entities, convertAC(
      {x=xpos,y=ypos}, defines.direction.south,
      {
        [1] = {red = {{entity_id=count-1,circuit_id=2}}},
        [2] = {green={mul}}
      },
      rdsignal,signal))
    ypos=ypos+2

    local addAC = convertAC(
      {x=xpos,y=ypos}, defines.direction.south,
      {["1"] = {red = {
        {entity_id=count-2,circuit_id=1},
      }}},
      r1signal,rdsignal)
    addAC.control_behavior.arithmetic_conditions.second_signal=r2signal
    table.insert(entities,addAC)
    ypos=ypos+2

    local newadd={entity_id=count,circuit_id=2}
    table.insert(entities, convertAC(
      {x=xpos,y=ypos}, defines.direction.south,
      {
        ["1"] = {red = {{entity_id=count-1,circuit_id=2}}},
        ["2"] = {green={add}}
      },
      rdsignal,signal))
    ypos=ypos+2

    return newr1,newr2,newmul,newadd
  end

  local buspoles,lastr1,lastr2,lastmul,lastadd = bus({x=0,y=0})

  local sigcount = 1
  local pos = {x=6,y=-1.5}
  for _,signal in pairs(map) do
    lastr1,lastr2,lastmul,lastadd = doSig(pos,lastr1,lastr2,lastmul,lastadd,signal)
    sigcount = sigcount + 1
    pos.x = pos.x + 1
    if sigcount % 100 == 0 then
      pos.x = 6
      pos.y = pos.y + 14
      buspoles,lastr1,lastr2,lastmul,lastadd = bus({x=0,y=pos.y+1.5},buspoles)
    end
  end
  --[[ Assemble and return blueprint ]]
  local blueprintData = {
    entities = entities,
    icons={
      {index=1, signal={type="item",name="arithmetic-combinator"}},
      {index=2, signal={type="item",name="constant-combinator"}}
    },
    name = "MaskGen",
    version = "0.2.5"
  }

  return blueprintData

end
