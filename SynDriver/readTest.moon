
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

	%- Reading value from console and saving it into a -%
	jl R15,getint
	sw a(R0),R1

	%- Comparing variables a less than b -%
	lw R1,a(R0)
	lw R2,b(R0)
	clt R3,R1,R2
	sw temp_1(R0),R3

	%- Start branch for if statement -%
	lw R3,temp_1(R0)
	bz R3,else1

	%- Printing int 0 -%
	addi R1,R0,0
	jl R15,putint

	%- Start branch for else statement -%
	j endif1
else1

	%- Printing int 1 -%
	addi R1,R0,1
	jl R15,putint

	%- End if statement -%
endif1

	hlt

a	res 4
b	res 4
lit_0 res 4
lit_1 res 4
temp_1 res 4
