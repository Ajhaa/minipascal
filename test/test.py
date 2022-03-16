import os

exit_code = os.system("dotnet run simple.mpsc && node node_glue")
print(exit_code)
assert(exit_code == 0)
