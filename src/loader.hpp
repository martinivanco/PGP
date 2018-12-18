#include <stdio.h>
#include <string>
#include <vector>
#include <iostream>
#include <fstream>
#include <algorithm>
#include <sstream>
#include <stdlib.h>
#include <string.h>

#include <GL/glew.h>

#ifndef LOADER_HPP
#define LOADER_HPP

GLuint LoadShaders(const char * vertex_file_path,const char * fragment_file_path);

#endif