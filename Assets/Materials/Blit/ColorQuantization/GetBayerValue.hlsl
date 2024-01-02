#ifndef CUSTOMBAYERINCLUDED
#define CUSTOMBAYERINCLUDED

static const int bayer4[4 * 4] = {
	0, 8, 2, 10,
	12, 4, 14, 6,
	3, 11, 1, 9,
	15, 8, 13, 5
};

static int bayer8[8 * 8] = {
	0, 32, 8, 40, 2, 34, 10, 42, 
	48, 16, 56, 24, 50, 18, 58, 26, 
	12, 44, 4, 36, 14, 46, 6, 38, 
	60, 28, 52, 20, 62, 30, 54, 22,
	3, 35, 11, 43, 1, 33, 9, 41, 
	51, 19, 59, 27, 49, 17, 57, 25,
	15, 47, 7, 39, 13, 45, 5, 37,
	63, 31, 55, 23, 61, 29, 53, 21
};


void Bayer4_float(float2 pixelCoord, float spread, out float value)
{
	value = bayer4[pixelCoord.x % 4, pixelCoord.y % 4] +1.;
	value /= 4. * 4.;
	value -= .5;
	value *= spread;
}

void Bayer8_float(float2 pixelCoord, float spread, out float value)
{
	value = bayer8[pixelCoord.x % 8, pixelCoord.y % 8] +1.;
	value /= 8. * 8.;
	value -= .5;
	value *= spread;
}

#endif