
	%- Function definition for print -%
print

	%- Assigning inner function parameter print_a -%
	lw R1,print_param1(R0)
	sw print_a(R0),R1

	%- Returning value of print_a -%
	lw R1,print_a(R0)
	sw print_return(R0),R1

	jr R15

	entry

	%- Storing literal lit_0 -%
	addi R1,R0,2
	sw lit_0(R0),R1

	%- Assigning variable a -%
	lw R1,lit_0(R0)
	sw a(R0),R1

	%- Storing literal lit_1 -%
	addi R1,R0,1
	sw lit_1(R0),R1

	%- Assigning array variable arr[1] -%
	addi R1,R0,4
	lw R2,lit_1(R0)
	sw arr(R1),R2

	%- Computation 2 add linear -%
	lw R2,linear(R0)
	addi R1,R2,2
	sw temp_1(R0),R1

	%- Computation a add temp_1 -%
	lw R1,a(R0)
	lw R2,temp_1(R0)
	add R3,R1,R2
	sw temp_2(R0),R3

	%- Computation 3 add temp_2 -%
	lw R3,temp_2(R0)
	addi R2,R3,3
	sw temp_3(R0),R2

	%- Storing result of temp_3 in b -%
	lw R2,temp_3(R0)
	sw b(R0),R2

	%- Printing value of b -%
	lw R2,b(R0)
	add R1,R0,R2
	jl R15,putint

	%- Printing a space -%
	putc R0

	%- Assigning array variable arr[0] -%
	addi R2,R0,0
	lw R3,linear(R0)
	sw arr(R2),R3

	%- Assigning function parameter print_param1 -%
	lw R3,a(R0)
	sw print_param1(R0),R3

	%- Jump to function print -%
	jl R15,print

	hlt

print_param1 res 4
print_a	res 4
a	res 4
b	res 4
c	res 4
arr	res 8
linear res 8
linear_a	res 4
linear_b	res 4
lit_0 res 4
lit_1 res 4
temp_1 res 4
temp_2 res 4
temp_3 res 4
