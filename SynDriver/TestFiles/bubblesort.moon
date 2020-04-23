
	%- Function definition for bubbleSort -%
bubbleSort

	%- Assigning inner function parameter bubbleSort_arr -%
	lw R1,bubbleSort_param1(R0)
	sw bubbleSort_arr(R0),R1

	%- Assigning inner function parameter bubbleSort_size -%
	lw R1,bubbleSort_param2(R0)
	sw bubbleSort_size(R0),R1

	%- Assigning variable bubbleSort_n -%
	lw R1,bubbleSort_size(R0)
	sw bubbleSort_n(R0),R1

	%- Storing literal lit_0 -%
	addi R1,R0,0
	sw lit_0(R0),R1

	%- Assigning variable bubbleSort_i -%
	lw R1,lit_0(R0)
	sw bubbleSort_i(R0),R1

	%- Storing literal lit_1 -%
	addi R1,R0,0
	sw lit_1(R0),R1

	%- Assigning variable bubbleSort_k -%
	lw R1,lit_1(R0)
	sw bubbleSort_k(R0),R1

	%- Storing literal lit_2 -%
	addi R1,R0,0
	sw lit_2(R0),R1

	%- Assigning variable bubbleSort_temp -%
	lw R1,lit_2(R0)
	sw bubbleSort_temp(R0),R1
bubbleSort_gowhile1

	%- Computation bubbleSort_n sub 1 -%
	lw R1,bubbleSort_n(R0)
	subi R2,R1,1
	sw temp_1(R0),R2

	%- Branch to end while loop -%
	lw R2,temp_1(R0)
	bz R2,bubbleSort_endwhile1

	%- Computation bubbleSort_i add 1 -%
	lw R2,bubbleSort_i(R0)
	addi R1,R2,1
	sw temp_2(R0),R1

	%- Storing result of temp_2 in bubbleSort_temp2 -%
	lw R1,temp_2(R0)
	sw bubbleSort_temp2(R0),R1
bubbleSort_gowhile2

	%- Computation bubbleSort_n sub bubbleSort_temp2 -%
	lw R1,bubbleSort_n(R0)
	lw R2,bubbleSort_temp2(R0)
	sub R3,R1,R2
	sw temp_3(R0),R3

	%- Branch to end while loop -%
	lw R3,temp_3(R0)
	bz R3,bubbleSort_endwhile2

	%- Computation bubbleSort_k add 1 -%
	lw R3,bubbleSort_k(R0)
	addi R2,R3,1
	sw temp_4(R0),R2

	%- Mult value of temp_4 by int size -%
	lw R2,temp_4(R0)
	muli R3,R2,4
	sw temp_6(R0),R3

	%- Setting arr[temp_4] to temp variable temp_5 -%
	lw R2,temp_6(R0)
	lw R3,arr(R2)
	sw temp_5(R0),R3

	%- Retriving value of bubbleSort_k -%
	lw R2,bubbleSort_k(R0)
	sw temp_8(R0),R2

	%- Mult value of temp_8 by int size -%
	lw R2,temp_8(R0)
	muli R3,R2,4
	sw temp_9(R0),R3

	%- Setting arr[0] to temp variable temp_7 -%
	lw R2,temp_9(R0)
	lw R3,arr(R2)
	sw temp_7(R0),R3

	%- Computation temp_7 cgt temp_5 -%
	lw R2,temp_7(R0)
	lw R3,temp_5(R0)
	cgt R1,R2,R3
	sw temp_10(R0),R1

	%- Start branch for if statement -%
	lw R1,temp_10(R0)
	bz R1,bubbleSort_else1

	%- Assigning variable bubbleSort_temp -%
	lw R1,bubbleSort_arr(R0)
	sw bubbleSort_temp(R0),R1

	%- Computation bubbleSort_k add 1 -%
	lw R1,bubbleSort_k(R0)
	addi R3,R1,1
	sw temp_11(R0),R3

	%- Mult value of temp_11 by int size -%
	lw R3,temp_11(R0)
	muli R1,R3,4
	sw temp_13(R0),R1

	%- Setting arr[temp_11] to temp variable temp_12 -%
	lw R3,temp_13(R0)
	lw R1,arr(R3)
	sw temp_12(R0),R1
	lw R3,temp_12(R0)
	sw bubbleSort_arr(R0),R3
	lw R3,bubbleSort_temp(R0)
	sw bubbleSort_arr(R0),R3

	%- Start branch for else statement -%
	j bubbleSort_endif1
bubbleSort_else1

	%- End if statement -%
bubbleSort_endif1

	%- Computation bubbleSort_k add 1 -%
	lw R3,bubbleSort_k(R0)
	addi R0,R3,1
	sw temp_14(R0),R0

	%- Storing result of temp_14 in bubbleSort_k -%
	lw R0,temp_14(R0)
	sw bubbleSort_k(R0),R0
	j bubbleSort_gowhile2
bubbleSort_endwhile2

	%- Computation bubbleSort_i add 1 -%
	lw R0,bubbleSort_i(R0)
	addi R3,R0,1
	sw temp_15(R0),R3

	%- Storing result of temp_15 in bubbleSort_i -%
	lw R3,temp_15(R0)
	sw bubbleSort_i(R0),R3
	j bubbleSort_gowhile1
bubbleSort_endwhile1

	jr R15

	%- Function definition for printarray -%
printarray

	%- Assigning inner function parameter printarray_arr -%
	lw R3,printarray_param1(R0)
	sw printarray_arr(R0),R3

	%- Assigning inner function parameter printarray_size -%
	lw R3,printarray_param2(R0)
	sw printarray_size(R0),R3

	%- Assigning variable printarray_n -%
	lw R3,printarray_size(R0)
	sw printarray_n(R0),R3

	%- Storing literal lit_3 -%
	addi R3,R0,0
	sw lit_3(R0),R3

	%- Assigning variable printarray_i -%
	lw R3,lit_3(R0)
	sw printarray_i(R0),R3
printarray_gowhile3

	%- Computation printarray_i clt printarray_n -%
	lw R3,printarray_i(R0)
	lw R0,printarray_n(R0)
	clt R0,R3,R0
	sw temp_16(R0),R0

	%- Branch to end while loop -%
	lw R0,temp_16(R0)
	bz R0,printarray_endwhile3

	%- Printing value of printarray_arr -%
	lw R3,printarray_i(R0)
	lw R2,printarray_arr(R3)
	add R1,R0,R2
	jl R15,putint

	%- Printing a space -%
	putc R0

	%- Computation printarray_i add 1 -%
	lw R0,printarray_i(R0)
	addi R0,R0,1
	sw temp_17(R0),R0

	%- Storing result of temp_17 in printarray_i -%
	lw R0,temp_17(R0)
	sw printarray_i(R0),R0
	j printarray_gowhile3
printarray_endwhile3

	jr R15

	entry

	%- Storing literal lit_4 -%
	addi R0,R0,64
	sw lit_4(R0),R0

	%- Assigning array variable arr[0] -%
	addi R0,R0,0
	lw R0,lit_4(R0)
	sw arr(R0),R0

	%- Storing literal lit_5 -%
	addi R0,R0,34
	sw lit_5(R0),R0

	%- Assigning array variable arr[1] -%
	addi R0,R0,4
	lw R0,lit_5(R0)
	sw arr(R0),R0

	%- Storing literal lit_6 -%
	addi R0,R0,25
	sw lit_6(R0),R0

	%- Assigning array variable arr[2] -%
	addi R0,R0,8
	lw R0,lit_6(R0)
	sw arr(R0),R0

	%- Storing literal lit_7 -%
	addi R0,R0,12
	sw lit_7(R0),R0

	%- Assigning array variable arr[3] -%
	addi R0,R0,12
	lw R0,lit_7(R0)
	sw arr(R0),R0

	%- Storing literal lit_8 -%
	addi R0,R0,22
	sw lit_8(R0),R0

	%- Assigning array variable arr[4] -%
	addi R0,R0,16
	lw R0,lit_8(R0)
	sw arr(R0),R0

	%- Storing literal lit_9 -%
	addi R0,R0,11
	sw lit_9(R0),R0

	%- Assigning array variable arr[5] -%
	addi R0,R0,20
	lw R0,lit_9(R0)
	sw arr(R0),R0

	%- Storing literal lit_10 -%
	addi R0,R0,90
	sw lit_10(R0),R0

	%- Assigning array variable arr[6] -%
	addi R0,R0,24
	lw R0,lit_10(R0)
	sw arr(R0),R0

	%- Assigning function parameter printarray_param1 -%
	lw R0,arr(R0)
	sw printarray_param1(R0),R0

	%- Assigning function parameter printarray_param2 -%
	addi R0,R0,7
	sw printarray_param2(R0),R0

	%- Jump to function printarray -%
	jl R15,printarray

	%- Assigning function parameter bubbleSort_param1 -%
	lw R0,arr(R0)
	sw bubbleSort_param1(R0),R0

	%- Assigning function parameter bubbleSort_param2 -%
	addi R0,R0,7
	sw bubbleSort_param2(R0),R0

	%- Jump to function bubbleSort -%
	jl R15,bubbleSort

	%- Assigning function parameter printarray_param1 -%
	lw R0,arr(R0)
	sw printarray_param1(R0),R0

	%- Assigning function parameter printarray_param2 -%
	addi R0,R0,7
	sw printarray_param2(R0),R0

	%- Jump to function printarray -%
	jl R15,printarray

	hlt

bubbleSort_param1 res 4
bubbleSort_param2 res 4
bubbleSort_arr	res 4
bubbleSort_size	res 4
bubbleSort_n	res 4
bubbleSort_i	res 4
bubbleSort_k	res 4
bubbleSort_temp	res 4
bubbleSort_temp2	res 4
lit_0 res 4
lit_1 res 4
lit_2 res 4
temp_1 res 4
temp_2 res 4
temp_3 res 4
temp_4 res 4
temp_5 res 4
temp_6 res 4
temp_7 res 4
temp_8 res 4
temp_9 res 4
temp_10 res 4
temp_11 res 4
temp_12 res 4
temp_13 res 4
temp_14 res 4
temp_15 res 4
printarray_param1 res 4
printarray_param2 res 4
printarray_arr	res 4
printarray_size	res 4
printarray_n	res 4
printarray_i	res 4
lit_3 res 4
temp_16 res 4
temp_17 res 4
arr	res 28
lit_4 res 4
lit_5 res 4
lit_6 res 4
lit_7 res 4
lit_8 res 4
lit_9 res 4
lit_10 res 4
