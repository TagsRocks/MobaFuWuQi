#coding:utf8
import urllib2
import urllib
import time
import json

countData = []

def sample():
	now = (int)(time.time())
	#linux 服务器测试
	url = 'http://127.0.0.1:12002/query?flags=agent'
	#url = 'http://172.16.10.65:12002/query?flags=agent'

	con = urllib2.urlopen(url).read()
	data = json.loads(con)
	#print data
	#监控每个Actor的流量 显示出一条时间 总流量曲线
	agentCount = data["AgentStatus"]["AgentCount"]
	cd = {
		"time":now,
		"count":agentCount
	}


	f = open("count.log", "a")
	f.write(str(cd)+"\n\n")
	f.close()

def main():
	while True:
		try:
			sample()
		except:
			pass
		time.sleep(20)

main()




