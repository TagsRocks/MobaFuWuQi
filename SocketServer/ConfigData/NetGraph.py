#coding:utf8
import urllib2
import urllib
import time
import json
import matplotlib.pyplot as plt

#服务器报文数量采样分析

sampleNum = 20
sampleInterval = 0.5

startTime = time.time()

actors = {}
def sample():
	now = time.time()-startTime
	#linux 服务器测试
	url = 'http://127.0.0.1:12002/query?flags=agent'
	#url = 'http://172.16.10.65:12002/query?flags=agent'

	con = urllib2.urlopen(url).read()
	data = json.loads(con)
	print data
	#监控每个Actor的流量 显示出一条时间 总流量曲线
	agent = data["AgentStatus"]["Agents"]
	for a in agent:
		a = a["Agent"]
		aid = a["id"]
		if a["Active"]:
			if not actors.get(aid):
				actors[aid] = []

			actors[aid].append({
				"id":aid,
				"time": now,
				"ReceivePacketsSize": a["ReceivePacketsSize"],
				"ReceivePackets" : a["ReceivePackets"],
				"SendPacketsSize" : a["SendPacketsSize"],
				"SendPackets" : a["SendPackets"],
			})

def calculate():
	for a in actors:
		data = actors[a]
		times = []
		values = []
		values2 = []
		values3 = []
		values4 = []
		values5 = []
		values6 = []
		lastSend = 0
		lastRecv = 0
		lastSendNum = 0
		lastRecvNum = 0
		i = 0
		lastTime = 0
		for t in data:
			if i > 0:
				times.append(t["time"])
				diffTime = t["time"]-lastTime

				sendBytes = t["SendPacketsSize"]-lastSend
				recvBytes = t["ReceivePacketsSize"]-lastRecv
				sendNum = t["SendPackets"]-lastSendNum
				recvNum = t["ReceivePackets"]-lastRecvNum
				if sendNum == 0:
					sendNum = 1
				if recvNum == 0:
					recvNum = 1
				values.append(sendBytes/diffTime)
				values2.append(recvBytes/diffTime)
				values3.append(sendBytes*1.0/sendNum)
				values4.append(recvBytes*1.0/recvNum)
				values5.append(sendNum/diffTime)
				values6.append(recvNum/diffTime)
			lastSend = t["SendPacketsSize"]
			lastRecv = t["ReceivePacketsSize"]
			lastSendNum = t["SendPackets"]
			lastRecvNum = t["ReceivePackets"]
			lastTime = t["time"]
			i += 1

		nf = open("net.log", "a")
		nf.write("==================================")
		nf.write(str(time.time())+"\n")
		nf.write(str(times)+"\n")
		nf.write("\nSendBytes/s\n")
		nf.write(str(values)+"\n")
		nf.write("\nRecvBytes/s\n")
		nf.write(str(values2)+"\n\n")

		nf.write("\nSendPackageSize\n")
		nf.write(str(values3)+"\n")
		nf.write("\nRecvPackageSize\n")
		nf.write(str(values4)+"\n\n")

		nf.write("\nSendNum/s\n")
		nf.write(str(values5)+"\n")
		nf.write("\nRecvNum/s\n")
		nf.write(str(values6)+"\n\n")
		nf.close()
		break


def draw():
	for a in actors:
		data = actors[a]
		times = []
		values = []
		values2 = []
		values3 = []
		values4 = []
		values5 = []
		values6 = []
		lastSend = 0
		lastRecv = 0
		lastSendNum = 0
		lastRecvNum = 0
		i = 0
		lastTime = 0
		for t in data:
			if i > 0:
				times.append(t["time"])
				diffTime = t["time"]-lastTime

				sendBytes = t["SendPacketsSize"]-lastSend
				recvBytes = t["ReceivePacketsSize"]-lastRecv
				sendNum = t["SendPackets"]-lastSendNum
				recvNum = t["ReceivePackets"]-lastRecvNum
				if sendNum == 0:
					sendNum = 1
				values.append(sendBytes/diffTime)
				values2.append(recvBytes/diffTime)
				values3.append(sendBytes*1.0/sendNum)
				values4.append(recvBytes*1.0/recvNum)
				values5.append(sendNum/diffTime)
				values6.append(recvNum/diffTime)
			lastSend = t["SendPacketsSize"]
			lastRecv = t["ReceivePacketsSize"]
			lastSendNum = t["SendPackets"]
			lastRecvNum = t["ReceivePackets"]
			lastTime = t["time"]
			i += 1

		#每秒发送字节数量
		plt.figure(1)
		plt.subplot(211)
		plt.plot(times, values)
		plt.plot(times, values2, "r-")
		
		#平均每个报文大小
		#plt.subplot(212)
		plt.figure(2)
		plt.plot(times, values3)
		plt.plot(times, values4, "r-")		

		#每秒报文数量
		plt.figure(3)
		plt.plot(times, values5)
		plt.plot(times, values6, "r-")

		break

	plt.show()

def main():
	while True:
		i = 0
		actors = {}
		startTime = time.time()
		while i < sampleNum:
			sample()
			time.sleep(sampleInterval)
			i += 1
		calculate()
		time.sleep(60)
	#draw()
	
main()	