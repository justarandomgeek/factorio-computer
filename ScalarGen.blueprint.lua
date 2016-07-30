do local script=true
  --[[The line above allows Foreman to recognize this as a script.]]

  if not defines then defines={direction={east=2,south=4}} end

  local addrsignal = {name = "signal-black",type = "virtual"}
  local r1signal = {name = "signal-grey",type = "virtual"}
  local r2signal = {name = "signal-white",type = "virtual"}
  local s1signal = {name = "signal-S",type = "virtual"}
  local s2signal = {name = "signal-U",type = "virtual"}
  local sdsignal = {name = "signal-W",type = "virtual"}
  local srsignal = {name = "signal-grey",type = "virtual"}

  local map={
    --[[There's no current way to get these automatically. RSeding said he'd add it though! ]]
    {name='signal-1',type='virtual'},{name='signal-2',type='virtual'},{name='signal-3',type='virtual'},
    {name='signal-4',type='virtual'},{name='signal-5',type='virtual'},{name='signal-6',type='virtual'},
    {name='signal-7',type='virtual'},{name='signal-8',type='virtual'},{name='signal-9',type='virtual'},
    {name='signal-0',type='virtual'},{name='signal-A',type='virtual'},{name='signal-B',type='virtual'},
    {name='signal-C',type='virtual'},{name='signal-D',type='virtual'},{name='signal-E',type='virtual'},
    {name='signal-F',type='virtual'},{name='signal-G',type='virtual'},{name='signal-H',type='virtual'},
    {name='signal-I',type='virtual'},{name='signal-J',type='virtual'},{name='signal-K',type='virtual'},
    {name='signal-L',type='virtual'},{name='signal-M',type='virtual'},{name='signal-N',type='virtual'},
    {name='signal-O',type='virtual'},{name='signal-P',type='virtual'},{name='signal-Q',type='virtual'},
    {name='signal-R',type='virtual'},{name='signal-S',type='virtual'},{name='signal-T',type='virtual'},
    {name='signal-U',type='virtual'},{name='signal-V',type='virtual'},{name='signal-W',type='virtual'},
    {name='signal-X',type='virtual'},{name='signal-Y',type='virtual'},{name='signal-Z',type='virtual'},
    {name='signal-red',type='virtual'},{name='signal-green',type='virtual'},{name='signal-blue',type='virtual'},
    {name='signal-yellow',type='virtual'},{name='signal-pink',type='virtual'},{name='signal-cyan',type='virtual'},
    {name='signal-white',type='virtual'},{name='signal-grey',type='virtual'},{name='signal-each',type='virtual'}
  }

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

  function pole(pos,connections)
    local p = {
      connections = connections, entity_number = count, name = "medium-electric-pole", position = pos
    }
    count = count+1
    return p
  end

  function busCol(pos)
    local firstpole = count
    table.insert(entities, pole({x=pos.x,y=pos.y},{}))
    table.insert(entities, pole({x=pos.x,y=pos.y+4},{{green={{entity_id=firstpole}},red={{entity_id=firstpole}}}}))
    table.insert(entities, pole({x=pos.x,y=pos.y+8},{{green={{entity_id=firstpole+1}},red={{entity_id=firstpole+1}}}}))
    return firstpole
  end


  function bus(pos)
    local buspoles={}


    buspoles[1]=busCol({x=pos.x,y=pos.y})
    buspoles[2]=busCol({x=pos.x+1,y=pos.y})
    buspoles[3]=busCol({x=pos.x+2,y=pos.y})
    buspoles[4]=busCol({x=pos.x+3,y=pos.y})

    local xpos = pos.x+3.5
    local ypos = pos.y-1

    local op1 = {entity_id=count,circuit_id=2}
    table.insert(entities, convertAC(
      {x=xpos,y=ypos}, defines.direction.east,
      {["1"] = {green = {{entity_id = buspoles[2]+0}}}},
      s1signal,s1signal))

    ypos=ypos+4
    local op2 = {entity_id=count,circuit_id=2}
    table.insert(entities, convertAC(
      {x=xpos,y=ypos}, defines.direction.east,
      {["1"] = {green = {{entity_id = buspoles[2]+1}}}},
      s2signal,s2signal))

    ypos=ypos+4
    local opr = {entity_id=count,circuit_id=2}
    table.insert(entities, convertAC(
      {x=xpos,y=ypos}, defines.direction.east,
      {["1"] = {green = {{entity_id = buspoles[2]+2}}}},
      sdsignal,addrsignal))

    local r1 = {entity_id=buspoles[4]+0}
    local s1 = {entity_id=buspoles[4]+0}

    local r2 = {entity_id=buspoles[3]+0}
    local s2 = {entity_id=buspoles[4]+1}

    local sr = {entity_id=buspoles[1]+2}
    local vr = {entity_id=buspoles[3]+2}

    return op1,r1,s1,op2,r2,s2,opr,sr,vr
  end


  function doSig(pos,op1,r1,s1,op2,r2,s2,opr,sr,vr,signal,id)
    local xpos = pos.x
    local ypos = pos.y

    local newr1={entity_id=count,circuit_id=1}
    table.insert(entities, convertAC(
      {x=xpos,y=ypos}, defines.direction.south,
      {["1"] = {green = {r1}}},
      signal,r1signal))
    ypos=ypos+2

    local newop1={entity_id=count,circuit_id=1}
    local news1={entity_id=count,circuit_id=2}
    table.insert(entities, filterDC(
      {x=xpos,y=ypos}, defines.direction.south,
      {["1"] = {green = {op1},red={{entity_id=count-1,circuit_id=2}}},
       ["2"] = {red={s1}}},
      s1signal,id,r1signal))
    ypos=ypos+2

    local newr2={entity_id=count,circuit_id=1}
    table.insert(entities, convertAC(
      {x=xpos,y=ypos}, defines.direction.south,
      {["1"] = {green = {r2}}},
      signal,r2signal))
    ypos=ypos+2

    local newop2={entity_id=count,circuit_id=1}
    local news2={entity_id=count,circuit_id=2}
    table.insert(entities, filterDC(
      {x=xpos,y=ypos}, defines.direction.south,
      {["1"] = {green = {op2},red={{entity_id=count-1,circuit_id=2}}},
       ["2"] = {red={s2}}},
      s2signal,id,r2signal))
    ypos=ypos+2

    local newsr,newopr,newvr
    if signal.name == "signal-each" then
      newsr,newopr,newvr=sr,opr,vr
    else

      newsr={entity_id=count,circuit_id=1}
      table.insert(entities, convertAC(
        {x=xpos,y=ypos}, defines.direction.south,
        {["1"] = {green = {sr}}},
        srsignal,signal))
      ypos=ypos+2

      newopr={entity_id=count,circuit_id=1}
      newvr={entity_id=count,circuit_id=2}
      table.insert(entities, filterDC(
        {x=xpos,y=ypos}, defines.direction.south,
        {["1"] = {green = {opr},red={{entity_id=count-1,circuit_id=2}}},
         ["2"] = {red={vr}}},
        addrsignal,id,signal))
      ypos=ypos+2
    end

    return newop1,newr1,news1,newop2,newr2,news2,newopr,newsr,newvr
  end

  game.remove_path("scalarmap.lua")

  local siglist = {}

  local op1,r1,s1,op2,r2,s2,opr,sr,vr = bus({x=0,y=0})
  local sigcount = 1
  local pos = {x=5,y=-2.5}
  for _,signal in pairs(map) do
    op1,r1,s1,op2,r2,s2,opr,sr,vr = doSig(pos,op1,r1,s1,op2,r2,s2,opr,sr,vr,signal,sigcount)
    table.insert(siglist,{id=sigcount,type=signal.type,name=signal.name})
    sigcount = sigcount + 1
    pos.x = pos.x + 1
    if sigcount % 100 == 0 then
      pos.x = 5
      pos.y = pos.y + 14
      op1,r1,s1,op2,r2,s2,opr,sr,vr = bus({x=0,y=pos.y+2.5})
    end
  end

  game.write_file("scalarmap.lua", serpent.dump(siglist), false)

  --[[ Assemble and return blueprint ]]
  local blueprintData = {
    entities = entities,
    icons={
      {index=1, signal={type="item",name="decider-combinator"}},
      {index=2, signal={type="item",name="constant-combinator"}}
    },
    name = "ScalarGen",
    version = "0.2.5"
  }

  return blueprintData

end
