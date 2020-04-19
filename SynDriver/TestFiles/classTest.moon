
	entry

	%- Storing literal lit_0 -%
	addi R1,R0,2
	sw lit_0(R0),R1

	%- Assigning variable a -%
	lw R1,lit_0(R0)
	sw a(R0),R1

	%- Printing value of a -%
	lw R2,a(R0)
	add R1,R0,R2
	jl R15,putint

	%- Storing literal lit_1 -%
	addi R1,R0,3
	sw lit_1(R0),R1

	%- Assigning variable b -%
	lw R1,lit_1(R0)
	sw b(R0),R1

	%- Printing value of b -%
	lw R2,b(R0)
	add R1,R0,R2
	jl R15,putint

	%- Storing literal lit_2 -%
	addi R1,R0,4
	sw lit_2(R0),R1

	%- Assigning variable c_a -%
	lw R1,lit_2(R0)
	sw c_a(R0),R1

	%- Printing value of c_a -%
	lw R2,c_a(R0)
	add R1,R0,R2
	jl R15,putint

	%- Storing literal lit_3 -%
	addi R1,R0,5
	sw lit_3(R0),R1

	%- Assigning variable c_b -%
	lw R1,lit_3(R0)
	sw c_b(R0),R1

	%- Printing value of c_b -%
	lw R2,c_b(R0)
	add R1,R0,R2
	jl R15,putint

	%- Adding variables c_a and c_b -%
	lw R1,c_a(R0)
	lw R2,c_b(R0)
	add R3,R1,R2
	sw temp_1(R0),R3

	%- Storing result of temp_1 in d -%
	lw R3,temp_1(R0)
	sw d(R0),R3

	%- Printing value of d -%
	lw R2,d(R0)
	add R1,R0,R2
	jl R15,putint

	hlt

a	res 4
b	res 4
c res 8
c_a	res 4
c_b	res 4
d	res 4
lit_0 res 4
lit_1 res 4
lit_2 res 4
lit_3 res 4
temp_1 res 4
