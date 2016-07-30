do local script=true
--[[ The line above must start with "do local script" to be recognized as a script]]
local name = "TEST PROGRAM"
local data = {
[1001]={["signal-0"]=58,["signal-R"]=0,["signal-S"]=0,["signal-T"]=13,["signal-U"]=43,["signal-white"]=1009,["signal-V"]=1,["signal-W"]=37,},
[1002]={["signal-0"]=82,["signal-R"]=1,["signal-S"]=37,["signal-V"]=2,},
[1003]={["signal-0"]=50,["signal-R"]=2,["signal-T"]=13,["signal-U"]=43,["signal-white"]=0,["signal-V"]=14,},
[1004]={["signal-0"]=58,["signal-R"]=0,["signal-S"]=0,["signal-T"]=13,["signal-U"]=43,["signal-white"]=1,["signal-A"]=1,["signal-V"]=1,["signal-W"]=37,},
[1005]={["signal-0"]=74,["signal-R"]=1,["signal-S"]=37,["signal-T"]=13,["signal-U"]=43,["signal-white"]=1020,},
[1006]={["signal-0"]=70,["signal-R"]=13,["signal-S"]=44,["signal-grey"]=0,},
[1007]={["signal-0"]=70,["signal-R"]=13,["signal-S"]=44,["signal-grey"]=1002,},
[1008]={["signal-0"]=70,["signal-R"]=13,["signal-S"]=44,["signal-grey"]=0,},
[1009]={["signal-H"]=1,["rail"]=1,},
[1010]={["signal-E"]=1,["rail"]=1,},
[1011]={["signal-L"]=1,["rail"]=1,},
[1012]={["signal-L"]=1,["rail"]=1,},
[1013]={["signal-O"]=1,["rail"]=1,},
[1014]={["rail"]=1,},
[1015]={["signal-W"]=1,["rail"]=1,},
[1016]={["signal-O"]=1,["rail"]=1,},
[1017]={["signal-R"]=1,["rail"]=1,},
[1018]={["signal-L"]=1,["rail"]=1,},
[1019]={["signal-D"]=1,["rail"]=1,},
[1020]={},
[1021]={["signal-0"]=58,["signal-R"]=0,["signal-S"]=0,["signal-T"]=13,["signal-U"]=43,["signal-white"]=1,["signal-V"]=14,["signal-W"]=0,},
}
local types={
  ["rail"]="item"
}

  local addrsignal = "signal-black"
  local dir = 4

  local function signal_type(n) if types[n] then return types[n] else return "virtual" end end
  local function signal(n) return {name=n,type=signal_type(n)} end
  local function filter(i,n,v) return {index=i,count=v,signal=signal(n)} end

  local entities = {}
  local count = 1

  function CC(position, direction, addr, signals, connections)
    local cc = {
      connections = connections,control_behavior={filters={}},
      name = "constant-combinator", direction=direction, position = position
      }
    local i=1
    for n,v in pairs(signals) do
      cc.control_behavior.filters[i]=filter(i,n,v)
      i=i+1
    end
    cc.control_behavior.filters[i]=filter(i,addrsignal,-addr)
    table.insert(entities,cc)
    count = count+1
    return cc
  end

  function filterDC(position,dir,connections,selectsig,selectval,outputsig)
    local dc = {
      control_behavior={
        decider_conditions={
          first_signal=signal(selectsig),
          comparator="=",
          constant=selectval,
          output_signal=signal(outputsig),
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


  pole({x=0,y=0},{})

  local prevIn = {entity_id=1}
  local prevOut = {entity_id=1}

  local xpos = 1
  for addr,ccdata in pairs(data) do

    CC({x=xpos,y=-1},dir,addr,ccdata,{})

    filterDC({x=xpos,y=0.5},dir,
      {["1"]={green={prevIn},red={{entity_id=count-1}}},["2"]={red={prevOut}}},
      addrsignal,0,"signal-everything")
    prevIn = {entity_id=count-1,circuit_id=1}
    prevOut = {entity_id=count-1,circuit_id=2}
    xpos=xpos+1
  end

--[[ Assemble and return blueprint ]]
local blueprintData = {
  entities = entities,
  icons={{index=1, signal={type="item",name="constant-combinator"}}},
  name = "ROM " .. name
}

--[[if game then game.write_file("ROMGEN.lua", serpent.block(blueprintData), false) end]]
return blueprintData

end
