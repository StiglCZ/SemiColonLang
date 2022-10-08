# Semicolon language (SCL)

## Language made just by semicolons and  curly brackets

## Instructions:
```
Instruction     Arguments
mov   ;         from, to address					
jump  ;;        command number
if    ;;;       address, operator, address2, where to jump if true
count ;;;;      A, operator, B, out	
print ;;;;;     Input address(ASCII int)
exit  ;;;;;;    Exit code
input ;;;;;;;   outAddress
copy  ;;;;;;;;  intieger(;;;;; = 5), outAddress
```
## SCL Interactive:
```
Launch by:
./scComplier
You can use normal commands as in files but in shell
```

## Hello world:
```
{;;;;;;;;{;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;}{;}};
{;;;;;;;;{;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;}{;;}};
{;;;;;;;;{;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;}{;;;}};
{;;;;;;;;{;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;}{;;;;}};
{;;;;;;;;{;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;}{;;;;;}};
{;;;;;;;;{;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;}{;;;;;;;}};
{;;;;;;;;{;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;}{;;;;;;}};
{;;;;;{;}};
{;;;;;{;;}};
{;;;;;{;;;}};
{;;;;;{;;;}};
{;;;;;{;;;;}};
{;;;;;{;;;;;}};
{;;;;;{;;;;}};
{;;;;;{;;;;;;;}};
{;;;;;{;;;}};
{;;;;;{;;;;;;}};
```
## Arguments
	-o [filename.c] -> Changes name of compiled C file
	-i [filename.sl] -> Can be used for changing input file
	-g [complier] -> Change C compiler name when using C compilation integrated
	-gp [args] -> Arguments of compiler if using integrated one(to make space type _)
	-d -> Automaticly deletes C file after compiling
	-x [filename] -> change filename of executable when automatic executing after compiling c
	-c Configuration utility
## Compilation:
	Make the C code by running:
		./scComplier Program.sl
	Then compile file by:
		gcc Compiled.c
	Run the file:
		./a.out
## Use config files
	Make new config file by:
		./scComplier -c new
	Use it by:
		./scComplier -c [config_name]
	Or use default configuration for building by:
		./scComplier -c default