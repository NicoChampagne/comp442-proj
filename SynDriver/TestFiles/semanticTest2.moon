
	entry

	%- Computation linear add 1 -%
	lw R1,linear(R0)
	addi R2,R1,1
	sw temp_1(R0),R2

	%- Storing result of temp_1 in a -%
	lw R2,temp_1(R0)
	sw a(R0),R2

	%- Computation linear sub 1 -%
	lw R2,linear(R0)
	subi R1,R2,1
	sw temp_2(R0),R1

	%- Storing result of temp_2 in a -%
	lw R1,temp_2(R0)
	sw a(R0),R1

	%- Assigning variable a -%
	lw R1,linear(R0)
	sw a(R0),R1

	%- Assigning variable a -%
	lw R1,linear(R0)
	sw a(R0),R1

	%- Assigning variable a -%
	lw R1,linear(R0)
	sw a(R0),R1

	%- Storing literal lit_0 -%
	addi R1,R0,1
	sw lit_0(R0),R1

	%- Assigning variable quad -%
	lw R1,lit_0(R0)
	sw quad(R0),R1

	%- Assigning variable quad -%
	lw R1,linear(R0)
	sw quad(R0),R1

	hlt

quad res 8
quad_a	res 4
quad_b	res 4
linear res 8
linear_a	res 4
linear_b	res 4
a	res 4
temp_1 res 4
temp_2 res 4
lit_0 res 4
