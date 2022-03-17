#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>
int F(int n,int b) {
int foo;
int __temp_0 = n + b;
foo = __temp_0;
return foo;
}
void G() {
int x;
int y;
int z;
int __temp_1 = 6*7;
z = __temp_1;
label_2:;
bool __temp_4 = z>0;
if (__temp_4 == 0) goto label_3;
printf("%d\n", z);
int __temp_5 = z - 1;
z = __temp_5;
goto label_2;
label_3:;
return;
}
int main() {
char* s;
s = "atte";
char* h = " haarni";
size_t __temp_7 = strlen(s) + strlen(h);
__temp_7 = __temp_7 + 1;
char* __temp_6 = malloc(__temp_7);
strcat(__temp_6,s);
strcat(__temp_6,h);
char* newstring = __temp_6;
printf("%s\n", newstring);
size_t __temp_9 = strlen(newstring) + strlen(" 112");
__temp_9 = __temp_9 + 1;
char* __temp_8 = malloc(__temp_9);
strcat(__temp_8,newstring);
strcat(__temp_8," 112");
printf("%s\n", __temp_8);
}
