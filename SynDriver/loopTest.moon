	entry

	%- Storing literal lit_0 -%
	addi R1,R0,0
	sw lit_0(R0),R1

	%- Assigning variable a -%
	lw R1,lit_0(R0)
	sw a(R0),R1

	%- Storing literal lit_1 -%
	addi R1,R0,9
	sw lit_1(R0),R1

	%- Assigning variable b -%
	lw R1,lit_1(R0)
	sw b(R0),R1
gowhile1

	%- Comparing variables a less than b -%
	lw R1,a(R0)
	lw R2,b(R0)
	clt R3,R1,R2
	sw temp_0(R0),R3

	%- Branch to end while loop -%
	lw R3,temp_0(R0)
	bz R3,endwhile1

	%- Printing value of a -%
	lw R2,a(R0)
	add R1,R0,R2
	jl R15,putint

	%- Adding a and 1 -%
	lw R3,a(R0)
	addi R2,R3,1
	sw temp_1(R0),R2

	%- Storing result of temp_1 in a -%
	lw R2,temp_1(R0)
	sw a(R0),R2
	j gowhile1
endwhile1

	hlt

a	res 4
b	res 4
lit_0 res 4
lit_1 res 4
temp_0 res 4
temp_1 res 4
