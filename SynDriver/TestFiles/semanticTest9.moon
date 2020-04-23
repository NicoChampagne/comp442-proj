
	entry

	%- Printing value of line_x -%
	lw R2,line_x(R0)
	add R1,R0,R2
	jl R15,putint

	%- Printing a space -%
	putc R0

	hlt

line res 8
line_x	res 4
line_y	res 4
a	res 4
a	res 4
