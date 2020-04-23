
	%- Function definition for print -%
print

	%- Assigning inner function parameter print_a -%
	lw R1,print_param1(R0)
	sw print_a(R0),R1

	%- Save R15 contents -%
	add R1,R0,R15
	sw temp_1(R0),R1

	%- Printing value of print_a -%
	lw R2,print_a(R0)
	add R1,R0,R2
	jl R15,putint

	%- Printing a space -%
	putc R0

	%- Unload R15 contents -%
	lw R1,temp_1(R0)
	add R15,R0,R1

	jr R15

	%- Function definition for addition -%
addition

	%- Assigning inner function parameter addition_a -%
	lw R1,addition_param1(R0)
	sw addition_a(R0),R1

	%- Assigning inner function parameter addition_b -%
	lw R1,addition_param2(R0)
	sw addition_b(R0),R1

	%- Computation addition_a add addition_b -%
	lw R1,addition_a(R0)
	lw R2,addition_b(R0)
	add R3,R1,R2
	sw temp_2(R0),R3

	%- Storing result of temp_2 in addition_c -%
	lw R3,temp_2(R0)
	sw addition_c(R0),R3

	%- Save R15 contents -%
	add R1,R0,R15
	sw temp_3(R0),R1

	%- Printing value of addition_c -%
	lw R2,addition_c(R0)
	add R1,R0,R2
	jl R15,putint

	%- Printing a space -%
	putc R0

	%- Unload R15 contents -%
	lw R1,temp_3(R0)
	add R15,R0,R1

	%- Returning value of addition_c -%
	lw R3,addition_c(R0)
	sw addition_return(R0),R3

	jr R15

	entry

	%- Storing literal lit_0 -%
	addi R3,R0,2
	sw lit_0(R0),R3

	%- Assigning variable a -%
	lw R3,lit_0(R0)
	sw a(R0),R3

	%- Assigning function parameter print_param1 -%
	lw R3,a(R0)
	sw print_param1(R0),R3

	%- Jump to function print -%
	jl R15,print

	%- Storing literal lit_1 -%
	addi R3,R0,3
	sw lit_1(R0),R3

	%- Assigning variable b -%
	lw R3,lit_1(R0)
	sw b(R0),R3

	%- Assigning function parameter print_param1 -%
	lw R3,b(R0)
	sw print_param1(R0),R3

	%- Jump to function print -%
	jl R15,print

	%- Assigning function parameter addition_param1 -%
	lw R3,a(R0)
	sw addition_param1(R0),R3

	%- Assigning function parameter addition_param2 -%
	lw R3,b(R0)
	sw addition_param2(R0),R3

	%- Jump to function addition -%
	jl R15,addition

	%- Assigning return variable to c -%
	lw R3,addition_return(R0)
	sw c(R0),R3

	%- Printing value of c -%
	lw R2,c(R0)
	add R1,R0,R2
	jl R15,putint

	%- Printing a space -%
	putc R0

	hlt

print_a	res 4
temp_1 res 4
addition_a	res 4
addition_b	res 4
addition_integer	res 4
addition_c	res 4
temp_2 res 4
temp_3 res 4
a	res 4
b	res 4
c	res 4
lit_0 res 4
print_param1 res 4
lit_1 res 4
addition_param1 res 4
addition_param2 res 4
addition_return res 4
