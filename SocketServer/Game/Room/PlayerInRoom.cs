using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    /// <summary>
    /// 接管从网络发送过来的数据到 玩家在Room中的Proxy中
    /// 和EntityActor进行交互
    /// </summary>
    public class PlayerInRoom : ActorInRoom
    {
        private AvatarInfo lastAvatarInfo;
        private AvatarInfo avatarInfo;
        private PlayerActorProxy proxy;
        private AINPC ai;
        private GridManager gridManager;

        public PlayerInRoom(PlayerActor pl, AvatarInfo info)
        {
            lastAvatarInfo = AvatarInfo.CreateBuilder(info).Build();
            avatarInfo = AvatarInfo.CreateBuilder(info).Build();
            avatarInfo.Level = 1;

            proxy = new PlayerActorProxy(pl, this);
            //玩家在房间中的对象通过Room访问
            Id = pl.Id;
            avatarInfo.State = PlayerState.NotInRoom;
        }
        /// <summary>
        /// 初始化完地图中坐标
        /// </summary>
        public void AfterInitPos()
        {
            avatarInfo.State = PlayerState.WaitChoose;
        }
        /// <summary>
        /// 初始化完设置所在房间
        /// </summary>
        public override void InitAfterSetRoom()
        {
            ai = AddComponent<PlayerAI>();
            avatarInfo.TeamColor = GetRoom().GetTeamColor();
            //用于后面设置位置使用
            gridManager = GetRoom().GetComponent<GridManager>();
        }
        //建立PlayerActor和PlayerInRoom之间通信的管道
        //需要同步进行初始化
        public async Task InitProxy()
        {
            await proxy.InitProxy();
        }

        public override void RunAI()
        {
            ai.RunAI();
        }

        public override void HandleCmd(ActorMsg msg)
        {
            //LogHelper.Log("PlayerInRoom:HandleCmd", ""+msg?.msg+":"+msg.packet?.protoBody+":frame:"+GetRoom().GetFrameId()+":roomTie:"+GetRoom().GetRoomTimeNow());
            if (!string.IsNullOrEmpty(msg.msg))
            {
                var cmds = msg.msg.Split(' ');
                if (cmds[0] == "close")
                {
                    ConnectClose();
                }
            }
            else
            {
                LogHelper.Log("PlayerActor", "ReceivePacket " + Id + " p " + msg.packet.protoBody.ToString());
                if (msg.packet.protoBody.GetType() == typeof(CGPlayerCmd))
                {
                    var cmd = msg.packet.protoBody as CGPlayerCmd;
                    var cmds = cmd.Cmd.Split(' ');
                    var cmd0 = cmds[0];
                    //服务器端判定伤害
                    switch (cmd0)
                    {
                        case "UpdateData":
                            UpdateData(cmd);
                            break;
                        case "Damage":
                            Damage(cmd);
                            break;
                        case "Skill":
                            Skill(cmd);
                            break;
                        case "Ready":
                            Ready(cmd);
                            break;
                        case "InitData":
                            InitData(cmd, msg);
                            break;
                        case "HeartBeat":
                            HeartBeat(cmd, msg);
                            break;
                        case "TestUDP":
                            TestUDP(cmd);
                            break;
                        case "UDPConnect":
                            UDPConnect(cmd);
                            break;
                        case "UDPLost":
                            UDPLost(cmd);
                            break;
                        case "TestKCP":
                            TestKCP(cmd);
                            break;
                        case "KCPConnect":
                            KCPConnect(cmd);
                            break;
                        case "KCPLost":
                            KCPLost(cmd);
                            break;
                        case "ChooseHero":
                            ChooseHero(cmd);
                            break;
                    }
                }
            }
        }




        /// <summary>
        /// 在房间内时断线处理
        /// </summary>
        private void ConnectClose()
        {
            LogHelper.Log("PlayerActor", "CloseActor " + Id);
            GetRoom().RemovePlayer(this, AvatarInfo.CreateBuilder(avatarInfo).Build());
            proxy.ConnectClose();
        }

        #region Score
        private static List<int[]> scoreList = new List<int[]>()
        {
            new []{10, 10},
            new []{20, 15},
            new []{30, 20},
            new []{40, 25},
            new []{50, 30},
            new []{60, 35},
            new []{70, 40},
            new []{20, 40},
        };

        public void AddScore(int eneContinue, int eneId)
        {
            var myCount = this.avatarInfo.ContinueKilled;
            //我的连杀得分
            if (myCount < scoreList.Count)
            {
                var s = scoreList[myCount][0];
                this.avatarInfo.Score += s;
            }
            else
            {
                var s = scoreList[scoreList.Count - 1][0];
                this.avatarInfo.Score += s;
            }

            //击杀连杀玩家补充得分
            if (eneContinue < scoreList.Count)
            {
                var s = scoreList[eneContinue][1];
                this.avatarInfo.Score += s;
            }
            else
            {
                var s = scoreList[scoreList.Count - 1][1];
                this.avatarInfo.Score += s;
            }
            avatarInfo.ContinueKilled++;

            var dmg = GCPlayerCmd.CreateBuilder();
            var dinfo = DamageInfo.CreateBuilder();
            var ainfo = AvatarInfo.CreateBuilder();
            ainfo.ContinueKilled = avatarInfo.ContinueKilled;
            dinfo.Attacker = this.Id;
            dinfo.Enemy = eneId;
            dmg.Result = "Dead";
            dmg.DamageInfo = dinfo.Build();
            dmg.AvatarInfo = ainfo.Build();
            GetRoom().AddCmd(dmg);
        }
        #endregion



        /// <summary>
        /// 1:FrameID ==0  128 TCP稳定发过来
        /// 2：服务器lowChange 事件同步给客户端
        /// 3：客户端FrameID 更新
        /// Room 内玩家数据的同步
        /// </summary>
        /// <param name="cmd"></param>
	    private void UpdateData(CGPlayerCmd cmd)
        {
            if (cmd.AvatarInfo.HasIsRobot)
            {
                avatarInfo.IsRobot = cmd.AvatarInfo.IsRobot;
            }

            //玩家服务器状态不直接受客户端影响
            //同步速度
            /*
            if (cmd.AvatarInfo.HasSpeedX)
            {
                avatarInfo.SpeedX = cmd.AvatarInfo.SpeedX;
                avatarInfo.SpeedY = cmd.AvatarInfo.SpeedY;
            }
            */
            //if (cmd.AvatarInfo.HasX)
            {
                SetClientSyncPos(cmd);
                //SetPos(new MyVec3( cmd.AvatarInfo.X, cmd.AvatarInfo.Y, cmd.AvatarInfo.Z));
            }

            /*
            if (cmd.AvatarInfo.HasDir)
            {
                avatarInfo.Dir = cmd.AvatarInfo.Dir;
            }
            if (cmd.AvatarInfo.HasHP)
            {
                avatarInfo.HP = cmd.AvatarInfo.HP;
            }
            */

            if (cmd.AvatarInfo.HasNetSpeed)
            {
                avatarInfo.NetSpeed = cmd.AvatarInfo.NetSpeed;
            }
            if (cmd.AvatarInfo.HasThrowSpeed)
            {
                avatarInfo.ThrowSpeed = cmd.AvatarInfo.ThrowSpeed;
            }
            if (cmd.AvatarInfo.HasJumpForwardSpeed)
            {
                avatarInfo.JumpForwardSpeed = cmd.AvatarInfo.JumpForwardSpeed;
            }
            if (cmd.AvatarInfo.HasName)
            {
                avatarInfo.Name = cmd.AvatarInfo.Name;
            }
            if (cmd.AvatarInfo.HasJob)
            {
                avatarInfo.Job = cmd.AvatarInfo.Job;
            }
            if (cmd.AvatarInfo.HasTowerDir)
            {
                avatarInfo.TowerDir = cmd.AvatarInfo.TowerDir;
            }

            if (cmd.AvatarInfo.HasPlayerModelInGame)
            {
                avatarInfo.PlayerModelInGame = cmd.AvatarInfo.PlayerModelInGame;
            }
        }

        private List<AvatarInfo> positions = new List<AvatarInfo>();
        

        public MyVec3 GetClientVelocity()
        {
            if(positions.Count > 0)
            {
                var p1 = positions[positions.Count-1];
                var speed = new MyVec3(p1.SpeedX, 0, p1.SpeedY);
                return speed;
            }
            return MyVec3.zero;
        }
        /// <summary>
        /// 获得预测的客户端当前移动位置
        /// 根据玩家实际距离计算修正值速度
        /// 这一frame 结束时刻玩家的位置
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPredictClientPos()
        {

            if (positions.Count > 0) {
                var curPos = Util.NetPosToFloat(avatarInfo);
                var p1 = positions[positions.Count-1];
                if(p1.SpeedX == 0 && p1.SpeedY == 0)
                {
                    return curPos;
                }
                var speed = new MyVec3(p1.SpeedX, 0, p1.SpeedY).ToFloat();
                return curPos + Util.PredictTimeStep* speed;

                /*
                var p1 = positions[positions.Count-1];
                if(p1.SpeedX ==0 && p1.SpeedY == 0)
                {
                    //return Util.NetPosToFloat(p1);

                }

                //var p1Pos = Util.NetPosToFloat(p1);
                var speed = new MyVec3(p1.SpeedX, 0, p1.SpeedY).ToFloat();
                var ft = GetRoom().GetRoomTimeNow();

                var f1 = Util.ClientFrameToServer(p1.FrameID);
                var p1Time = Util.FrameToTime(f1);
                var extraTime = ft - p1Time;

                if (extraTime > 0)
                {
                    //预测这帧结束时候的位置 移动速度修正
                    return p1Pos + speed * (extraTime+Util.FrameSecTime);
                }else
                {
                    return p1Pos + speed * Util.FrameSecTime;
                }

                var p0 = positions[positions.Count - 2];
                if (p1.FrameID > p0.FrameID)
                {
                    var ft = GetRoom().GetRoomTimeNow();

                    var f1 = Util.ClientFrameToServer(p1.FrameID);
                    var f0 = Util.ClientFrameToServer(p0.FrameID);
                    var dt = Util.FrameToTime(f1 - f0);
                    var dp = Util.DeltaPos(p1, p0);
                    var speed = dp / dt;
                    speed.Y = 0;

                    var p1Pos = Util.NetPosToFloat(p1);
                    //var p1Time = Util.FrameToTime(p1.FrameID);
                    var p1Time = Util.FrameToTime(f1);

                    //服务器外插值玩家的位置 服务器当前时间-
                    var extraTime = ft - p1Time;
                    if (extraTime > 0)
                    {
                        return p1Pos + speed * extraTime;
                    }else
                    {
                        return p1Pos;
                    }
                }
                else
                {
                    return Util.NetPosToFloat(p1);
                }
                */
            }
            /*else if(positions.Count > 0)
            {
                return Util.NetPosToFloat(positions[positions.Count-1]);
            }*/
            else
            {
                return Util.NetPosToFloat(avatarInfo);
            }
        }

        /// <summary>
        /// 服务器类似于客户端需要存储多个客户端同步位置
        /// 服务器玩家的移动需要由客户端或者自己驱动
        /// SpeedX SpeedY
        /// Dir 属性
        /// 
        /// 客户端报文都带有时间戳
        /// 服务器报文也都带有时间戳
        /// </summary>
        /// <param name="cmd"></param>
        public void SetClientSyncPos(CGPlayerCmd cmd)
        {
            if (avatarInfo.ResetPos)
            {
                return;
            }
            //玩家位置已经同步下去了才可以开始 接受玩家的移动命令
            if(avatarInfo.State != PlayerState.AfterReset)
            {
                return;
            }

            var info = cmd.AvatarInfo;
            /*
            if (info.HasX)
            {

            }
            */
            /*
            if (info.HasSpeedX)
            {
                info.FrameID = cmd.FrameId;
                avatarInfo.SpeedX = info.SpeedX;
                avatarInfo.SpeedY = info.SpeedY;
                positions.Add(info);
                if (positions.Count > 5)
                {
                    positions.RemoveAt(0);
                }
            }
            else
            {
                if (positions.Count > 0)
                {
                    var last = positions[positions.Count - 1];
                    var fakeInfo = AvatarInfo.CreateBuilder();
                    fakeInfo.SpeedX = last.SpeedX;
                    fakeInfo.SpeedY = last.SpeedY;
                    fakeInfo.FrameID = cmd.FrameId;

                }
            }
            */
            //服务器计算客户端位置 客户端只发送操控命令
            /*
            if (info.HasX)
            {
                SetPos(Util.NetPosToIntVec(info));
                return;
            }
            */

            //将客户端输入统计
            if (info.HasX)
            {
                info.FrameID = cmd.FrameId;
                positions.Add(info);
                if (positions.Count > 5)
                {
                    positions.RemoveAt(0);
                }
            }else
            {
                /*
                //重复上次的命令
                if (positions.Count > 0)
                {
                    var lastInfo = positions[positions.Count - 1];
                    var fakeInfo = AvatarInfo.CreateBuilder();
                    fakeInfo.X = lastInfo.X;
                    fakeInfo.Y = lastInfo.Y;
                    fakeInfo.Z = lastInfo.Z;
                    fakeInfo.SpeedX = lastInfo.SpeedX;
                    fakeInfo.SpeedY = lastInfo.SpeedY;
                    fakeInfo.FrameID = cmd.FrameId;

                    positions.Add(fakeInfo.Build());
                    if (positions.Count > 5)
                    {
                        positions.RemoveAt(0);
                    }
                }
                */
            }

        }


        private void Damage(CGPlayerCmd cmd)
        {
            var gc = GCPlayerCmd.CreateBuilder();
            gc.DamageInfo = cmd.DamageInfo;
            gc.Result = cmd.Cmd;
            //world.AddCmd (gc);
            GetRoom().AddCmd(gc);

        }

        private void Skill(CGPlayerCmd cmd)
        {
            ai.aiCharacter.blackboard[AIParams.Command] = new AIEvent() { cmd=cmd };
            ai.aiCharacter.ChangeState(AIStateEnum.ATTACK);
        }


        private void Buff(CGPlayerCmd cmd)
        {
            var gc = GCPlayerCmd.CreateBuilder();
            cmd.BuffInfo.BuffId = ++buffId;
            gc.BuffInfo = cmd.BuffInfo;
            gc.Result = cmd.Cmd;
            GetRoom().AddCmd(gc);
            this.lastAvatarInfo.BuffInfoList.Add(cmd.BuffInfo);
        }

        /// <summary>
        /// 客户端通知服务器已经进入场景成功
        /// </summary>
        /// <param name="cmd"></param>
        private void Ready(CGPlayerCmd cmd)
        {
            GetRoom().SetReady(this);
            avatarInfo.State = PlayerState.AfterReset;
        }

        //服务器来初始化玩家的位置
        //需要初始化需要将模型从 0 0 0 位置挪到正确的游戏位置
        private void InitData(CGPlayerCmd cmd, ActorMsg msg)
        {
            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "InitData";
            proxy.SendPacket(gc, msg.packet.flowId, 0);
        }

        private void HeartBeat(CGPlayerCmd cmd, ActorMsg msg)
        {
            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "HeartBeat";
            proxy.SendPacket(gc, msg.packet.flowId, 0);
        }


        private void TestUDP(CGPlayerCmd cmd)
        {
            LogHelper.Log("UDP", "TestUDP");
            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "TestUDP";
            //proxy.ForceUDP(gc, 0, 0);
        }

        private void UDPConnect(CGPlayerCmd cmd)
        {
            LogHelper.Log("UDP", "UseUDP");
            //proxy.UseUDP();
        }

        private void UDPLost(CGPlayerCmd cmd)
        {
            LogHelper.Log("UDP", "UDPLost");
            //proxy.UDPLost();
        }

        private void TestKCP(CGPlayerCmd cmd)
        {
            LogHelper.Log("KCP", "TestKCP");
            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "TestKCP";
            //agent.ForceKCP(gc, 0, 0);
        }
        private void KCPConnect(CGPlayerCmd cmd)
        {
            LogHelper.Log("KCP", "UseKCP");
            //agent.UseKCP();
        }
        private void KCPLost(CGPlayerCmd cmd)
        {
            LogHelper.Log("KCP", "KCPLost");
            //agent.KCPLost();
        }

        private void ChooseHero(CGPlayerCmd cmd)
        {
            UpdateData(cmd);
            ai.AfterSelectHeroInit();
            GetRoom().ChooseHero(this);
            avatarInfo.State = PlayerState.AfterChoose;
        }
        public void AfterInGame()
        {
            ai.AfterInGame();
        }



        public AvatarInfo.Builder GetPosInfoDiff()
        {
            var na1 = AvatarInfo.CreateBuilder();
            na1.Id = avatarInfo.Id;

            if (avatarInfo.X != lastAvatarInfo.X
                || avatarInfo.Y != lastAvatarInfo.Y
                || avatarInfo.Z != lastAvatarInfo.Z
                || avatarInfo.Dir != lastAvatarInfo.Dir
                || avatarInfo.SpeedX != lastAvatarInfo.SpeedX
                || avatarInfo.SpeedY != lastAvatarInfo.SpeedY)
            {
                na1.X = avatarInfo.X;
                na1.Y = avatarInfo.Y;
                na1.Z = avatarInfo.Z;
                na1.Dir = avatarInfo.Dir;
                na1.SpeedX = avatarInfo.SpeedX;
                na1.SpeedY = avatarInfo.SpeedY;
                na1.Changed = true;

                lastAvatarInfo.X = avatarInfo.X;
                lastAvatarInfo.Y = avatarInfo.Y;
                lastAvatarInfo.Z = avatarInfo.Z;
                lastAvatarInfo.Dir = avatarInfo.Dir;
                lastAvatarInfo.SpeedX = avatarInfo.SpeedX;
                lastAvatarInfo.SpeedY = avatarInfo.SpeedY;
            }

            if(avatarInfo.ResetPos)
            {
                na1.ResetPos = true;
                na1.Changed = true;
                avatarInfo.ResetPos = false;
            }


            if (avatarInfo.TowerDir != lastAvatarInfo.TowerDir)
            {
                na1.TowerDir = avatarInfo.TowerDir;
                na1.Changed = true;

                lastAvatarInfo.TowerDir = avatarInfo.TowerDir;
            }

            if (avatarInfo.FrameID != lastAvatarInfo.FrameID)
            {
                na1.FrameID = avatarInfo.FrameID;
                na1.Changed = true;
                lastAvatarInfo.FrameID = avatarInfo.FrameID;
            }
         

            /*
            //服务器端关闭UDP
            if (agent.useUDP)
            {
                if (agent.udpAgent != null)
                {
                    var now = Util.GetTimeNow();
                    var lr = agent.udpAgent.lastReceiveTime;
                    if (now - lr > 5)
                    {
                        agent.UDPLost();
                        var gc = GCPlayerCmd.CreateBuilder();
                        gc.Result = "UDPLost";
                        agent.SendPacket(gc, 0, 0);

                        LogHelper.Log("UDP", "UDPLost From Server");

                    }
                }
            }
            */

            return na1;
        }


        public AvatarInfo GetAvatarInfo()
        {
            //return lastAvatarInfo;
            return avatarInfo;
        }

        public void UpdateLevel(int rank)
        {
            if (level >= 40)
            {
                return;
            }

            ++dayBattleCount;

            var data = GameData.RoleUpgradeConfig[level - 1];
            var baseExp = data.baseExp;
            var extraExp = data.extraExp;

            rank = Math.Min(rank, GameData.LevelConfig.Count - 1);
            var rankRatio = GameData.LevelConfig[rank].rankRatio / 100f;

            dayBattleCount = Math.Min(GameData.LevelConfig.Count - 1, dayBattleCount);
            var dayRatio = GameData.LevelConfig[dayBattleCount].dayRatio / 100f;

            var getExp = baseExp * rankRatio + extraExp * dayRatio;
            Exp += (int)getExp;

            while (Exp >= data.exp && level <= 40)
            {
                level++;
                Exp -= data.exp;
                data = GameData.RoleUpgradeConfig[level - 1];
            }

            if (level >= 40)
            {
                Exp = 0;
            }

            //Login.SaveUserInfo(pid, uid, level, Exp, medal, dayBattleCount);
        }


        /// <summary>
        /// 得到玩家属性的diff
        /// public 的Async方法需要 和Actor自身同步 
        /// delta 数据压缩
        /// 	repeat的Delta数据压缩
        /// 	
        /// c++ 中通过宏定义
        /// 这里可以通过反射机制实现？
        /// </summary>
        public  AvatarInfo.Builder GetAvatarInfoDiff()
        {
            var na1 = AvatarInfo.CreateBuilder();
            na1.Id = avatarInfo.Id;

            if (avatarInfo.HP != lastAvatarInfo.HP)
            {
                na1.HP = avatarInfo.HP;
                na1.Changed = true;

                lastAvatarInfo.HP = avatarInfo.HP;
            }
            if (avatarInfo.TeamColor != lastAvatarInfo.TeamColor)
            {
                na1.TeamColor = avatarInfo.TeamColor;
                na1.Changed = true;

                lastAvatarInfo.TeamColor = avatarInfo.TeamColor;
            }
            if (avatarInfo.IsMaster != lastAvatarInfo.IsMaster)
            {
                na1.IsMaster = avatarInfo.IsMaster;
                na1.Changed = true;

                lastAvatarInfo.IsMaster = avatarInfo.IsMaster;
            }
            if (avatarInfo.NetSpeed != lastAvatarInfo.NetSpeed)
            {
                na1.NetSpeed = avatarInfo.NetSpeed;
                na1.Changed = true;

                lastAvatarInfo.NetSpeed = avatarInfo.NetSpeed;
            }

            if (avatarInfo.ThrowSpeed != lastAvatarInfo.ThrowSpeed)
            {
                na1.ThrowSpeed = avatarInfo.ThrowSpeed;
                na1.Changed = true;

                lastAvatarInfo.ThrowSpeed = avatarInfo.ThrowSpeed;
            }

            if (avatarInfo.JumpForwardSpeed != lastAvatarInfo.JumpForwardSpeed)
            {
                na1.JumpForwardSpeed = avatarInfo.JumpForwardSpeed;
                na1.Changed = true;

                lastAvatarInfo.JumpForwardSpeed = avatarInfo.JumpForwardSpeed;
            }
            if (avatarInfo.Name != lastAvatarInfo.Name)
            {
                na1.Name = avatarInfo.Name;
                na1.Changed = true;

                lastAvatarInfo.Name = avatarInfo.Name;
            }

            if (avatarInfo.Job != lastAvatarInfo.Job)
            {
                na1.Job = avatarInfo.Job;
                na1.Changed = true;

                lastAvatarInfo.Job = avatarInfo.Job;
            }

            if (avatarInfo.KillCount != lastAvatarInfo.KillCount)
            {
                na1.KillCount = avatarInfo.KillCount;
                na1.Changed = true;

                lastAvatarInfo.KillCount = avatarInfo.KillCount;
            }

            if (avatarInfo.DeadCount != lastAvatarInfo.DeadCount)
            {
                na1.DeadCount = avatarInfo.DeadCount;
                na1.Changed = true;

                lastAvatarInfo.DeadCount = avatarInfo.DeadCount;
            }

            if (avatarInfo.SecondaryAttackCount != lastAvatarInfo.SecondaryAttackCount)
            {
                na1.SecondaryAttackCount = avatarInfo.SecondaryAttackCount;
                na1.Changed = true;

                lastAvatarInfo.SecondaryAttackCount = avatarInfo.SecondaryAttackCount;
            }

            if (avatarInfo.Score != lastAvatarInfo.Score)
            {
                na1.Score = avatarInfo.Score;
                na1.Changed = true;

                lastAvatarInfo.Score = avatarInfo.Score;
            }

            if (avatarInfo.ContinueKilled != lastAvatarInfo.ContinueKilled)
            {
                na1.ContinueKilled = avatarInfo.ContinueKilled;
                na1.Changed = true;

                lastAvatarInfo.ContinueKilled = avatarInfo.ContinueKilled;
            }

            if (avatarInfo.PlayerModelInGame != lastAvatarInfo.PlayerModelInGame)
            {
                na1.PlayerModelInGame = avatarInfo.PlayerModelInGame;
                na1.Changed = true;
                lastAvatarInfo.PlayerModelInGame = avatarInfo.PlayerModelInGame;
            }

            if(avatarInfo.Level != lastAvatarInfo.Level)
            {
                na1.Level = avatarInfo.Level;
                na1.Changed = true;
                lastAvatarInfo.Level = avatarInfo.Level;
            }
            if(avatarInfo.State != lastAvatarInfo.State)
            {
                na1.State = avatarInfo.State;
                na1.Changed = true;
                lastAvatarInfo.State = avatarInfo.State;
            }
            return na1;
        }


        public void SetMaster()
        {
            avatarInfo.IsMaster = true;
        }



        public void AddSecondaryAttack()
        {
            avatarInfo.SecondaryAttackCount++;
        }

        public void AddKillCount()
        {
            avatarInfo.KillCount++;
        }

        public void AddDeadCount()
        {
            avatarInfo.DeadCount++;
        }

        public int finalScore;
        public void GetScore()
        {
            finalScore = avatarInfo.Score;
        }



        public void DecScore()
        {
            avatarInfo.ContinueKilled = 0;
            avatarInfo.Score = (int)(avatarInfo.Score * 0.8f);
        }

        public void RefreshLive()
        {
            lastReceiveTime = Util.GetTimeNow();
        }

        public bool CheckLive()
        {
            var now = Util.GetTimeNow();
            if (now - lastReceiveTime > 5)
            {
                LogHelper.Log("Actor", "Client Pause For Too Long Time");
                //this.agent.Close();
                proxy.ConnectClose();
                return false;
            }
            return true;
        }

        public void SendCmd(GCPlayerCmd.Builder cmd)
        {
            proxy.SendPacket(cmd, 0, 0);
        }


        /// <summary>
        /// 重置服务器位置 清空客户端移动命令
        /// </summary>
        public void ResetPos()
        {
            avatarInfo.ResetPos = true;
            positions.Clear();
        }

      

        #region Data
	    private int buffId = 1;
	    //private bool InitDataYet = false;
	    public int level = 1;
	    public long Exp = 0;
	    private int medal = 0;
	    private int dayBattleCount = 0;
	    private double lastReceiveTime = 0;
        #endregion


        #region ActorINROOM
        //迭代两次每次50ms 增加物理稳定性
        public override void SetPosWithPhysic(Vector3 cp, Vector3 np)
        {
            var cutNum = 8;
            var deltaPos = np - cp;
            var halfDelta = deltaPos / cutNum;
            var initPos = cp;
            for(var i = 0; i < cutNum; i++)
            {
                var p1 = initPos + halfDelta;
                var newPos1 = gridManager.FindNearestWalkableGridPos(p1);
                initPos = newPos1; 
            }
            var fixPos = MyVec3.FromVec3(initPos);
            SetPos(fixPos);
        }

        public override int GetUnitId()
        {
            return avatarInfo.PlayerModelInGame;
        }
        public override MyVec3 GetIntPos()
        {
            var myVec = new MyVec3(avatarInfo.X, avatarInfo.Y, avatarInfo.Z);
            return myVec;
        }
        public override int dir
        {
            get
            {
                return avatarInfo.Dir;
            }

            set
            {
                avatarInfo.Dir = value;
            }
        }
        public override int IDInRoom
        {
            get
            {
                return Id;
            }
        }
        public override int TeamColor
        {
            get
            {
                return avatarInfo.TeamColor;
            }
        }
        public override dynamic DuckInfo
        {
            get
            {
                return avatarInfo;
            }
        }
        public override bool IsPlayer
        {
            get
            {
                return true;
            }
        }


        //玩家不是移除而是复活处理 不需要实现该函数
        public override void RemoveSelf()
        {
            throw new NotImplementedException();
        }
        public override int Level
        {
            get
            {
                return avatarInfo.Level;
            }
        }
        #endregion
    }
}
