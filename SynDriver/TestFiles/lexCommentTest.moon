
	entry

	%- Storing literal lit_0 -%
	addi R1,R0,1
	sw lit_0(R0),R1

	%- Assigning array variable arr[0] -%
	addi R1,R0,0
	lw R2,lit_0(R0)
	sw arr(R1),R2

	%- Printing value of arr at 0 -%
	addi R3,R0,0
	lw R2,arr(R3)
	add R1,R0,R2
	jl R15,putint

	%- Printing a space -%
	putc R0

	%- Storing literal lit_1 -%
	addi R2,R0,2
	sw lit_1(R0),R2

	%- Assigning array variable arr[1] -%
	addi R2,R0,4
	lw R1,lit_1(R0)
	sw arr(R2),R1

	%- Printing value of arr at 1 -%
	addi R3,R0,4
	lw R2,arr(R3)
	add R1,R0,R2
	jl R15,putint

	%- Printing a space -%
	putc R0

	%- Storing literal lit_2 -%
	addi R1,R0,3
	sw lit_2(R0),R1

	%- Assigning variable int -%
	lw R1,lit_2(R0)
	sw int(R0),R1

	%- Printing value of int -%
	lw R2,int(R0)
	add R1,R0,R2
	jl R15,putint

	%- Printing a space -%
	putc R0

	hlt

arr	res 12
int	res 4
lit_0 res 4
lit_1 res 4
lit_2 res 4
