#coding:utf8
'''
分析日志文件
'''
import os

def handleFile(f):
	lines = open("log/"+f).readlines()
	for l in lines:
		if l.find("Excep") != -1:
			print f
			print l


def main():
	flist = os.listdir('log')
	for f in flist:
		print f
		handleFile(f)


main()
