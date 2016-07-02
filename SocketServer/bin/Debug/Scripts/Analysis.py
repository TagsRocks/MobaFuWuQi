#coding:utf8
import os
import json
js = json.loads(open("res.js", 'r').read())

numToPlayer = {}

for j in js:
	d = js[j]
	if numToPlayer.get(d["num"]) == None:
		numToPlayer[d["num"]] = 0

	numToPlayer[d["num"]] += 1


arr = []
for n in numToPlayer:
	arr.append([n, numToPlayer[n]])

def func(a):
	return a[0]

arr.sort(key=func)

for k in arr:
	print k

print "totalPlayers", len(js)

