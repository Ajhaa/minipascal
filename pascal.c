#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>
int main() {
int* a = malloc(sizeof(int) * 5);
int i = 0;
label_0:;
bool __temp_2 = i<5;
if (__temp_2 == 0) goto label_1;
int __temp_3 = i*i;
a[i] = __temp_3;
int __temp_4 = i + 1;
i = __temp_4;
goto label_0;
label_1:;
printf("%d\n", a[3]);
}
