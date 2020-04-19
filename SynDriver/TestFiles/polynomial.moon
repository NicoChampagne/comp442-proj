
	%- Function definition for POLYNOMIAL -%
POLYNOMIAL

	jr R15

	%- Function definition for LINEAR -%
LINEAR

	%- Storing literal lit_0 -%
	addi R1,R0,0
	sw lit_0(R0),R1

	%- Assigning variable LINEAR_result -%
	lw R1,LINEAR_lit_0(R0)
	sw LINEAR_result(R0),R1

	%- Computation LINEAR_a add LINEAR_b -%
	lw R1,LINEAR_a(R0)
	lw R2,LINEAR_b(R0)
	add R3,R1,R2
	sw temp_1(R0),R3

	%- Storing result of temp_1 in LINEAR_result -%
	lw R3,temp_1(R0)
	sw LINEAR_result(R0),R3

	%- Returning value of LINEAR_result -%
	lw R3,LINEAR_result(R0)
	sw LINEAR_return(R0),R3

	%- Assigning variable LINEAR_result -%
	lw R3,LINEAR_a(R0)
	sw LINEAR_result(R0),R3

	%- Computation LINEAR_result add LINEAR_b -%
	lw R3,LINEAR_result(R0)
	lw R2,LINEAR_b(R0)
	add R1,R3,R2
	sw temp_2(R0),R1

	%- Storing result of temp_2 in LINEAR_result -%
	lw R1,temp_2(R0)
	sw LINEAR_result(R0),R1

	%- Computation LINEAR_result add LINEAR_c -%
	lw R1,LINEAR_result(R0)
	lw R2,LINEAR_c(R0)
	add R3,R1,R2
	sw temp_3(R0),R3

	%- Storing result of temp_3 in LINEAR_result -%
	lw R3,temp_3(R0)
	sw LINEAR_result(R0),R3

	%- Returning value of LINEAR_result -%
	lw R3,LINEAR_result(R0)
	sw LINEAR_return(R0),R3

	%- Assigning variable LINEAR_new_function -%
	lw R3,LINEAR_A(R0)
	sw LINEAR_new_function(R0),R3

	%- Assigning variable LINEAR_new_function -%
	lw R3,LINEAR_B(R0)
	sw LINEAR_new_function(R0),R3

	%- Returning value of LINEAR_new_function -%
	lw R3,LINEAR_new_function(R0)
	sw LINEAR_return(R0),R3

	%- Assigning variable LINEAR_new_function -%
	lw R3,LINEAR_A(R0)
	sw LINEAR_new_function(R0),R3

	%- Assigning variable LINEAR_new_function -%
	lw R3,LINEAR_B(R0)
	sw LINEAR_new_function(R0),R3

	%- Assigning variable LINEAR_new_function -%
	lw R3,LINEAR_C(R0)
	sw LINEAR_new_function(R0),R3

	%- Returning value of LINEAR_new_function -%
	lw R3,LINEAR_new_function(R0)
	sw LINEAR_return(R0),R3

	jr R15

	entry

	%- Assigning variable f1 -%
	lw R3,f1(R0)
	sw f1(R0),R3

	%- Computation build sub 2 -%
	lw R3,build(R0)
	subi R2,R3,2
	sw temp_4(R0),R2

	%- Storing result of temp_4 in f2 -%
	lw R2,temp_4(R0)
	sw f2(R0),R2

	%- Storing literal lit_1 -%
	addi R2,R0,1
	sw lit_1(R0),R2

	%- Assigning variable counter -%
	lw R2,lit_1(R0)
	sw counter(R0),R2
gowhile1

	%- Computation counter clt 10 -%
	lw R2,counter(R0)
	clti R3,R2,10
	sw temp_5(R0),R3

	%- Branch to end while loop -%
	lw R3,temp_5(R0)
	bz R3,endwhile1

	%- Printing value of counter -%
	lw R2,counter(R0)
	add R1,R0,R2
	jl R15,putint

	%- Printing value of f1_evaluate -%
	lw R2,f1_evaluate(R0)
	add R1,R0,R2
	jl R15,putint

	%- Printing value of f2_evaluate -%
	lw R2,f2_evaluate(R0)
	add R1,R0,R2
	jl R15,putint
	j gowhile1
endwhile1

	hlt

POLYNOMIAL_x res 8
LINEAR_x res 8
LINEAR_float res 8
LINEAR_result res 8
lit_0 res 4
temp_1 res 4
LINEAR_float res 8
LINEAR_result res 8
temp_2 res 4
temp_3 res 4
f1 res 16
f1_private	res 0
f1_a	res 8
f1_b	res 8
f2 res 24
f2_private	res 0
f2_a	res 8
f2_b	res 8
f2_c	res 8
counter	res 4
temp_4 res 4
lit_1 res 4
temp_5 res 4
