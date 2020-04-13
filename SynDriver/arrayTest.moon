
	entry

	%- Storing literal lit_0 -%
	addi R1,R0,64
	sw lit_0(R0),R1

	%- Assigning array variable arr[0] -%
	addi R1,R0,0
	lw R2,lit_0(R0)
	sw arr(R1),R2

	%- Storing literal lit_1 -%
	addi R2,R0,34
	sw lit_1(R0),R2

	%- Assigning array variable arr[1] -%
	addi R2,R0,4
	lw R1,lit_1(R0)
	sw arr(R2),R1

	%- Storing literal lit_2 -%
	addi R1,R0,25
	sw lit_2(R0),R1

	%- Assigning array variable arr[2] -%
	addi R1,R0,8
	lw R2,lit_2(R0)
	sw arr(R1),R2

	%- Storing literal lit_3 -%
	addi R2,R0,12
	sw lit_3(R0),R2

	%- Assigning array variable arr[3] -%
	addi R2,R0,12
	lw R1,lit_3(R0)
	sw arr(R2),R1

	%- Storing literal lit_4 -%
	addi R1,R0,22
	sw lit_4(R0),R1

	%- Assigning array variable arr[4] -%
	addi R1,R0,16
	lw R2,lit_4(R0)
	sw arr(R1),R2

	%- Storing literal lit_5 -%
	addi R2,R0,11
	sw lit_5(R0),R2

	%- Assigning array variable arr[5] -%
	addi R2,R0,20
	lw R1,lit_5(R0)
	sw arr(R2),R1

	%- Storing literal lit_6 -%
	addi R1,R0,90
	sw lit_6(R0),R1

	%- Assigning array variable arr[6] -%
	addi R1,R0,24
	lw R2,lit_6(R0)
	sw arr(R1),R2

	%- Printing value of arr at 0 -%
	addi R3,R0,0
	lw R2,arr(R3)
	add R1,R0,R2
	jl R15,putint

	%- Printing value of arr at 1 -%
	addi R3,R0,4
	lw R2,arr(R3)
	add R1,R0,R2
	jl R15,putint

	%- Printing value of arr at 2 -%
	addi R3,R0,8
	lw R2,arr(R3)
	add R1,R0,R2
	jl R15,putint

	%- Printing value of arr at 3 -%
	addi R3,R0,12
	lw R2,arr(R3)
	add R1,R0,R2
	jl R15,putint

	%- Printing value of arr at 4 -%
	addi R3,R0,16
	lw R2,arr(R3)
	add R1,R0,R2
	jl R15,putint

	%- Printing value of arr at 5 -%
	addi R3,R0,20
	lw R2,arr(R3)
	add R1,R0,R2
	jl R15,putint

	%- Printing value of arr at 6 -%
	addi R3,R0,24
	lw R2,arr(R3)
	add R1,R0,R2
	jl R15,putint

	%- Setting arr[4] to temp variable temp_1 -%
	addi R2,R0,16
	lw R1,arr(R2)
	sw temp_1(R0),R1

	%- Setting arr[5] to temp variable temp_2 -%
	addi R2,R0,20
	lw R1,arr(R2)
	sw temp_2(R0),R1

	%- Adding variables temp_2 and temp_1 -%
	lw R2,temp_2(R0)
	lw R1,temp_1(R0)
	add R3,R2,R1
	sw temp_3(R0),R3

	%- Storing result of temp_3 in complex -%
	lw R3,temp_3(R0)
	sw complex(R0),R3

	%- Printing value of complex -%
	lw R2,complex(R0)
	add R1,R0,R2
	jl R15,putint

	%- Adding literals 1 and 3 -%
	addi R3,R0,1
	addi R1,R3,3
	sw temp_4(R0),R1

	%- Mult value of temp_4 by int size -%
	lw R1,temp_4(R0)
	muli R3,R1,4
	sw temp_6(R0),R3

	%- Setting arr[temp_4] to temp variable temp_5 -%
	lw R1,temp_6(R0)
	lw R3,arr(R1)
	sw temp_5(R0),R3

	%- Adding literals 2 and 3 -%
	addi R1,R0,2
	addi R3,R1,3
	sw temp_7(R0),R3

	%- Mult value of temp_7 by int size -%
	lw R3,temp_7(R0)
	muli R1,R3,4
	sw temp_9(R0),R1

	%- Setting arr[temp_9] to temp variable temp_8 -%
	lw R3,temp_9(R0)
	lw R1,arr(R3)
	sw temp_8(R0),R1

	%- Adding variables temp_8 and temp_5 -%
	lw R3,temp_8(R0)
	lw R1,temp_5(R0)
	add R2,R3,R1
	sw temp_10(R0),R2

	%- Storing result of temp_10 in complex2 -%
	lw R2,temp_10(R0)
	sw complex2(R0),R2

	%- Printing value of complex2 -%
	lw R2,complex2(R0)
	add R1,R0,R2
	jl R15,putint

	hlt

arr	res 28
complex	res 4
complex2	res 4
lit_0 res 4
lit_1 res 4
lit_2 res 4
lit_3 res 4
lit_4 res 4
lit_5 res 4
lit_6 res 4
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
