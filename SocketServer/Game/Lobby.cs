using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SimpleJSON;

namespace MyLib
{
	public class Lobby : Actor
	{
		private List<RoomActor> rooms = new List<RoomActor> ();

		public Lobby ()
		{
		}

	    public async Task<RoomActor[]> GetRooms()
	    {
	        await this._messageQueue;
	        return rooms.ToArray();
	    }

	    private async Task<RoomActor> CreateRoom (int maxPlayerNum, bool isNewUser, RoomInfo roomInfo)
		{
			await this._messageQueue;
			var r = new RoomActor (maxPlayerNum, isNewUser, roomInfo);
			ActorManager.Instance.AddActor (r, false);
			rooms.Add (r);
			return r;
		}

		public async Task DeleteRoom (int roomId)
		{
			await this._messageQueue;
			foreach (var r in rooms) {
				if (r.Id == roomId) {
					rooms.Remove (r);
					r.RemoveRoom ();
					break;
				}
			}
		}

	    public async Task<RoomActor> GetRoom(int id, int maxPlayerNum, PlayerActor player)
	    {
	        await this._messageQueue;
	        foreach (var roomActor in rooms)
	        {
	            if (roomActor.Id == id)
	            {
	                var suc = await roomActor.ReAddPlayer(player, maxPlayerNum);
	                if (suc)
	                {
	                    return roomActor;
	                }
	                else
	                {
	                    return null;
	                }
	            }
	        }
	        return null;
	    } 

		public async Task<RoomActor> FindRoom (PlayerActor player, int maxPlayerNum, RoomInfo roomInfo, bool isNew = false)
		{
			await this._messageQueue;
			var arr = rooms.ToArray ();
			//循环中await会导致循环被挂起，这样其它函数有可能操作Lobby 为了避免数据不一致需要将room拷贝
			//但是对Room可能后面并行处理导致问题，所以对Room最好能够一次性事务操作
			RoomActor rm = null;
			foreach (var r in arr) {				
			    var leftTime = await r.GetLeftTime();
			    if (leftTime > GameConst.LeftNotEnterTime)
			    {
					
                    var suc = await r.AddPlayerNew(player, maxPlayerNum, isNew);
					LogHelper.Log("Lobby", "AddPlayer " + leftTime+" isNew "+isNew+" suc "+suc);
                    if (suc)
                    {
                        rm = r;
                        break;
                    }
                }            
			}
			if (rm != null) {
				return rm;
			}

			var newRoom = await CreateRoom (maxPlayerNum, isNew, roomInfo);
			var suc2 = await newRoom.AddPlayerNew (player, maxPlayerNum, isNew);
			return newRoom;
		}

		public override void Init ()
		{
			//base.Init ();
		}

	    public override string ToString()
	    {
	        var sj = new SimpleJSON.JSONClass();

	        var jsonObj = new JSONClass();

	        var jsonArray = new JSONArray();
	        jsonObj.Add("RoomCount", new JSONData(rooms.Count));

	        foreach (var room in rooms)
	        {
	            jsonArray.Add("Room", room.GetJsonStatus().Result);
	        }
	        jsonObj.Add("Rooms", jsonArray);
	        sj.Add("LobbyStatus", jsonObj);
	        return sj.ToString();
	    }

	    public async Task<int> GetRoomNum()
	    {
	        await this._messageQueue;
	        return rooms.Count;
	    }

	    public async Task BroadcastNews(string con)
	    {
	        await this._messageQueue;
	        var rarr = rooms.ToArray();
	        foreach (var roomActor in rarr)
	        {
	            roomActor.BroadcastNews(con);
	        }
	    }
	}
}

