
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

	%- Storing literal lit_2 -%
	addi R1,R0,5
	sw lit_2(R0),R1

	%- Assigning variable c -%
	lw R1,lit_2(R0)
	sw c(R0),R1
gowhile1

	%- Computation a clt b -%
	lw R1,a(R0)
	lw R2,b(R0)
	clt R3,R1,R2
	sw temp_1(R0),R3

	%- Branch to end while loop -%
	lw R3,temp_1(R0)
	bz R3,endwhile1

	%- Storing literal lit_3 -%
	addi R3,R0,1
	sw lit_3(R0),R3

	%- Assigning variable d -%
	lw R3,lit_3(R0)
	sw d(R0),R3
gowhile2

	%- Computation d clt c -%
	lw R3,d(R0)
	lw R2,c(R0)
	clt R1,R3,R2
	sw temp_2(R0),R1

	%- Branch to end while loop -%
	lw R1,temp_2(R0)
	bz R1,endwhile2

	%- Printing value of d -%
	lw R2,d(R0)
	add R1,R0,R2
	jl R15,putint

	%- Printing a space -%
	putc R0

	%- Computation d add 1 -%
	lw R1,d(R0)
	addi R2,R1,1
	sw temp_3(R0),R2

	%- Storing result of temp_3 in d -%
	lw R2,temp_3(R0)
	sw d(R0),R2
	j gowhile2
endwhile2

	%- Printing value of a -%
	lw R2,a(R0)
	add R1,R0,R2
	jl R15,putint

	%- Printing a space -%
	putc R0

	%- Computation a add 1 -%
	lw R2,a(R0)
	addi R1,R2,1
	sw temp_4(R0),R1

	%- Storing result of temp_4 in a -%
	lw R1,temp_4(R0)
	sw a(R0),R1
	j gowhile1
endwhile1

	hlt

a	res 4
b	res 4
c	res 4
d	res 4
lit_0 res 4
lit_1 res 4
lit_2 res 4
temp_1 res 4
lit_3 res 4
temp_2 res 4
temp_3 res 4
temp_4 res 4
