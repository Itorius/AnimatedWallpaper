#version 330 core

layout (points) in;
layout (triangle_strip, max_vertices = 4) out;

uniform mat4 u_ViewProjection;
uniform int size;

void main() {    
    vec4 basePos = gl_in[0].gl_Position;
    
    gl_Position = u_ViewProjection *(basePos + vec4(-size, -size, 0.0, 0.0)); 
    EmitVertex();

    gl_Position = u_ViewProjection *(basePos + vec4( -size, size, 0.0, 0.0));
    EmitVertex();

	  gl_Position = u_ViewProjection *(basePos + vec4(size, -size, 0.0, 0.0)); 
    EmitVertex();

    gl_Position = u_ViewProjection *(basePos + vec4( size, size, 0.0, 0.0));
    EmitVertex();
 

 
    EndPrimitive();
}  