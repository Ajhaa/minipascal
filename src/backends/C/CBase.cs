namespace C
{
    class CBase
    {
        public static string Base = @"
#include <assert.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>

typedef struct {
    size_t size;
    int* content;
} integerArray;

typedef struct {
    size_t size;
    char** content;
} stringArray;

typedef struct {
    size_t size;
    bool* content;
} BooleanArray;

";

    }
}
