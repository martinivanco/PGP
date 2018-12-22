#include <chrono>
#include <stdio.h>
#include <stdlib.h>
#include <chrono>

#include <GL/glew.h>
#include <GLFW/glfw3.h>
GLFWwindow* window;
#include <glm/glm.hpp>
using namespace glm;

#include "loader.hpp"

int main( void ) {
	if(!glfwInit()) {
		fprintf(stderr, "Failed to initialize GLFW\n");
		getchar();
		return -1;
	}

	glfwWindowHint(GLFW_SAMPLES, 4);
	glfwWindowHint(GLFW_RESIZABLE,GL_FALSE);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
	glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE); // To make MacOS happy
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

	window = glfwCreateWindow(800, 600, "3D Fractal", NULL, NULL);
	if(window == NULL) {
		fprintf( stderr, "Failed to open GLFW window\n" );
		getchar();
		glfwTerminate();
		return -1;
	}
	glfwMakeContextCurrent(window);

	// Initialize GLEW
	if (glewInit() != GLEW_OK) {
		fprintf(stderr, "Failed to initialize GLEW\n");
		getchar();
		glfwTerminate();
		return -1;
	}

	glfwSetInputMode(window, GLFW_STICKY_KEYS, GL_TRUE);

	glClearColor(0.0f, 0.0f, 0.0f, 0.0f);

	GLuint VertexArrayID;
	glGenVertexArrays(1, &VertexArrayID);
	glBindVertexArray(VertexArrayID);

	GLuint programID = LoadShaders("src/fractal.vs", "src/fractal.fs");

	static const GLfloat vertices[] = {
		-1.0f,  1.0f, 0.0f,
		-1.0f, -1.0f, 0.0f,
		 1.0f,  1.0f, 0.0f,
		 1.0f, -1.0f, 0.0f
	};

	static const GLshort uvs[] = {
		  0,   0,
		  0, 600,
		800,   0,
		800, 600
	};

	GLuint vertexbuffer;
	glGenBuffers(1, &vertexbuffer);
	glBindBuffer(GL_ARRAY_BUFFER, vertexbuffer);
	glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

	GLuint uvbuffer;
	glGenBuffers(1, &uvbuffer);
	glBindBuffer(GL_ARRAY_BUFFER, uvbuffer);
	glBufferData(GL_ARRAY_BUFFER, sizeof(uvs), uvs, GL_STATIC_DRAW);

	static const unsigned short indices[] = {0, 1, 2, 3};
	GLuint indexbuffer;
	glGenBuffers(1, &indexbuffer);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexbuffer);
	glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);

	glUseProgram(programID);
	GLuint camID = glGetUniformLocation(programID, "cameraPosition");
	GLuint tlID = glGetUniformLocation(programID, "tlCoord");
	GLuint xStepID = glGetUniformLocation(programID, "xStep");
	GLuint yStepID = glGetUniformLocation(programID, "yStep");

	std::chrono::time_point<std::chrono::system_clock> start = std::chrono::system_clock::now();
	do {
		glClear(GL_COLOR_BUFFER_BIT);

		glUseProgram(programID);
		std::chrono::duration<double> diff = std::chrono::system_clock::now() - start;
		float modelTime = diff.count() * 0.2;

		float z = -sin(modelTime * 0.5) * 2.0;
		vec3 camPos = vec3(cos(modelTime) * 2.0, z, sin(modelTime) * 2.0);
	  	glUniform3f(camID, camPos.x, camPos.y, camPos.z);
		vec3 normal = normalize(-camPos);
		float distance = 1.25 + sin(modelTime) * 0.75;
		vec3 left = normalize(cross(vec3(0, 1, 0), normal)); left *= distance;
		vec3 up = normalize(cross(normal, left)); up *= distance;
		vec3 topLeft = left; topLeft *= (4.0 / 3.0); topLeft += up - camPos;
		glUniform3f(tlID, topLeft.x, topLeft.y, topLeft.z);
		glUniform3f(xStepID, -left.x, -left.y, -left.z);
		glUniform3f(yStepID, -up.x, -up.y, -up.z);
		
		glEnableVertexAttribArray(0);
		glBindBuffer(GL_ARRAY_BUFFER, vertexbuffer);
		glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 0, (void*)0);

		glEnableVertexAttribArray(1);
		glBindBuffer(GL_ARRAY_BUFFER, uvbuffer);
		glVertexAttribPointer(1, 2, GL_SHORT, GL_FALSE, 0, (void*)0);

		glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexbuffer);

		glDrawElements(GL_TRIANGLE_STRIP, sizeof(indices) / sizeof(unsigned short), GL_UNSIGNED_SHORT, (void*)0);

		glDisableVertexAttribArray(0);
		glDisableVertexAttribArray(1);

		glfwSwapBuffers(window);
		glfwPollEvents();
	}
	while(glfwGetKey(window, GLFW_KEY_ESCAPE ) != GLFW_PRESS && glfwWindowShouldClose(window) == 0);

	glfwTerminate();

	return 0;
}

