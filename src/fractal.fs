#version 410 core
in vec2 pixelPosition;
out vec3 color;
uniform vec3 cameraPosition;
uniform vec3 tlCoord;
uniform vec3 xStep;
uniform vec3 yStep;

const int MAX_MARCHING_STEPS = 128;
const float MIN_DIST = 0.0;
const float MAX_DIST = 10.0;
const float EPSILON = 0.001;
const vec3 LIGHT1 = vec3(0.577, 0.577, -0.577);
const vec3 LIGHT2 = vec3(-0.707, 0.000, 0.707);
#define ANTI_ALIASING 2

float mandelbulbSDF(in vec3 c, out vec4 color) {
    vec3 w = c;
    float wdist = length(w); // distance of w from origin
    float derivative = 1.0;
    vec4 trap = vec4(abs(w), wdist * wdist);

	for(int i = 0; i < 4; i++) {
		derivative = 8.0 * pow(wdist, 7) * derivative + 1.0;

        float x = w.x; float x2 = x * x; float x4 = x2 * x2;
        float y = w.y; float y2 = y * y; float y4 = y2 * y2;
        float z = w.z; float z2 = z * z; float z4 = z2 * z2;

        float k1 = x4 + y4 + z4 - 6.0 * y2 * z2 - 6.0 * x2 * y2 + 2.0 * z2 * x2;
        float k3 = x2 + z2;
        float k2 = inversesqrt(pow(k3, 7));
        float k4 = x2 - y2 + z2;

        w.x = c.x + 64.0 * x * y * z * (x2 - z2) * k4 * (x4 - 6.0 * x2 * z2 + z4) * k1 * k2;
        w.y = c.y + -16.0 * y2 * k3 * k4 * k4 + k1 * k1;
        w.z = c.z + -8.0 * y * k4 * (x4 * x4 - 28.0 * x4 * x2 * z2 + 70.0 * x4 * z4 - 28.0 * x2 * z2 * z4 + z4 * z4) * k1 * k2;
        
        trap = min(trap, vec4(abs(w), wdist * wdist));
        wdist = length(w);
		if(wdist > 16) break;
    }

    color = vec4(wdist * wdist, trap.yzw);
    return 0.5 * log(wdist) * wdist / derivative;
}

float calcShadow(in vec3 origin, in vec3 ray, in float k) {
    vec4 tmp;
    float result = 1.0;
    float t = 0.0;

    for(int i = 0; i < MAX_MARCHING_STEPS / 2; i++) {
        float rStep = mandelbulbSDF(origin + ray * t, tmp);
        result = min(result, k * rStep / t);
        if(result < EPSILON) break;
        t += clamp(rStep, 0.01, 0.2);
    }

    return clamp(result, 0.0, 1.0);
}

vec3 calcNormal(in vec3 point) {
    vec4 tmp;
    vec2 e = vec2(EPSILON, -EPSILON);
    // Houbbard-Douady
    return normalize(e.xyy * mandelbulbSDF(point + e.xyy, tmp) + e.yyx * mandelbulbSDF(point + e.yyx, tmp) + 
					 e.yxy * mandelbulbSDF(point + e.yxy, tmp) + e.xxx * mandelbulbSDF(point + e.xxx, tmp));
}

float raymarch(in vec3 ray, out vec4 color) {
	float t = MIN_DIST;

	for(int i = 0; i < MAX_MARCHING_STEPS; i++) { 
		float rstep = mandelbulbSDF(cameraPosition + ray * t, color);
        if(rstep < EPSILON)
            return t;
        t += rstep;
		if(t >= MAX_DIST)
            break;
    }
    return MAX_DIST + 1;
}

vec3 render(in vec2 coord) {
    // Intersect mandelbulb
    vec3 ray = normalize(tlCoord + coord.x / 300.0 * xStep + coord.y / 300.0 * yStep);
    vec4 trap;
    float t = raymarch(ray, trap);
    vec3 color;

    // Coloring
    if(t >= MAX_DIST) {
        // Sky
     	color = vec3(0.0625, 0.09375, 0.125) + 0.0625 * ray.y;
	} else {
        // Mandelbulb
        vec3 intersection = cameraPosition + t * ray;
        vec3 normal = calcNormal(intersection);
        vec3 lightRay = normalize(LIGHT1 - ray); // for specular (only first light)
        float occlusion = clamp(0.05 * log(trap.x), 0.0, 1.0);

        // Color
        color = vec3(0.01);
		color = mix(color, vec3(0.3, 0.2, 0.1), clamp(trap.y, 0.0, 1.0));
	 	color = mix(color, vec3(0.1, 0.1, 0.1), clamp(trap.z * trap.z, 0.0, 1.0));
        color = mix(color, vec3(0.4, 0.5, 0.8), clamp(pow(trap.w, 6), 0.0, 1.0));
        color *= 0.5;

        // Light
        float kAmbient = (0.1 + 0.9 * occlusion);
        float kShadow1 = calcShadow(intersection + EPSILON * normal, LIGHT1, 32.0);
        float kDiffuse1 = clamp(dot(LIGHT1, normal), 0.0, 1.0) * kShadow1;
        float kShadow2 = calcShadow(intersection + EPSILON * normal, LIGHT2, 32.0);
        float kDiffuse2 = clamp(0.5 * dot(LIGHT2, normal), 0.0, 1.0) * kShadow2;
        float kSpecular = pow(clamp(dot(normal, lightRay), 0.0, 1.0), 32.0);
        kSpecular *= kDiffuse1 * (0.04 + 0.96 * pow(clamp(1.0 - dot(lightRay, LIGHT1), 0.0, 1.0), 5.0));

        vec3 lighting = vec3(0.0, 0.0, 0.0);
        lighting += kAmbient * vec3(0.875, 0.75, 0.625);
        lighting += kDiffuse1 * vec3(10.5, 7.7, 4.9);
        lighting += kDiffuse2 * vec3(5.25, 3.85, 2.45);
        color *= lighting;
        color += kSpecular * 15.0;
    }

    // Gamma correction
    return pow(color, vec3(0.59375));
}

void main() {
#if ANTI_ALIASING < 2
    color = render(pixelPosition);
#else
    color = vec3(0.0);
    for(int j = 0; j < ANTI_ALIASING; j++) {
        for(int i = 0; i < ANTI_ALIASING; i++) color += render(pixelPosition + (vec2(i,j) / float(ANTI_ALIASING)));
    }
	color /= float(ANTI_ALIASING * ANTI_ALIASING);
#endif
}