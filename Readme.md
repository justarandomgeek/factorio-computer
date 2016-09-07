* `CPU Mk3.book.lua`: a Foreman Exported Book of all the logic modules required to build the CPU. Each module links to the main bus by matching colors as described below.
* `ScalarGen.blueprint.lua`: A Foreman Blueprint Script to generate the Scalar Pick/Return mechanism for the current game's signal list.
* `scalarmap.lua`: A machine descriptor file generated by ScalarGen.
* `compiler.*`: A compiler (in C# and Lua) that transforms source files into blueprint strings for a strips of constant combinators configured as a ROM.


![Image](screenshot_30924151.png)
![Image](screenshot_30933153.png)



## Machine Structure

### Main Bus Signals

Major signals are labeled with floor concrete. In general pairs will be kept together for single-color poles, but when a pole must carry mixed pairs, the left side is colored for the green wire, and the right side for red wire.

For wiring clarity, it is reccomended to remove power wires from poles and only carry power at substations.

| Pole Color | Green Wire | Red Wire |
|------------|------------|----------|
|Purple      | Memory Read Request           | Memory Read Response |
|Magenta     | rIndex                        | Memory Write         |
|Blue        | Scalar Result (`signal-grey`) |                      |
|Red         | Keyboard                      | Keyboard             |
|White       | NixieTerm                     | NixieTerm            |
|Cyan        | R2                            | Vector Result        |
|Green       | R1                            | Scalars              |
|Yellow      | Op                            | To PC                |
|Orange      | Op Pulse                      | Status               |
|Hazard      | IO Wire                       | IO Wire              |
|FireHazard  | IO Wire Register              | IO Wire Register     |

* Scalars
	* signal-grey: R1.s1
	* signal-white: R2.s2
* To PC
	* signal-blue: next
	* signal-green: rjmp to signal-black
	* signal-red: jump to signal-black

### Machine Blocks

The machine is made of several independant functional blocks which each connect to the main bus.

* White: PC update, Instruction Fetch & Decode
* White: IO Wire Port
* Black: NixieTerm
* Black: Registers
* Red: ALU
* Blue: Scalar Pick & Return
* Purple: Memory Controller
* Purple: RAM
* Magenta: Flow Control
* Yellow+Hazard: Debug controls

#### ScalarGen and Scalar Pick & Return

The Scalar Pick & Return mechanism allows operating on individual signals from registers. There are two Pick channels, s1 and s2, and one Return channel sd, selected by the corresponding signals in the current opcode, and operating on the corresponding register selection. Because the signals available will vary depending on what mods are installed, this module is generated by a script, ScalarGen. ScalarGen is executed by pasting it into Foreman's import window as if it were a blueprint string, and will add a blueprint named ScalarGen to your list. It will also produce a file `scalarmap.lua` in your script-output directory listing the numeric mappings for the assembly it has generated, which must be used when compiling programs for your computer.

#### Debug controls

The 'Stepping' button toggles between step and run modes. In step mode, the PC Update commands are cached in the debugger until released by the 'Step' button. The debugger's buffer can also be manipulated directly to adjust program flow, using the four buttons above the command display. In run mode, PC Update commands are run immediately.


#### Registers

Registers store an entire circuit network frame, except `signal-black`. `signal-black` is used internally for various scalar and flag values throughout the machine, and cannot be stored in registers/memory, or expressed correctly in most mechanisms. All registers may be referred to by their number as `r0`,`r1`,`r2`,etc, and some registers may be referred to by name as listed in the table below.


| ID | Name | Purpose |
|----|------|---------|
|0|`rNull`|No Register selected. Returns 0 on every signal.|
|1-4|`r1`-`r4`| General Purpose data registers. Callee saved. |
|5-8|`r5`-`r8`| General Purpose data registers. Callee scratch regisers. |
|9|`rIndex`| Indexing regiser. Supports auto-indexing memory operations.|
|10|`rRed`| IO Wire Red data since list transmitted|
|11|`rGreen`| IO Wire Green data since last transmitted|
|12|`rStat`| CPU Status register <ul><li>`signal-blue`: PC</li><li>`signal-green`: last call's return site</li></ul>|
|13|`rOp`| Current Op data|
|14|`rNixie`| NixieTerm |
|15,16|`rFlanRX`,`rFlanTX`| Wireless masts
|17|`rKeyboard`| Keyboard interface. Reads a single buffered key. Clear buffer with `signal-grey`.
|18+| `r18`-... | IO Expansion ports<br>Aditional devices may be connected to these registers

#### Calling Conventions

Stack 1 is used as the call stack, and `r8` is used in saving/restoring the callsite.
Registers `r1-r4` must be preserved by the callee. Registers `r5-r8` may be used freely.
Stack 2 must be preserved by the callee, Stacks 3-4 may be used freely.

Call arguments and return values are passed in `r5-r8`, starting with `r5`.
Pointer arguments and return values are passed in rIndex stacks 3, then 4.
If more than three frames of data are needed, it may be spilled to the call stack, or to RAM and indicated with a pointer.

### Operations
The following signals are used to select registers and signals:

|Signal  |Purpose|
|--------|-------|
|signal-0|Op|
|signal-A| Accumulate |
|signal-R|R1|
|signal-S|S1|
|signal-T|R2|
|signal-U|S2|
|signal-V|Rd|
|signal-W|Sd|
|signal-grey|R1.S1, Imm1 in ROM, Scalar Result|
|signal-white|R2.S2, Imm2 in ROM|

If Rd is set, the selected register will be cleared as Op Pulse is triggered unless Accumulate is also set (>0), even if the current operation does not actually assign to it. The whole register will be cleared, even in scalar operations.

#### 0: Halt
Any undefined opcode will halt the machine, but Op=0 is specifically reserved for doing so.

#### 1-60: ALU Operations
The ALU performs every possible operation in parallel, and returns the requested operation's result to Scalar Result or Vector Result.

|    |Comparisons|Arithmetic|
|----|-----------|----------|
|Output|<ul><li>0: Vector</li><li>24: Scalar</li></ul>|<ul><li>48: Vector</li><li>52: Scalar</li></ul></td>
|Operator|<ul><li>1:`=`</li><li>2:`<`</li><li>3:`>`</li></ul>|<ul><li>1:`-`</li><li>2:`+`</li><li>3:`/`</li><li>4:`*`</li></ul>
|Output Mode|<ul><li>0:`?=` Input Value</li><li>3:`?1` Flags</li></ul>||
|Input Mode|<ul><li>0: Every</li><li>6: Any</li><li>12: Scalar</li><li>18: Each</li></ul>|<ul><li>0: Each</li><li>4: Scalar</li></ul>

Add one value from each cell of a column to form an instruction.

#### 70: Jump
Jump to R1.s1 if `signal-green`=0 or PC+R1.s1 if `signal-green`=1. Return PC+1 to Rd.Sd.

#### 71: Branch

Returns PC+1 to Rd.sd. Compares R1.s1 to R2.s2, and makes the following jumps:

* `=` PC+rOp.1
* `<` PC+rOp.2
* `>` PC+rOp.3

#### 72: Exec

Execute the contents of R1 as an instruction, at the current PC value.

#### 80: Wire
Write a packet to a two-wire network, and clear the receive registers for a response. To leave either wire untouched, select `rNull` for it. `rRed` and `rGreen` are cleared on the same frame the selected signals are transmitted. Write `rNull` to both wires to clear the receive buffer without transmitting anything.
* R1=>Red Wire
* R2=>Green Wire
* 0=>rRed,rGreen

#### 81: Write Memory
Write the contents of R2 to the memory location or memory-mapped device selected by R1.s1.

* R2 -> [R1.s1]

#### 82: Read Memory
Read the memory location or memory-mapped device selected by R1.s1 into Rd.

* [R1.s1] -> Rd

#### 83: Push
Store a frame to one of the stacks in rIndex. Stacks are selected by `signal-S`, but have a different mapping from usual signals. R1 must also be set to rIndex.

|  S  |Signal        | Usage      |
|-----|--------------|------------|
|  1  |`signal-red`  | Call Stack |
|  2  |`signal-green`| Callee Preserved |
|  3  |`signal-blue` | Callee argument/scratch |
|  4  |`signal-red`  | Callee argument/scratch |

* R2 -> [rIndex.stack-1]
* rIndex.stack--


#### 84: Pop
Retrieve a frame to one of the stacks in rIndex. Stacks are selected as described for Push. R1 must also be set to rIndex.

* [rIndex.stack] -> Rd
* rIndex.stack++


#### 85: (not currently implemented)
Store a frame to an array in one of the index pointers. Arrays are selected as described for Push. R1 must also be set to rIndex.

* R2 -> [rIndex.stack++]

## IO Devices

### NixieTerm

NixieTerm is a multi-line Alpha Nixie display. It can be accessed as an array of bit-packed strings in memory starting at `[500]`, or the lowermost row can be accessed as a `rNixie`, and shifted upward.

Any signals written in register mode will be added to the sum for the lowermost row. Writing `signal-grey` will shift all rows upward and clear the lower row. Color signals may be bit-packed along with characters.

A numeric value may also be written to a row on `signal-white` which will display in the left column of the corresponding display row.

NixieTerm supports all the characters supported by the Alpha Nixie:
* A-Z0-9 as their correspnding signals
* . as `train-stop`
* \- as `fast-splitter`

### Wireless

A cluster of wireless masts are connected on `rFlanRX`/`r15` and `rFlanTX`/`r16`. `rFlanRX` reads the current values on the wireless (including current transmitted signal). `rFlanTX` is a memory cell which is connected to transmit to the wireless. There are 12 masts connected, by default configured to relay all virtual signals except `signal-black`, leaving 16 slots available for custom configuration. Additional masts may be wired in as needed.
