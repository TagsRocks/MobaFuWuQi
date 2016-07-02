#coding:utf8
import os
import time

def main():
	while True:	
		try:
			cmd = 'ps aux | grep "mono --debug" | grep -v grep | awk \'{print $2}\' > pid.txt'
			os.system(cmd)
			con = open('pid.txt').read().replace('\n', '')
			

			cmd = "top -p %s -b -n5 | awk '{print $9,$10}' > cpu.txt" % (con)
			os.system(cmd)

			ls = open("cpu.txt").readlines()
			rowNum = -1
			if len(ls[-1]) < 3:
				rowNum = -2
			print ls[rowNum]
			
			cm= ls[rowNum].replace('\n', '').split(' ')
			print cm
			cpu = cm[0]
			mem = cm[1]
			print cpu, mem

			with open('status.txt', 'a') as f:
				cm.append((int)(time.time()))
				f.write(str(cm)+"\n")
		except:
			pass

		time.sleep(5)

main()
