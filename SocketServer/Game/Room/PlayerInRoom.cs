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
        public PlayerInRoom(PlayerActor pl, AvatarInfo info)
        {
            lastAvatarInfo = AvatarInfo.CreateBuilder(info).Build();
            avatarInfo = AvatarInfo.CreateBuilder(info).Build();
            avatarInfo.Level = 1;

            proxy = new PlayerActorProxy(pl, this);
            //玩家在房间中的对象通过Room访问
            Id = pl.Id;
            ai = AddComponent<PlayerAI>();
        }

        public override void InitAfterSetRoom()
        {
            avatarInfo.TeamColor = GetRoom().GetTeamColor();
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
            LogHelper.Log("PlayerInRoom:HandleCmd", ""+msg?.msg+":"+msg.packet?.protoBody);
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




        private bool lowChange = false;
        private int lastFrameId = -1;
        /// <summary>
        /// 1:FrameID ==0  128 TCP稳定发过来
        /// 2：服务器lowChange 事件同步给客户端
        /// 3：客户端FrameID 更新
        /// Room 内玩家数据的同步
        /// </summary>
        /// <param name="cmd"></param>
	    private void UpdateData(CGPlayerCmd cmd)
        {
            if (avatarInfo == null)
            {
                avatarInfo = cmd.AvatarInfo;
                return;
            }

            if (cmd.AvatarInfo.HasIsRobot)
            {
                avatarInfo.IsRobot = cmd.AvatarInfo.IsRobot;
            }

            if (cmd.AvatarInfo.HasFrameID)
            {
                var newFrameId = cmd.AvatarInfo.FrameID;
                //newFrame == 0 为客户端发送的可靠报文 确保到达
                //TCP 发送的重新标定 ID序列的可靠报文
                if (lastFrameId == -1)
                {
                    lowChange = true;
                    lastFrameId = newFrameId;
                    avatarInfo.FrameID = newFrameId;
                }
                else if (newFrameId == 0)
                {
                    lowChange = true;
                    lastFrameId = newFrameId;
                    avatarInfo.FrameID = newFrameId;
                }
                else if (newFrameId == 128)
                {
                    lowChange = true;
                    lastFrameId = newFrameId;
                    avatarInfo.FrameID = newFrameId;
                }
                else if (newFrameId > 127 && lastFrameId <= 127)//报文不在同一个区间段 上一阶段的报文
                {
                    return;
                }
                else if (newFrameId <= 127 && lastFrameId > 127) //报文不在同一个区间段 上一阶段的报文
                {
                    return;
                }
                else if (newFrameId <= lastFrameId) //报文不是新的
                {
                    return;
                }
                else //通知客户端更新FrameID
                {
                    lastFrameId = newFrameId;
                    avatarInfo.FrameID = newFrameId;
                }
            }

            //同步速度
            if (cmd.AvatarInfo.HasSpeedX)
            {
                avatarInfo.SpeedX = cmd.AvatarInfo.SpeedX;
                avatarInfo.SpeedY = cmd.AvatarInfo.SpeedY;
            }

            if (cmd.AvatarInfo.HasX)
            {
                /*
                var curPos = GetFloatPos();
                avatarInfo.X = cmd.AvatarInfo.X;
                avatarInfo.Y = cmd.AvatarInfo.Y;
                avatarInfo.Z = cmd.AvatarInfo.Z;
                var newPos = GetFloatPos();
                ai.Move(newPos-curPos);
                */
                SetPos(new MyVec3( cmd.AvatarInfo.X, cmd.AvatarInfo.Y, cmd.AvatarInfo.Z));
            }

            if (cmd.AvatarInfo.HasDir)
            {
                avatarInfo.Dir = cmd.AvatarInfo.Dir;
            }
            if (cmd.AvatarInfo.HasHP)
            {
                avatarInfo.HP = cmd.AvatarInfo.HP;
            }
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
        private void Ready(CGPlayerCmd cmd)
        {
            GetRoom().SetReady(this);
        }

        //废弃 服务器来初始化玩家的位置
        private void InitData(CGPlayerCmd cmd, ActorMsg msg)
        {
            /*
            UpdateData(cmd);
            avatarInfo.Id = Id;
            avatarInfo.ResetPos = true;
            InitDataYet = true;
            if (lastAvatarInfo == null)
            {
                lastAvatarInfo = AvatarInfo.CreateBuilder(avatarInfo).Build();
            }
            */

            //avatarInfo = cmd.AvatarInfo;
            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "InitData";
            //agent.SendPacket(gc, msg.packet.flowId, 0);
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
            GetRoom().ChooseHero();
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

            /*
            if (InitDataYet)
            {
                InitDataYet = false;
                na1.ResetPos = true;
                na1.Changed = false;
            }
            */


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
            if (lowChange)
            {
                na1.LowChange = true;
                na1.Changed = true;
                lowChange = false;
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
            return lastAvatarInfo;
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
