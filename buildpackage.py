import subprocess
import sys
import os
import shutil

os.makedirs('Bin')
print subprocess.check_output([sys.argv[1],'-projectPath',os.getcwd(), '-logFile','Bin/Build.log', '-batchmode', '-executeMethod', 'PackageDesigner.CommandLineExportAllPackage', os.getcwd()+'/Bin'])
