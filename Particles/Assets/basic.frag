#version 440 core

layout(location = 0) out vec4 color;

uniform vec4 u_Color;
uniform vec2 u_MousePos;

void main()
{
	float dist = distance(gl_FragCoord.xy, u_MousePos);
	
	if(dist > 400) discard;
	
	color = u_Color;
	color.xyz *= 1-(dist/400);
}