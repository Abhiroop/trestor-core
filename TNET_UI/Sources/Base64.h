
#ifndef BASE_64_H
#define BASE_64_H

class Base_64
{

public:
	Base_64();

	static char *encode(const unsigned char *data,
		size_t input_length,
		size_t *output_length);

	static unsigned char *decode(const char *data,
		size_t input_length,
		size_t *output_length);

};


#endif