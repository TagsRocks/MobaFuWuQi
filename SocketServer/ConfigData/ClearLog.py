import os

files = os.listdir('log')
for f in files:
    os.remove("log/"+f)


