	entry

	%- Storing literal lit_0 -%
	addi R1,R0,6
	sw lit_0(R0),R1

	%- Assigning variable a -%
	lw R1,lit_0(R0)
	sw a(R0),R1

	%- Printing value of a -%
	lw R2,a(R0)
	add R1,R0,R2
	jl R15,putint

	%- Storing literal lit_1 -%
	addi R1,R0,2
	sw lit_1(R0),R1

	%- Assigning variable b -%
	lw R1,lit_1(R0)
	sw b(R0),R1

	%- Printing value of b -%
	lw R2,b(R0)
	add R1,R0,R2
	jl R15,putint

	%- Adding a and 2 -%
	lw R1,a(R0)
	addi R2,R1,2
	sw temp_0(R0),R2

	%- Storing result of temp_0 in c -%
	lw R2,temp_0(R0)
	sw c(R0),R2

	%- Printing value of c -%
	lw R2,c(R0)
	add R1,R0,R2
	jl R15,putint

	%- Adding 2 and a -%
	lw R2,a(R0)
	addi R1,R2,2
	sw temp_1(R0),R1

	%- Storing result of temp_1 in d -%
	lw R1,temp_1(R0)
	sw d(R0),R1

	%- Printing value of d -%
	lw R2,d(R0)
	add R1,R0,R2
	jl R15,putint

	%- Adding literals 2 and 2 -%
	addi R1,R0,2
	addi R2,R1,2
	sw temp_2(R0),R2

	%- Storing result of temp_2 in e -%
	lw R2,temp_2(R0)
	sw e(R0),R2

	%- Printing value of e -%
	lw R2,e(R0)
	add R1,R0,R2
	jl R15,putint

	%- Adding variables b and b -%
	lw R2,b(R0)
	lw R1,b(R0)
	add R3,R2,R1
	sw temp_3(R0),R3

	%- Storing result of temp_3 in f -%
	lw R3,temp_3(R0)
	sw f(R0),R3

	%- Printing value of f -%
	lw R2,f(R0)
	add R1,R0,R2
	jl R15,putint

	%- Adding variables b and b -%
	lw R3,b(R0)
	lw R1,b(R0)
	add R2,R3,R1
	sw temp_4(R0),R2

	%- Adding variables b and temp_4 -%
	lw R2,b(R0)
	lw R1,temp_4(R0)
	add R3,R2,R1
	sw temp_5(R0),R3

	%- Storing result of temp_5 in g -%
	lw R3,temp_5(R0)
	sw g(R0),R3

	%- Printing value of g -%
	lw R2,g(R0)
	add R1,R0,R2
	jl R15,putint

	%- Subtracting 8 and a -%
	lw R3,a(R0)
	subi R1,R3,8
	sw temp_6(R0),R1

	%- Storing result of temp_6 in h -%
	lw R1,temp_6(R0)
	sw h(R0),R1

	%- Printing value of h -%
	lw R2,h(R0)
	add R1,R0,R2
	jl R15,putint

	%- Subtracting a and 2 -%
	lw R1,a(R0)
	subi R3,R1,2
	sw temp_7(R0),R3

	%- Storing result of temp_7 in i -%
	lw R3,temp_7(R0)
	sw i(R0),R3

	%- Printing value of i -%
	lw R2,i(R0)
	add R1,R0,R2
	jl R15,putint

	%- Subtracting variables a and b -%
	lw R3,a(R0)
	lw R1,b(R0)
	sub R2,R3,R1
	sw temp_8(R0),R2

	%- Storing result of temp_8 in ii -%
	lw R2,temp_8(R0)
	sw ii(R0),R2

	%- Printing value of ii -%
	lw R2,ii(R0)
	add R1,R0,R2
	jl R15,putint

	%- Subtracting literals 2 and 2 -%
	addi R2,R0,2
	muli R1,R2,2
	sw temp_9(R0),R1

	%- Storing result of temp_9 in k -%
	lw R1,temp_9(R0)
	sw k(R0),R1

	%- Printing value of k -%
	lw R2,k(R0)
	add R1,R0,R2
	jl R15,putint

	%- Subtracting 2 and b -%
	lw R1,b(R0)
	muli R2,R1,2
	sw temp_10(R0),R2

	%- Storing result of temp_10 in l -%
	lw R2,temp_10(R0)
	sw l(R0),R2

	%- Printing value of l -%
	lw R2,l(R0)
	add R1,R0,R2
	jl R15,putint

	%- Subtracting variables a and b -%
	lw R2,a(R0)
	lw R1,b(R0)
	mul R3,R2,R1
	sw temp_11(R0),R3

	%- Storing result of temp_11 in m -%
	lw R3,temp_11(R0)
	sw m(R0),R3

	%- Printing value of m -%
	lw R2,m(R0)
	add R1,R0,R2
	jl R15,putint

	%- Subtracting variables a and b -%
	lw R3,a(R0)
	lw R1,b(R0)
	div R2,R3,R1
	sw temp_12(R0),R2

	%- Storing result of temp_12 in n -%
	lw R2,temp_12(R0)
	sw n(R0),R2

	%- Printing value of n -%
	lw R2,n(R0)
	add R1,R0,R2
	jl R15,putint

	hlt

a	res 4
b	res 4
c	res 4
d	res 4
e	res 4
f	res 4
g	res 4
h	res 4
i	res 4
ii	res 4
k	res 4
l	res 4
m	res 4
n	res 4
lit_0 res 4
lit_1 res 4
temp_0 res 4
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
