`justarandomgeek's CPU Mk2.book.lua`: a Foreman Exported Book of all the modules required to build the CPU. Link matching colored signals as described below.  
`ROMGen.blueprint.lua`: a lua script which can be used with Foreman (select "allow scripts") to quickly build program ROMs.  
[`Factorio CPU.gsheet`](https://docs.google.com/spreadsheets/d/1rZuvIk4y2Q7IRc290Hhj-6zExVZ4t4LZj9GsXv5fzfg/edit?usp=sharing): An assembler implemented in Google Sheet, which produces copy/pastable data for ROMGen.

[![Image](http://static.justarandomgeek.com/factorio/screenshot+2016+07+15_800.png)  
(Click to embiggen)](http://static.justarandomgeek.com/factorio/screenshot+2016+07+15.png)  

## Machine Structure

#### Main Bus Signals

Major signals are labeled with floor concrete. Left side for green wire, right side for red wire. Power wires run on substations, signals wires run on poles.

* Purple: RAM Read
	* r: Read Response
	* g: Read Request
* Magenta: RAM Write
	* r: Write Request
* Blue:
	* r: Write Register
	* g: Scalar Result (as 'A')
* Cyan:
	* r: Vector Result
	* g: R2
* Green:
	* r: R1::s
	* g: R1
* Yellow:
	* r: To PC
	* g: Payload
* Orange:
	* r: Status
	* g: Op Pulse

* Hazard: Wire IO Port
* FireHazard: Wire Registers

#### Machine Blocks

* White: PC update, Instruction Fetch & Decode
* White: IO Wire Port
* Black: Registers
* Black: Register Copy
* Red: Vector ALU
* Orange: Scalar ALU
* Blue: Scalar Pick & Insert
* Purple: Memory Controller
* Purple: RAM 
* Magenta: Flow Control
* Yellow+Hazard: Debug controls
* Hazard Border: Ops that run very long, like memset
	* Purple: Memset
	* Magenta: Memory Block operations


## Opcode Structure

Opcodes are stored on the Blue signal.

    Op|sr| s|R2|R1
     7| 8| 8| 4| 4

* Op: 7bit Instruction select
* sr: 8bit Scalar Select for return
* s : 8bit Scalar Select for input
* R2,R1: 4bit each, Register Select  

## Machine Timing

* 12 ticks machine update
	* 1 tick debug
		* Possible additional delay if 'Hold' signal set
	* 3 ticks PC Update
	* 2 ticks Instruction Fetch
	* 2 ticks Instruction Decode
	* 2 ticks Register Select
	* 2 ticks Scalar Select
* *n* ticks OP
	* OP timing varies depending on the operation. See timing notes per operation. Most operations 2-5 ticks.

Some operations start small operations at the same time they issue the PC Update, to take place in parallel. This approach is taken with most write operations

## Debug controls

* Auto-Step
	* Set `Yellow` in this combinator to enable automatic slow stepping through a program. Adjust timing in the `<` combinator.
* Step
	* Set and Clear `Yellow` in this combinator to run the cached PC update
* Hold
	* While `Yellow` is set in this combinator, PC update will be cached in the debug cirtuit and ignored by the PC update circuit. When cleared, the cached command is re-issued and program execution continues.
* Manual PC Command
	* Enter a PC update packet here, and Set and Clear `Yellow` to store it to the cache, or begin executing it immediately if not held.
* Clear Cached Command
	* Set Yellow here to clear the stored command.

## Registers
* 0 : null
* 1-10 : physical registers
* 11 : Red Wire
* 12 : Green Wire
* 13 : ALU Result
* 14 : status - PC(blue sq), RET(green sq), decoded instruction (0-4)
* 15 : current op/payload

Registers store an entire circuit network frame, except the 'A' signal. Signal 'A' is used internally for various scalar and flag values throughout the machine, and cannot be stored in registers/ram.

### Red/Green Wire registers
These registers contain the sum of all values recieved on the respective IO wires since the last wire output instruction.

### ALU Result register
The ALU operations add their result to this register. Otherwise, it is identical to registers 1-10.

### Status register
This register reports various internal values of the CPU:

* Blue: PC
* Green: last call's return site
* 0-4: decoded current instruction

### Current OP register
This register returns signals stored with the current instruction in memory. 

## Opcode List

#### 0: Halt
#### 1-40: ALU Operations

2 ticks

The ALU performs every possible operation in parallel, and then adds the requested operation's result to rALU.

 * Vectors output:  
	* 1 : Every = R1::s1
	* 2 : Every < R1::s1
	* 3 : Every > R1::s1
	* 4 : Any = R1::s1
	* 5 : Any < R1::s1
	* 6 : Any > R1::s1
	* 7 : Each = R1::s1
	* 8 : Each < R1::s1
	* 9 : Each > R1::s1
	* 10: Each - R1::s1
	* 11: Each + R1::s1
	* 12: Each / R1::s1
	* 13: Each * R1::s1
	*  14: R1::s1 = ::B
	* 15: R1::s1 < ::B
	* 16: R1::s1 > ::B
 * Scalar output to ALU::sr  
	* 17: Every = R1::s
	* 18: Every < R1::s
	* 19: Every > R1::s
	* 20: Any = R1::s
	* 21: Any < R1::s
	* 22: Any > R1::s
	* 23: Each = R1::s
	* 24: Each < R1::s
	* 25: Each > R1::s
	* 26: Each - R1::s
	* 27: Each + R1::s
	* 28: Each / R1::s
	* 29: Each * R1::s
	* 30: R1::s = ::B
	* 31: R1::s < ::B
	* 32: R1::s > ::B
	* 33: R1::s - B
	* 34: R1::s + B
	* 35: R1::s / B
	* 36: R1::s * B

#### 41-49 Memory and I/O Operations
* 41 : Wire Signal
	* 3 ticks
	* R1=>Red Wire
	* R2=>Green Wire
	* 0=>r11,r12
* 42 : register copy
	* 3 ticks
	*  `R1=>R2`
* 43 : Read from RAM
	* 5 ticks
	* `[R1::s]=>R2`
* 44 : read with pre-dec **requires sr==s**
	* 5 ticks
	* `[--R1::s]=>R2`
* 45 : read with post-inc (pop) **requires sr==s**
	* 5 ticks
	* `[R1::s++]=>R2`
* 46 : Write to RAM
	* 5 ticks
	* `R2=>[R1::s]`
* 47 : write with pre-dec (push) **requires sr==s**
	* 5 ticks
	* `R2=>[--R1::s]`
* 48 : write with post-inc (append) **requires sr==s**
	* 5 ticks
	* `R2=>[R1::s++]`
* 49 : memset - write block of `sr` cells starting at `R1::s`
	* 2+sr ticks
	* R2=>[R1::s - R1::s+(sr-1)]

Post-increment and Pre-decrement modes are implemented as parallel memory and register-update operations, and require sr==s to function properly.



#### 50-59 Control Flow
* 51: jmp
* 52: call
* 53: rjmp
* 54: rcall
* 55: branch
* 56: branchcall

`jmp`,`rjmp`,`call`,`rcall` 2 ticks
`branch`,`branchcall` 4 ticks

`call` variants store a return site in the Green signal of the status register. For `call` and `rcall` this is PC+1, for `branchcall` it is PC+4.

`jmp` and `call` execute the next instruction from the address R1::s.  
`rjmp` and `rcall` execute the next instruction from the address PC+R1::s.  
`branch` and `branchcall` compare R1::s to the raw value of `sr`, and makes the following jumps:  

 * R1::s1=sr PC+1
 * R1::s1<sr PC+2
 * R1::s1>sr PC+3

#### 60-69 Misc
* 60 : memcpy: Copy from one block of memory / memory-mapped register to another.
	* R1: Copy parameters
		* S: 1=Increment Source
		* D: 1=Increment Destination
		* N: 1=Counted 0=Null-terminated ("String")
		* W: Extra wait time
	* R2: Copy job
		* S: Source address
		* D: Destination address
		* N: Count of cells to copy, if counted mode
	* Returns R2:
		* S: Next Source / Source Terminator address
		* D: Next Destination address  
 
 
#### Future Expansion:
* Slice/Unslice: useful for self-modifying code, can assemble/decompose opcodes
	* Slice: R1::s and field sizes on R2::0..6 => fields R1::0..6 (Vector result)
	* Unslice: R1::0..6 and field sizes on R2::0..6 => R1::sr (Scalar Result)


## IO Devices

### Display Unit 

Based on [Xeteth's Scrolling Display](https://www.reddit.com/r/factorio/comments/4qlpr2/xeteths_scrolling_coloured_display_bp_string_in/). 

ROMGen will automatically map strings in the string table to the correct signals. 

Display operations are performed by writing to the display's memory address (default 1001). If iocpy'ing a string that contains multiple types of commands, use the longest W value (or copy in chunks to adjust W)

#### Operations
* `rail`+Column Data, iocpy W=6 : Print a single column of pixel data. This is probably buggy, especially with color.
* `medium-electric-pole`+Character+Color, iocpy W=7 : Print character from Character ROM 
	* 4 ticks - Character ROM and Loop Setup
	* 7 * (3 ticks) - column loop
		* parallel:
			* Increment pole
			* Print Column - this appears to have a minimum timing of 1/3ticks.
		* rail*pole
		* pole>0 :> pole=1 
* `inserter`=1 : Clear Display. 
* `filter-inster`=n iocpy W=20 or 24: Display integer digit. W=20 if digits after end of string, 24 if mixed. Currently no color support.

#### Column Data

Column data is stored as a color for the whole column plus a collection of on/off signals for each lamp.

Top to bottom:

* transport-belt
* fast-transport-belt
* express-transport-belt
* underground-belt
* fast-underground-belt
* express-underground-belt
* splitter
* fast-splitter


#### Character ROM

The character ROM stores 5x8px characters as binary encoded arrays of column data (LSB = Left Column). Characters from ROM are printed with an empty column before and after them.

Supported Character Signals:

* "A" : `alien-artifact`
* "B" - "Z" : `signal-B` - `signal-Z`
* " " (space") : `medium-electric-pole`
* "!" : `signal-grey`
* "_" : `signal-black` 
