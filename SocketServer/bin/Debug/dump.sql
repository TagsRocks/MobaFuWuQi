-- MySQL dump 10.13  Distrib 5.7.12, for Win64 (x86_64)
--
-- Host: localhost    Database: log_mix_cn_s999
-- ------------------------------------------------------
-- Server version	5.7.12-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `zczb_log_android_cn_s999`
--

/*!40000 DROP DATABASE IF EXISTS `zczb_log_android_cn_s999`*/;

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `zczb_log_android_cn_s999` /*!40100 DEFAULT CHARACTER SET utf8 */;

USE `zczb_log_android_cn_s999`;

--
-- Table structure for table `dict_action`
--

DROP TABLE IF EXISTS `dict_action`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `dict_action` (
  `action_id` int(11) DEFAULT NULL COMMENT '行为ID',
  `action_name` varchar(3072) DEFAULT NULL COMMENT '行为名称',
  `action_type_id` bigint(20) DEFAULT NULL COMMENT '行为类型ID',
  `level_req` int(11) DEFAULT NULL COMMENT '等级要求'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dict_action`
--

LOCK TABLES `dict_action` WRITE;
/*!40000 ALTER TABLE `dict_action` DISABLE KEYS */;
/*!40000 ALTER TABLE `dict_action` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dict_action_type`
--

DROP TABLE IF EXISTS `dict_action_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `dict_action_type` (
  `action_type_id` int(11) DEFAULT NULL,
  `action_type_name` varchar(1024) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dict_action_type`
--

LOCK TABLES `dict_action_type` WRITE;
/*!40000 ALTER TABLE `dict_action_type` DISABLE KEYS */;
INSERT INTO `dict_action_type` VALUES (1,'功能参与度'),(2,'物品商城购买'),(3,'道具产出与消耗'),(4,'货币变动'),(5,'副本参与度'),(6,'道具流通'),(7,'场景字典'),(8,'事件字典'),(9,'活动字典'),(10,'战场参与度'),(11,'战斗技能'),(12,'PVP字典'),(13,'福利字典'),(14,'聊天频道字典');
/*!40000 ALTER TABLE `dict_action_type` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dict_chat_channel`
--

DROP TABLE IF EXISTS `dict_chat_channel`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `dict_chat_channel` (
  `channel_id` int(11) DEFAULT NULL,
  `channel_name` varchar(30) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='聊天频道字典表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dict_chat_channel`
--

LOCK TABLES `dict_chat_channel` WRITE;
/*!40000 ALTER TABLE `dict_chat_channel` DISABLE KEYS */;
/*!40000 ALTER TABLE `dict_chat_channel` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dict_item`
--

DROP TABLE IF EXISTS `dict_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `dict_item` (
  `item_id` int(11) DEFAULT NULL COMMENT '道具ID',
  `item_name` varchar(50) DEFAULT NULL COMMENT '道具名称',
  `quality` int(11) DEFAULT '0' COMMENT '品质',
  `level_req` int(11) DEFAULT '0' COMMENT '使用等级要求'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dict_item`
--

LOCK TABLES `dict_item` WRITE;
/*!40000 ALTER TABLE `dict_item` DISABLE KEYS */;
/*!40000 ALTER TABLE `dict_item` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dict_link_step`
--

DROP TABLE IF EXISTS `dict_link_step`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `dict_link_step` (
  `StepId` int(11) NOT NULL DEFAULT '0' COMMENT '步骤ID',
  `NextStepId` int(11) NOT NULL DEFAULT '0' COMMENT '本步骤的下一个步骤ID',
  `StepName` varchar(100) NOT NULL DEFAULT '' COMMENT '步骤名称',
  `OrderId` int(11) NOT NULL DEFAULT '0' COMMENT '顺序ID'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='用户创建流失配置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dict_link_step`
--

LOCK TABLES `dict_link_step` WRITE;
/*!40000 ALTER TABLE `dict_link_step` DISABLE KEYS */;
INSERT INTO `dict_link_step` VALUES (100,0,'开始匹配',0),(101,0,'退出房间',0),(102,0,'匹配成功进入房间',0),(103,0,'取消匹配',0);
/*!40000 ALTER TABLE `dict_link_step` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dict_open`
--

DROP TABLE IF EXISTS `dict_open`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `dict_open` (
  `action_id` int(10) DEFAULT '0',
  `last_id` int(10) DEFAULT '0',
  `line_id` int(10) DEFAULT '0',
  `action_name` varchar(50) DEFAULT NULL,
  `is_recorded` int(2) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dict_open`
--

LOCK TABLES `dict_open` WRITE;
/*!40000 ALTER TABLE `dict_open` DISABLE KEYS */;
INSERT INTO `dict_open` VALUES (1001,1000,1,'到达插件引导页',0),(1002,1001,1,'判断浏览器是否支持插件',0),(1003,1002,1,'支持插件并判断是否已安装插件',0),(2000,1003,2,'已安装插件（跳过插件下载页）',1),(2001,2000,2,'显示游戏加载界面',0),(2010,2001,2,'进入选服页面',0),(2020,2010,2,'进入创角页面',0),(2030,2020,2,'创角成功并点击进入游戏按钮',1),(2040,2030,2,'进入游戏场景',0),(3000,1003,3,'未安装插件',1),(3001,3000,3,'点击下载',0),(4001,3001,4,'点击下载插件按钮',1),(4002,4001,4,'安装插件成功并启动',1),(4003,4002,4,'显示游戏加载界面',0),(4010,4003,4,'进入选服页面',0),(4020,4010,4,'进入创角页面',0),(4030,4020,4,'创角成功并点击进入游戏按钮',1),(4040,4030,4,'进入游戏场景',0),(5001,3001,5,'点击下载微端按钮',1),(5002,5001,5,'安装微端成功并启动',1),(5003,5002,5,'显示游戏加载界面',0),(5010,5003,5,'进入选服页面',0),(5020,5010,5,'进入创角页面',0),(5030,5020,5,'创角成功并点击进入游戏按钮',1),(5040,5030,5,'进入游戏场景',0),(6001,1003,6,'不支持插件并进行弹窗推荐下载微端',1),(6002,6001,6,'点击下载微端按钮',1),(6003,6002,6,'安装微端成功并启动',1),(6004,6003,6,'显示游戏加载界面',0),(6010,6004,6,'进入选服页面',0),(6020,6010,6,'进入创角页面',0),(6030,6020,6,'创角成功并点击进入游戏按钮',1),(6040,6030,6,'进入游戏场景',0);
/*!40000 ALTER TABLE `dict_open` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dict_prof`
--

DROP TABLE IF EXISTS `dict_prof`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `dict_prof` (
  `prof_id` int(11) DEFAULT NULL,
  `prof_name` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='职业字典表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dict_prof`
--

LOCK TABLES `dict_prof` WRITE;
/*!40000 ALTER TABLE `dict_prof` DISABLE KEYS */;
/*!40000 ALTER TABLE `dict_prof` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dict_task`
--

DROP TABLE IF EXISTS `dict_task`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `dict_task` (
  `task_id` int(11) DEFAULT NULL,
  `task_name` varchar(50) DEFAULT NULL,
  `task_type` int(11) DEFAULT NULL,
  `level_req_min` int(11) DEFAULT NULL,
  `level_req_max` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='任务字典表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dict_task`
--

LOCK TABLES `dict_task` WRITE;
/*!40000 ALTER TABLE `dict_task` DISABLE KEYS */;
/*!40000 ALTER TABLE `dict_task` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dict_task_type`
--

DROP TABLE IF EXISTS `dict_task_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `dict_task_type` (
  `task_type_id` tinyint(4) NOT NULL DEFAULT '0',
  `task_type_name` varchar(32) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='任务类型字典表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dict_task_type`
--

LOCK TABLES `dict_task_type` WRITE;
/*!40000 ALTER TABLE `dict_task_type` DISABLE KEYS */;
/*!40000 ALTER TABLE `dict_task_type` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_activity`
--

DROP TABLE IF EXISTS `tbllog_activity`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_activity` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `action_id` int(11) DEFAULT NULL,
  `status` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='功能参与度日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_activity`
--

LOCK TABLES `tbllog_activity` WRITE;
/*!40000 ALTER TABLE `tbllog_activity` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_activity` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_auction`
--

DROP TABLE IF EXISTS `tbllog_auction`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_auction` (
  `platform` varchar(50) DEFAULT NULL,
  `auction_id` int(11) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `opt_type_id` int(11) DEFAULT NULL,
  `item_id` int(11) DEFAULT NULL,
  `item_number` int(11) DEFAULT NULL,
  `bid_price_list` varchar(255) DEFAULT NULL,
  `a_price_list` varchar(255) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='角色拍卖表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_auction`
--

LOCK TABLES `tbllog_auction` WRITE;
/*!40000 ALTER TABLE `tbllog_auction` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_auction` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_battle`
--

DROP TABLE IF EXISTS `tbllog_battle`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_battle` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `battle_id` int(50) DEFAULT NULL,
  `time_duration` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='战场参与度日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_battle`
--

LOCK TABLES `tbllog_battle` WRITE;
/*!40000 ALTER TABLE `tbllog_battle` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_battle` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_chat`
--

DROP TABLE IF EXISTS `tbllog_chat`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_chat` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `platform` char(32) NOT NULL DEFAULT '' COMMENT '渠道名',
  `account_name` char(50) NOT NULL DEFAULT '' COMMENT '账号名',
  `role_id` int(10) unsigned NOT NULL DEFAULT '0' COMMENT '角色id',
  `role_name` char(32) NOT NULL DEFAULT '' COMMENT '角色名',
  `dim_level` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT '角色等级',
  `user_ip` char(16) NOT NULL DEFAULT '' COMMENT '用户ip',
  `channel` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT '聊天频道',
  `msg` varchar(1024) NOT NULL DEFAULT '' COMMENT '聊天内容',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT '聊天内容类型, 0语音, 1文本',
  `target_role_id` int(10) unsigned NOT NULL DEFAULT '0' COMMENT '聊天对象id',
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(10) unsigned NOT NULL DEFAULT '0' COMMENT '聊天发生时间',
  `log_time` int(10) unsigned NOT NULL DEFAULT '0' COMMENT '日志记录时间',
  PRIMARY KEY (`id`),
  KEY `role_id` (`role_id`),
  KEY `account_name` (`account_name`),
  KEY `log_time` (`log_time`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 COMMENT='聊天日志表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_chat`
--

LOCK TABLES `tbllog_chat` WRITE;
/*!40000 ALTER TABLE `tbllog_chat` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_chat` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_checkin`
--

DROP TABLE IF EXISTS `tbllog_checkin`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_checkin` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='战斗技能使用度日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_checkin`
--

LOCK TABLES `tbllog_checkin` WRITE;
/*!40000 ALTER TABLE `tbllog_checkin` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_checkin` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_complaints`
--

DROP TABLE IF EXISTS `tbllog_complaints`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_complaints` (
  `platform` varchar(50) DEFAULT NULL,
  `complaint_id` int(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `role_name` varchar(255) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `game_abbrv` varchar(255) DEFAULT NULL,
  `sid` int(50) DEFAULT NULL,
  `complaint_type` int(11) DEFAULT NULL,
  `complaint_title` varchar(255) DEFAULT NULL,
  `complaint_content` varchar(255) DEFAULT NULL,
  `complaint_time` int(11) DEFAULT NULL,
  `internal_id` int(11) DEFAULT NULL,
  `reply_cnts` int(11) DEFAULT NULL,
  `user_ip` varchar(255) DEFAULT NULL,
  `agent` varchar(255) DEFAULT NULL,
  `pay_amount` int(11) DEFAULT NULL,
  `qq_acount` int(11) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `evaluate` int(11) DEFAULT NULL,
  `sync_numbers` int(11) DEFAULT NULL,
  `last_reply_time` int(11) DEFAULT NULL,
  `is_spam` int(11) DEFAULT NULL,
  `spam_reporter` varchar(255) DEFAULT NULL,
  `spam_time` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='GM用户反馈';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_complaints`
--

LOCK TABLES `tbllog_complaints` WRITE;
/*!40000 ALTER TABLE `tbllog_complaints` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_complaints` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_deal`
--

DROP TABLE IF EXISTS `tbllog_deal`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_deal` (
  `platform` varchar(50) DEFAULT NULL,
  `deal_id` int(11) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `owner_id` int(11) DEFAULT NULL,
  `item_id` int(11) DEFAULT NULL,
  `item_number` int(11) DEFAULT NULL,
  `owner_item_id` int(11) DEFAULT NULL,
  `owner_item_number` int(11) DEFAULT NULL,
  `status` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='玩家交易表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_deal`
--

LOCK TABLES `tbllog_deal` WRITE;
/*!40000 ALTER TABLE `tbllog_deal` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_deal` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_equipment`
--

DROP TABLE IF EXISTS `tbllog_equipment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_equipment` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `item_id` int(11) DEFAULT NULL,
  `item_property` int(11) DEFAULT NULL,
  `value_before` int(11) DEFAULT NULL,
  `value_after` int(11) DEFAULT NULL,
  `change_type` int(11) DEFAULT NULL,
  `material` varchar(255) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='装备锻造、洗练、合成日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_equipment`
--

LOCK TABLES `tbllog_equipment` WRITE;
/*!40000 ALTER TABLE `tbllog_equipment` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_equipment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_error`
--

DROP TABLE IF EXISTS `tbllog_error`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_error` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `error_msg` varchar(1000) DEFAULT NULL,
  `did` varchar(100) DEFAULT NULL,
  `game_version` varchar(50) DEFAULT NULL,
  `os` varchar(255) DEFAULT NULL,
  `os_version` varchar(255) DEFAULT NULL,
  `device_name` varchar(255) DEFAULT NULL,
  `screen` varchar(255) DEFAULT NULL,
  `mno` varchar(255) DEFAULT NULL,
  `nm` varchar(255) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0',
  KEY `log_time` (`log_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='错误日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_error`
--

LOCK TABLES `tbllog_error` WRITE;
/*!40000 ALTER TABLE `tbllog_error` DISABLE KEYS */;
INSERT INTO `tbllog_error` VALUES (NULL,NULL,NULL,'Test',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'android',NULL,13050042);
/*!40000 ALTER TABLE `tbllog_error` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_event`
--

DROP TABLE IF EXISTS `tbllog_event`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_event` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL COMMENT '角色ID',
  `account_name` varchar(50) DEFAULT NULL COMMENT '平台账户',
  `event_id` int(11) DEFAULT NULL COMMENT '步骤(自定义)',
  `user_ip` varchar(20) DEFAULT NULL COMMENT '用户IP',
  `did` varchar(200) DEFAULT NULL,
  `game_version` varchar(50) DEFAULT NULL,
  `os` varchar(50) DEFAULT NULL,
  `os_version` varchar(50) DEFAULT NULL,
  `device_name` varchar(50) DEFAULT NULL,
  `screen` varchar(50) DEFAULT NULL,
  `mno` varchar(50) DEFAULT NULL,
  `nm` varchar(50) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL COMMENT '事件发生时间',
  `log_time` int(11) DEFAULT '0',
  KEY `log_time` (`log_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='页面加载日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_event`
--

LOCK TABLES `tbllog_event` WRITE;
/*!40000 ALTER TABLE `tbllog_event` DISABLE KEYS */;
INSERT INTO `tbllog_event` VALUES (NULL,NULL,NULL,100,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',12993913,12993913),(NULL,NULL,NULL,100,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',12994507,12994507),(NULL,NULL,NULL,101,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',12994522,12994522),(NULL,NULL,NULL,100,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',12994523,12994523),(NULL,NULL,NULL,101,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',12994527,12994527),(NULL,NULL,NULL,100,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',12994528,12994528),(NULL,NULL,NULL,101,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',12994534,12994534),(NULL,NULL,NULL,100,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',12994630,12994630),(NULL,NULL,NULL,101,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',12994635,12994635),(NULL,NULL,NULL,100,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',13048913,13048913),(NULL,NULL,NULL,101,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',13048958,13048958),(NULL,NULL,NULL,100,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',13049005,13049005),(NULL,NULL,NULL,101,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',13049030,13049030),(NULL,NULL,NULL,100,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',13049068,13049068),(NULL,NULL,NULL,101,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',13049077,13049077),(NULL,NULL,NULL,100,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',13049184,13049184),(NULL,NULL,NULL,101,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',13049195,13049195),(NULL,NULL,NULL,100,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',1464658064,1464658064);
/*!40000 ALTER TABLE `tbllog_event` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_fb`
--

DROP TABLE IF EXISTS `tbllog_fb`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_fb` (
  `platform` varchar(50) DEFAULT NULL,
  `fb_id` int(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `fb_level` int(11) DEFAULT NULL,
  `status` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='副本参与度';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_fb`
--

LOCK TABLES `tbllog_fb` WRITE;
/*!40000 ALTER TABLE `tbllog_fb` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_fb` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_gold`
--

DROP TABLE IF EXISTS `tbllog_gold`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_gold` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `dim_prof` int(11) DEFAULT NULL,
  `money_type` bigint(20) DEFAULT NULL,
  `amount` int(11) DEFAULT NULL,
  `money_remain` int(11) DEFAULT NULL,
  `opt` int(11) DEFAULT NULL,
  `action_1` int(11) DEFAULT NULL,
  `action_2` int(11) DEFAULT NULL,
  `item_number` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='货币变动日志'
/*!50100 PARTITION BY RANGE (log_time)
(PARTITION p201409 VALUES LESS THAN (1412121600) ENGINE = InnoDB,
 PARTITION p201410 VALUES LESS THAN (1414800000) ENGINE = InnoDB) */;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_gold`
--

LOCK TABLES `tbllog_gold` WRITE;
/*!40000 ALTER TABLE `tbllog_gold` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_gold` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_items`
--

DROP TABLE IF EXISTS `tbllog_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_items` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `opt` int(11) DEFAULT NULL,
  `action_id` int(11) DEFAULT NULL,
  `item_id` int(11) DEFAULT NULL,
  `item_number` int(11) DEFAULT NULL,
  `map_id` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='道具产出/消耗日志'
/*!50100 PARTITION BY RANGE (log_time)
(PARTITION p201409 VALUES LESS THAN (1412121600) ENGINE = InnoDB,
 PARTITION p201410 VALUES LESS THAN (1414800000) ENGINE = InnoDB) */;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_items`
--

LOCK TABLES `tbllog_items` WRITE;
/*!40000 ALTER TABLE `tbllog_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_level_up`
--

DROP TABLE IF EXISTS `tbllog_level_up`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_level_up` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `last_level` int(11) DEFAULT NULL,
  `current_level` int(11) DEFAULT NULL,
  `last_exp` bigint(20) DEFAULT NULL,
  `current_exp` bigint(20) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='等级变动日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_level_up`
--

LOCK TABLES `tbllog_level_up` WRITE;
/*!40000 ALTER TABLE `tbllog_level_up` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_level_up` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_login`
--

DROP TABLE IF EXISTS `tbllog_login`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_login` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `user_ip` varchar(20) DEFAULT NULL,
  `login_scene_id` int(11) DEFAULT NULL,
  `did` varchar(200) DEFAULT NULL,
  `game_version` varchar(50) DEFAULT NULL,
  `os` varchar(50) DEFAULT NULL,
  `os_version` varchar(50) DEFAULT NULL,
  `device_name` varchar(50) DEFAULT NULL,
  `screen` varchar(50) DEFAULT NULL,
  `mno` varchar(50) DEFAULT NULL,
  `nm` varchar(50) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0',
  KEY `log_time` (`log_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='登陆日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_login`
--

LOCK TABLES `tbllog_login` WRITE;
/*!40000 ALTER TABLE `tbllog_login` DISABLE KEYS */;
INSERT INTO `tbllog_login` VALUES (NULL,NULL,NULL,NULL,'127.0.0.1',NULL,'0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',12994473,12994473),(NULL,NULL,NULL,NULL,'127.0.0.1',NULL,'0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',12994627,12994627),(NULL,NULL,NULL,NULL,'127.0.0.1',NULL,'0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',13048910,13048910),(NULL,NULL,NULL,NULL,'127.0.0.1',NULL,'0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',13049000,13049000),(NULL,NULL,NULL,NULL,'127.0.0.1',NULL,'0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',13049063,13049063),(NULL,NULL,NULL,NULL,'127.0.0.1',NULL,'0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',13049182,13049182),(NULL,NULL,NULL,NULL,'127.0.0.1',NULL,'0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0',NULL,NULL,NULL,NULL,NULL,NULL,'android',1464658062,1464658062);
/*!40000 ALTER TABLE `tbllog_login` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_mail`
--

DROP TABLE IF EXISTS `tbllog_mail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_mail` (
  `platform` varchar(50) DEFAULT NULL,
  `mail_id` int(11) DEFAULT NULL,
  `mail_sender_id` int(11) DEFAULT NULL,
  `mail_sender_name` varchar(255) DEFAULT NULL,
  `mail_receiver_id` int(11) DEFAULT NULL,
  `mail_receiver_name` varchar(255) DEFAULT NULL,
  `mail_title` varchar(255) DEFAULT NULL,
  `mail_content` varchar(255) DEFAULT NULL,
  `mail_type` int(11) DEFAULT NULL,
  `mail_money_list` varchar(255) DEFAULT NULL,
  `mail_item_list` varchar(255) DEFAULT NULL,
  `mail_status` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='邮件流水日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_mail`
--

LOCK TABLES `tbllog_mail` WRITE;
/*!40000 ALTER TABLE `tbllog_mail` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_mail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_money_remain`
--

DROP TABLE IF EXISTS `tbllog_money_remain`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_money_remain` (
  `money_type` int(11) DEFAULT NULL,
  `money_remain` int(11) DEFAULT NULL,
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='货币剩余日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_money_remain`
--

LOCK TABLES `tbllog_money_remain` WRITE;
/*!40000 ALTER TABLE `tbllog_money_remain` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_money_remain` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_online`
--

DROP TABLE IF EXISTS `tbllog_online`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_online` (
  `platform` varchar(50) DEFAULT NULL,
  `people` varchar(11) DEFAULT NULL,
  `device_cnt` int(11) DEFAULT NULL,
  `ip_cnt` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0',
  KEY `log_time` (`log_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='在线日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_online`
--

LOCK TABLES `tbllog_online` WRITE;
/*!40000 ALTER TABLE `tbllog_online` DISABLE KEYS */;
INSERT INTO `tbllog_online` VALUES (NULL,'0',NULL,NULL,'android',13049533,13049533),(NULL,'0',NULL,NULL,'android',13049538,13049538);
/*!40000 ALTER TABLE `tbllog_online` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_open`
--

DROP TABLE IF EXISTS `tbllog_open`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_open` (
  `platform` varchar(50) DEFAULT NULL,
  `device` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `action_id` int(11) DEFAULT '0',
  `game_version` varchar(50) DEFAULT NULL,
  `user_ip` varchar(20) DEFAULT NULL,
  `did` varchar(200) DEFAULT NULL,
  `device_name` varchar(50) DEFAULT NULL,
  `os` varchar(50) DEFAULT NULL,
  `os_version` varchar(50) DEFAULT NULL,
  `happend_time` int(11) DEFAULT '0',
  `log_time` int(11) DEFAULT '0',
  UNIQUE KEY `account_name` (`account_name`,`action_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='引导页日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_open`
--

LOCK TABLES `tbllog_open` WRITE;
/*!40000 ALTER TABLE `tbllog_open` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_open` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_pay`
--

DROP TABLE IF EXISTS `tbllog_pay`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_pay` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `user_ip` varchar(20) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `order_id` varchar(256) DEFAULT NULL,
  `pay_money` float DEFAULT NULL,
  `pay_gold` int(20) DEFAULT NULL,
  `did` varchar(200) DEFAULT NULL,
  `game_version` varchar(50) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='充值日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_pay`
--

LOCK TABLES `tbllog_pay` WRITE;
/*!40000 ALTER TABLE `tbllog_pay` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_pay` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_player`
--

DROP TABLE IF EXISTS `tbllog_player`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_player` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `role_name` varchar(255) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `user_name` varchar(255) DEFAULT NULL,
  `dim_nation` varchar(255) DEFAULT NULL,
  `dim_prof` varchar(255) DEFAULT NULL,
  `dim_sex` tinyint(11) DEFAULT '2' COMMENT '0-男，1-女，2-未知',
  `reg_time` int(11) DEFAULT NULL,
  `reg_ip` varchar(20) DEFAULT NULL,
  `did` varchar(200) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `dim_vip_level` int(11) DEFAULT NULL,
  `dim_exp` int(11) DEFAULT NULL,
  `dim_guild` varchar(255) DEFAULT NULL,
  `dim_power` int(11) DEFAULT NULL,
  `coin_number` int(11) DEFAULT NULL,
  `gold_number` int(11) DEFAULT NULL,
  `pay_money` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `first_pay_time` int(11) DEFAULT NULL,
  `last_pay_time` int(11) DEFAULT NULL,
  `last_login_time` int(11) DEFAULT NULL,
  `happend_time` int(11) DEFAULT NULL,
  KEY `did_index` (`did`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='用户信息表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_player`
--

LOCK TABLES `tbllog_player` WRITE;
/*!40000 ALTER TABLE `tbllog_player` DISABLE KEYS */;
INSERT INTO `tbllog_player` VALUES ('',NULL,NULL,'','',NULL,NULL,2,12989904,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'android',NULL,NULL,1464658062,1464658062),('',NULL,NULL,'','',NULL,NULL,2,12990147,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'android',NULL,NULL,1464658062,1464658062),('',NULL,NULL,'','',NULL,NULL,2,12990286,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'android',NULL,NULL,1464658062,1464658062),('',NULL,NULL,'','',NULL,NULL,2,12990403,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'android',NULL,NULL,1464658062,1464658062),('',NULL,NULL,'','',NULL,NULL,2,12990436,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'android',NULL,NULL,1464658062,1464658062),('',NULL,NULL,'','',NULL,NULL,2,12991253,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'android',NULL,NULL,1464658062,1464658062),('',NULL,NULL,'','',NULL,NULL,2,12991329,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'android',NULL,NULL,1464658062,1464658062),('',NULL,NULL,'','',NULL,NULL,2,12991401,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'android',NULL,NULL,1464658062,1464658062),('',NULL,NULL,'','',NULL,NULL,2,12991521,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'android',NULL,NULL,1464658062,1464658062),('',NULL,NULL,'','',NULL,NULL,2,12991579,'127.0.0.1','0fb5e89a4a08a63642627d2032e7da799370589a',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'android',NULL,NULL,1464658062,1464658062);
/*!40000 ALTER TABLE `tbllog_player` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_pvp`
--

DROP TABLE IF EXISTS `tbllog_pvp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_pvp` (
  `platform` varchar(50) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `pvp_type` int(11) DEFAULT NULL,
  `pvp_id` int(11) DEFAULT NULL,
  `status` int(11) DEFAULT NULL,
  `time_duration` int(11) DEFAULT NULL,
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='PVP参与度日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_pvp`
--

LOCK TABLES `tbllog_pvp` WRITE;
/*!40000 ALTER TABLE `tbllog_pvp` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_pvp` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_quit`
--

DROP TABLE IF EXISTS `tbllog_quit`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_quit` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `login_level` int(11) DEFAULT NULL,
  `logout_level` int(11) DEFAULT NULL,
  `logout_ip` varchar(50) DEFAULT NULL,
  `login_time` int(11) DEFAULT NULL,
  `logout_time` int(11) DEFAULT NULL,
  `time_duration` int(11) DEFAULT NULL,
  `logout_scene_id` bigint(20) DEFAULT NULL,
  `reason_id` int(11) DEFAULT NULL,
  `msg` varchar(200) DEFAULT NULL,
  `did` varchar(200) DEFAULT NULL,
  `game_version` varchar(50) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `log_time` int(11) DEFAULT '0',
  KEY `log_time` (`log_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='退出日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_quit`
--

LOCK TABLES `tbllog_quit` WRITE;
/*!40000 ALTER TABLE `tbllog_quit` DISABLE KEYS */;
INSERT INTO `tbllog_quit` VALUES (NULL,NULL,NULL,NULL,NULL,'127.0.0.1',13049182,13049195,13,NULL,NULL,NULL,'0fb5e89a4a08a63642627d2032e7da799370589a','1.0.0','android',13049195);
/*!40000 ALTER TABLE `tbllog_quit` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_role`
--

DROP TABLE IF EXISTS `tbllog_role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_role` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `role_name` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `user_ip` varchar(20) DEFAULT NULL,
  `dim_prof` int(11) DEFAULT NULL,
  `dim_sex` tinyint(11) DEFAULT '2',
  `did` varchar(200) DEFAULT NULL,
  `game_version` varchar(50) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='角色创建日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_role`
--

LOCK TABLES `tbllog_role` WRITE;
/*!40000 ALTER TABLE `tbllog_role` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_role` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_sales`
--

DROP TABLE IF EXISTS `tbllog_sales`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_sales` (
  `platform` varchar(50) DEFAULT NULL,
  `sales_id` int(11) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `item_id` int(11) DEFAULT NULL,
  `price_type` int(11) DEFAULT NULL,
  `price_unit` int(11) DEFAULT NULL,
  `item_number` int(11) DEFAULT NULL,
  `action_id` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='玩家寄售表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_sales`
--

LOCK TABLES `tbllog_sales` WRITE;
/*!40000 ALTER TABLE `tbllog_sales` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_sales` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_scene_online`
--

DROP TABLE IF EXISTS `tbllog_scene_online`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_scene_online` (
  `platform` varchar(50) DEFAULT NULL,
  `scene_id` int(11) DEFAULT NULL,
  `player_num` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='场景在线';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_scene_online`
--

LOCK TABLES `tbllog_scene_online` WRITE;
/*!40000 ALTER TABLE `tbllog_scene_online` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_scene_online` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_shop`
--

DROP TABLE IF EXISTS `tbllog_shop`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_shop` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `shopId` varchar(32) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `dim_prof` int(11) DEFAULT NULL,
  `dim_nation` int(11) DEFAULT NULL,
  `money_type` int(11) DEFAULT NULL,
  `amount` int(11) DEFAULT NULL,
  `item_type_1` int(11) DEFAULT NULL,
  `item_type_2` int(11) DEFAULT NULL,
  `item_id` int(11) DEFAULT NULL,
  `item_number` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='物品(商城)购买日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_shop`
--

LOCK TABLES `tbllog_shop` WRITE;
/*!40000 ALTER TABLE `tbllog_shop` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_shop` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_skill`
--

DROP TABLE IF EXISTS `tbllog_skill`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_skill` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `skill_id` int(50) DEFAULT NULL,
  `used_num` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='战斗技能使用度日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_skill`
--

LOCK TABLES `tbllog_skill` WRITE;
/*!40000 ALTER TABLE `tbllog_skill` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_skill` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_task`
--

DROP TABLE IF EXISTS `tbllog_task`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_task` (
  `platform` varchar(50) DEFAULT NULL,
  `role_id` varchar(50) DEFAULT NULL,
  `account_name` varchar(50) DEFAULT NULL,
  `dim_prof` int(11) DEFAULT NULL,
  `dim_level` int(11) DEFAULT NULL,
  `task_id` int(50) DEFAULT NULL,
  `status` int(11) DEFAULT NULL,
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) DEFAULT NULL,
  `log_time` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='任务日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_task`
--

LOCK TABLES `tbllog_task` WRITE;
/*!40000 ALTER TABLE `tbllog_task` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_task` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tbllog_user_item`
--

DROP TABLE IF EXISTS `tbllog_user_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tbllog_user_item` (
  `platform` varchar(11) NOT NULL COMMENT '平台简称',
  `role_id` bigint(20) NOT NULL COMMENT '角色id',
  `account_name` varchar(30) NOT NULL COMMENT '账户名',
  `role_name` varchar(30) NOT NULL COMMENT '角色名',
  `item_id` bigint(20) NOT NULL COMMENT '道具id',
  `is_bind` int(2) NOT NULL COMMENT '是否绑定，可以忽略',
  `strengthen_level` int(10) NOT NULL COMMENT '强化等级，可以忽略',
  `item_amount` bigint(20) NOT NULL COMMENT '道具数量',
  `item_position` bigint(20) NOT NULL COMMENT '道具位置',
  `device` varchar(50) DEFAULT 'android' COMMENT '设备端, 默认值为 android,ios,pc,web',
  `happend_time` int(11) NOT NULL COMMENT '时间',
  `state` int(2) NOT NULL COMMENT '状态，可忽略',
  `bag_type` int(11) NOT NULL COMMENT '背包类型，可忽略'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='玩家剩余装备表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tbllog_user_item`
--

LOCK TABLES `tbllog_user_item` WRITE;
/*!40000 ALTER TABLE `tbllog_user_item` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbllog_user_item` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2016-05-31 10:15:17
