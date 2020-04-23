
	entry

	%- Storing literal lit_0 -%
	addi R1,R0,2
	sw lit_0(R0),R1

	%- Assigning array variable arr[0] -%
	addi R1,R0,0
	lw R2,lit_0(R0)
	sw arr(R1),R2

	%- Storing literal lit_1 -%
	addi R2,R0,2
	sw lit_1(R0),R2
	lw R2,lit_1(R0)
	sw arr(R0),R2

	%- Storing literal lit_2 -%
	addi R2,R0,2
	sw lit_2(R0),R2

	%- Assigning array variable arr[1] -%
	addi R2,R0,4
	lw R0,lit_2(R0)
	sw arr(R2),R0

	%- Printing value of arr -%
	lw R3,linear(R0)
	addi R4,R3,4
	lw R2,arr(R4)
	add R1,R0,R2
	jl R15,putint

	%- Printing a space -%
	putc R0

	hlt

linear res 8
linear_a	res 4
linear_b	res 4
arr	res 8
b	res 4
lit_0 res 4
lit_1 res 4
lit_2 res 4
