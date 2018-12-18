cmake_minimum_required (VERSION 3.0)
project (3DFractals)

find_package(OpenGL REQUIRED)

add_subdirectory (external)

include_directories(
	external/glfw/include/
	external/glm/
	external/glew/include/
	.
)

add_definitions(
	-DTW_STATIC
	-DTW_NO_LIB_PRAGMA
	-DTW_NO_DIRECT3D
	-DGLEW_STATIC
	-D_CRT_SECURE_NO_WARNINGS
)

add_executable(fractal 
	src/main.cpp
	src/loader.cpp
	src/loader.hpp
)

target_link_libraries(fractal
   ${OPENGL_LIBRARY}
   glfw
   glew
)
