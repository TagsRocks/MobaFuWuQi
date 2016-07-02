#coding:utf8
import os
def encode(key, string):
	print "fs", len(string), len(key)

	encodedres = []
	for i in xrange(len(string)):
		key_c = key[i%len(key)]
		encoded_c = chr((ord(string[i])+ord(key_c))%256)
		encodedres.append(encoded_c)
		if i == 713:
			print "err", ord(key_c), ord(encoded_c)

	encoded_string = "".join(encodedres)
	print "res", ord(encoded_string[713])
	print "os", len(encoded_string)
	return encoded_string

def decode(key, string):
	encodedres = []
	for i in xrange(len(string)):
		key_c = key[i%len(key)]
		encoded_c = chr((ord(string[i])-ord(key_c))%256)
		encodedres.append(encoded_c)

	encoded_string = "".join(encodedres)
	return encoded_string

def compareByte(a, b, c):
	for i in xrange(len(a)):
		if a[i] != b[i]:
			print ord("1")
			print i, ord(a[i]), ord(b[i]), ord(c[i])
			break

def main():
	of = open("hotUpdateTest.zip", 'rb').read()
	res = encode("11", of)
	of = open("hotUpdate.txt", 'wb')
	of.write(res)
	of.close()

	'''
	inf = open('res.txt', 'rb').read()
	res2 = decode("11", inf)
	of2 = open("res2.zip", 'wb')
	of2.write(res2)
	of2.close()
	'''


	'''
	of1 = open("mysql-connector-python-2.0.4.zip", 'rb').read()
	of2 = open("res2.zip", 'rb').read()
	of3 = open('res.txt', 'rb').read()
	compareByte(of1, of2, of3)
	'''


main()