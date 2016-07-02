#coding:utf8
import os
import mysql.connector
import json

#每个玩家事件发生的频率

def main(config):
	output = []
	didToNum = {}
	db = mysql.connector.connect(**config)
	cursor = db.cursor()
	sql = 'select * from tbllog_event'
	cursor.execute(sql)
	for r in cursor.fetchall():
		output.append(r)
		#print r
		if r[3] == 100:
			if didToNum.get(r[5]) == None:
				didToNum[r[5]] = {'ip':r[4], 'num':0}
			didToNum[r[5]]['num'] += 1

	for d in didToNum:
		print d, didToNum[d]

	js = json.dumps(didToNum)
	of = open("res.js", "w")
	of.write(js)
	of.close()



config = {
	'host' : 'localhost',
	'user' : 'zczbdev',
	'password' : 'zc6Tzb43#%',
	'database' : 'zczb_log_android_cn_s999',
	'charset' : 'utf8',
	'use_unicode': True,
    'get_warnings': True,
}

main(config)