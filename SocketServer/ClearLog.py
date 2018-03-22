import os
import shutil
fils = os.listdir("bin/Debug/log/")
for f in fils:
    os.system("rm %s" % ("bin/Debug/log/"+f))

