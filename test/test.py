import subprocess
import os


def run_and_compare(file):
    r1 = subprocess.run(['./bin/Debug/netcoreapp2.2/linux-x64/publish/minipascal', './test/minipascal/' + file])
    r2 = subprocess.run(["gcc", "pascal.c"])
    r3 = subprocess.run(['./a.out'], stdout=subprocess.PIPE, encoding="UTF8", stderr=subprocess.PIPE)


    # split = str(result.stdout).split('\n')
    try:
        r1.check_returncode()
        r2.check_returncode()
        r3.check_returncode()
        print("%s test success" % file)
        return True
    except:
        print('%s test failed' % file)
        return False


tests = os.listdir("./test/minipascal")

subprocess.run(["dotnet", "publish", "--self-contained", "--runtime", "linux-x64"])

successes = 0
for test in tests:
    if run_and_compare(test):
        successes += 1

print("\n%d / %d tests succeeded" % (successes, len(tests)))
