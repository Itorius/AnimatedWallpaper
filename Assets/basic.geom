﻿#version 330 core

layout (points) in;
layout (triangle_strip, max_vertices = 4) out;

void main() {    
    vec4 basePos = gl_in[0].gl_Position;
    
    gl_Position = basePos + vec4(-0.005, -0.005, 0.0, 0.0); 
    EmitVertex();

    gl_Position = basePos + vec4( 0.005, -0.005, 0.0, 0.0);
    EmitVertex();

    gl_Position = basePos + vec4( -0.005, 0.005, 0.0, 0.0);
    EmitVertex();
 
    gl_Position = basePos + vec4( 0.005, 0.005, 0.0, 0.0);
    EmitVertex();
 
    EndPrimitive();
}  