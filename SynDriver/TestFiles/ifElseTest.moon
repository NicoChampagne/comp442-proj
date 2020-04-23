
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

	%- Computation a clt b -%
	lw R1,a(R0)
	lw R2,b(R0)
	clt R3,R1,R2
	sw temp_1(R0),R3

	%- Start branch for if statement -%
	lw R3,temp_1(R0)
	bz R3,else1

	%- Printing value of a -%
	lw R2,a(R0)
	add R1,R0,R2
	jl R15,putint

	%- Start branch for else statement -%
	j endif1
else1

	%- Printing value of b -%
	lw R2,b(R0)
	add R1,R0,R2
	jl R15,putint

	%- End if statement -%
endif1

	%- Computation b clt a -%
	lw R3,b(R0)
	lw R2,a(R0)
	clt R1,R3,R2
	sw temp_2(R0),R1

	%- Start branch for if statement -%
	lw R1,temp_2(R0)
	bz R1,else2

	%- Printing value of a -%
	lw R2,a(R0)
	add R1,R0,R2
	jl R15,putint

	%- Start branch for else statement -%
	j endif2
else2

	%- Printing value of b -%
	lw R2,b(R0)
	add R1,R0,R2
	jl R15,putint

	%- End if statement -%
endif2

	hlt

a	res 4
b	res 4
lit_0 res 4
lit_1 res 4
temp_1 res 4
temp_2 res 4
