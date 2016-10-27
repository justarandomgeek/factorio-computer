* Data types
  * `frame`
    * A single frame, containing ints mapped by the native signal names.
  * Strings
    * String literals may be used anywhere a table literal would be valid, and will be encoded as a `var` containing a bitstring.
  * Arrays
    * One global
* Declare variables
  * var name
  * var name = {}
  * var name = "string"
  * var name[]
  * var name[] = []
  * var name[5]

* Declare functions
  * function name() ... end
  * function name(`param1`,`param2`,`param3`,`ptr1`,`ptr2`) ... end
  * Return values
    * return
    * return {}
    * return {},{},{},{}
  * Special functions
    * isr function foo()
      * save interrupt return site instead of call return
      * reenable interrups when returning

* Define custom types
  * type typename {field1,field2,field3}
  * typename varname = {1,2,3}
  * varname.field1
  * var foo = localtype{field1=1,field2=2}

* Define external functions/vars
  * require(filename)
  * var @500 nixies[5]
  * function @1010 foobar()


```
sexpr: sexpr [/*-+] sexpr
sexpr: integer
sexpr: vref.fieldref
sexpr: sum(vexpr)
sexpr: sref
sexpr: &vref
sexpr: &aref
sexpr: &aref[sexpr]

sassign: sref = sexpr // expand as sref += sexpr - sref
sassign: sref += sexpr

vexpr: table_literal
vexpr: string_literal
vexpr: vexpr [+-*/] vexpr|sexpr
vexpr: vref
vexpr: *vref.fieldref
vexpr: aref[sexpr]
vexpr: funcref

vassign: vref = vexpr
vassign: vref += vexpr

statement: vassign
statement: sassign
statement: return [sref,...,]vref,...
statement: [multiassign =] funcref


block: statement
block: block block
block: if cond then block [else block] end
block: while cond do block end

funcref: ident(arglist)

arglist: arg,
arg: vexpr
arg: sexpr

funcdef: ident(arglistdef) block end

arglistdef: sdeflist vdeflist
sdeflist: sdef...
vdeflist: vdef...

vdef: typeref [@integer|@register] ident
adef: typeref [@integer] ident[]
sdef: int ident
typedef: type ident {fielddef,...}
fielddef: [@signalname] ident


```



```
type ireg {
  @signal-red    callstack,
  @signal-green  sysdata,
  @signal-blue   appdata1,
  @signal-yellow appdata2,
  @signal-grey   intvect,
}

type sreg {
  @signal-blue  pc,
  @signal-green intreturn,
  @signal-cyan  intrequest,
  @signal-I     intenable,
}

type opcode {
  @signal-0     op,
  @signal-A     acc,
  @signal-I     index,
  @signal-R     r1,
  @signal-S     s1,
  @signal-T     r2,
  @signal-U     s2,
  @signal-V     rd,
  @signal-W     sd,
  @signal-grey  imm1,
  @signal-white imm2,
}

var     @r0   rNull
ireg    @r9   rIndex
var     @r10  rRed
var     @r11  rGreen
sreg    @r12  rStatus
opcode  @r13  rOp
var     @r14  rNixie
var     @r15  rFlanRX
var     @r16  rFlanTX
var     @r17  rKeyboard

var @500 nixies[5]


function MAIN()
  rIndex.pc = 50

  CLRDISP()

  -- Print helloworld
  rNixie = "HELLO WORLD"
  rNixie.signal-grey = 1
  COLORDEMO()

  CLRDISP()
  MATHDEMO(10)

  rNixie.signal-grey = 1

  KEYDEMO()
end

function CLRDISP()
  // Chear Display
  rNixie.signal-grey = 1
  rNixie.signal-grey = 1
  rNixie.signal-grey = 1
  rNixie.signal-grey = 1
  rNixie.signal-grey = 1
end

function READLINE()
  var line
  while rKeyboard.signal-grey == 0 do
  end
  line = rKeyboard
  rKeyboard = rNull
  return line
end

function COLORDEMO()
  // Print a color demo
  rNixie = { signal-red=1, signal-green=2, signal-blue=4, signal-cyan=8, signal-pink=16, signal-yellow=32 }&"COLORS"
  rNixie.signal-grey = 1
  rNixie = {signal-red=7, signal-green=496, signal-blue=61440}&"RED GREEN   BLUE"
  rNixie.signal-grey = 1
  rNixie = {signal-cyan=15, signal-pink=480, signal-yellow=258048}&"CYAN PINK   YELLOW"
end

function MATHDEMO(int i)
  --nn FACTORIAL
  --nn SQUARE
  --ii NUMBER
  var num = "NUMBER"
  num.signal-white = 1
  var square = "SQUARE"
  var factorial = "FACTORIAL"
  factorial.signal-white = 1
  while num.signal-white < i+1 do
    nixies[0]=num

    square.signal-white = num.signal-white * num.signal-white
    nixies[1]=square

    factorial.signal-white = factorial.signal-white * num.signal-white
    nixies[2]=factorial

    num.signal-white += 1
  end
  return
end

function KEYDEMO()
  while rNull.Null == rNull.Null do
    READLINE()
    rNixie.signal-grey=1
  end
end
```
