do local script=true
--[[The line above allows Foreman to recognize this as a script.]]

--[[ upper and lower row strings ]]
local strings = { 
"HELLO",
"WORLD",
}

--[[ SignGen Code begins here ]]
local chars={
["A"]='signal-A',["B"]='signal-B',["C"]='signal-C',["D"]='signal-D',["E"]='signal-E', 
["F"]='signal-F',["G"]='signal-G',["H"]='signal-H',["I"]='signal-I',["J"]='signal-J', 
["K"]='signal-K',["L"]='signal-L',["M"]='signal-M',["N"]='signal-N',["O"]='signal-O', 
["P"]='signal-P',["Q"]='signal-Q',["R"]='signal-R',["S"]='signal-S',["T"]='signal-T', 
["U"]='signal-U',["V"]='signal-V',["W"]='signal-W',["X"]='signal-X',["Y"]='signal-Y', 
["Z"]='signal-Z',[" "]='signal-blue',
}


function mapChar(s,i)
  if not s then return chars[' '] end
  
  local c = s:sub(i,i)
  if c and chars[c] then 
    return chars[c]
  else
    return chars[' '] 
  end
end

local count = 1

--[[ CC() Creates a constant combinator with the given data ]]
function CC(position, s1, s2)
  local cc = {
  entity_number = count,
    control_behavior={filters={
      {index=1, count=1, signal={name = mapChar(s1,1), type = 'virtual'}},
      {index=2, count=1, signal={name = mapChar(s1,2), type = 'virtual'}},
      {index=3, count=1, signal={name = mapChar(s2,1), type = 'virtual'}},
      {index=4, count=1, signal={name = mapChar(s2,2), type = 'virtual'}},
    }},
    name = "constant-combinator",
		direction=2,
    position = position 
    }
  count = count+1
  return cc
end 

function prefix(s)
  if not s then
    return nil,nil
  elseif #s<=2 then 
    return s,nil
  else
    return s:sub(1,2), s:sub(3)
  end
end

--[[ Generate ROM cells ]]
local xpos=0
local s1,s2
local entities={}
local name=strings[1] .. "|" .. strings[2]
repeat
  s1,strings[1] = prefix(strings[1])
  s2,strings[2] = prefix(strings[2])
  table.insert(entities, CC({x = xpos+1,y = 0}, s1, s2))
  xpos = xpos + 1
until(not strings[1] and not strings[2])


--[[ Assemble and return blueprint ]]
local blueprintData = {
  entities = entities,
  icons={
    {index=1, signal={type="item",name="constant-combinator"}}
  },
  name = name,
} 
return blueprintData

end