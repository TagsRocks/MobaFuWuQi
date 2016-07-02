/*
Navicat MySQL Data Transfer

Source Server         : local_copy_copy
Source Server Version : 50712
Source Host           : localhost:3306
Source Database       : tank

Target Server Type    : MYSQL
Target Server Version : 50712
File Encoding         : 65001

Date: 2016-06-27 14:48:43
*/

SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for `banip`
-- ----------------------------
DROP TABLE IF EXISTS `banip`;
CREATE TABLE `banip` (
  `did` varchar(255) DEFAULT NULL,
  `end_time` int(11) DEFAULT NULL,
  KEY `did` (`did`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of banip
-- ----------------------------
INSERT INTO banip VALUES ('0fb5e89a4a08a63642627d2032e7da799370589a', '1400000000');

-- ----------------------------
-- Table structure for `gmfeedback`
-- ----------------------------
DROP TABLE IF EXISTS `gmfeedback`;
CREATE TABLE `gmfeedback` (
  `ComplaintId` int(11) NOT NULL AUTO_INCREMENT,
  `RoleId` bigint(20) NOT NULL,
  `RoleName` varchar(255) NOT NULL,
  `AccountName` varchar(255) NOT NULL,
  `ComplaintSubmitTime` int(11) NOT NULL,
  `ComplaintType` tinyint(4) NOT NULL,
  `Title` varchar(255) NOT NULL DEFAULT '',
  `Content` text NOT NULL,
  `Platform` varchar(255) NOT NULL,
  `Game` varchar(255) NOT NULL DEFAULT 'xztk',
  `Server` varchar(255) NOT NULL DEFAULT 'S888',
  `ReplyFlag` tinyint(4) NOT NULL DEFAULT '0',
  `SetSpamFlag` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`ComplaintId`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of gmfeedback
-- ----------------------------

-- ----------------------------
-- Table structure for `ipwhite`
-- ----------------------------
DROP TABLE IF EXISTS `ipwhite`;
CREATE TABLE `ipwhite` (
  `ip` varchar(255) DEFAULT NULL,
  KEY `ip` (`ip`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of ipwhite
-- ----------------------------
INSERT INTO ipwhite VALUES ('123.91.23.12');
INSERT INTO ipwhite VALUES ('1234354');

-- ----------------------------
-- Table structure for `login`
-- ----------------------------
DROP TABLE IF EXISTS `login`;
CREATE TABLE `login` (
  `pid` varchar(255) NOT NULL,
  `uid` varchar(255) NOT NULL,
  `uname` varchar(30) NOT NULL DEFAULT '',
  `level` tinyint(4) NOT NULL DEFAULT '1',
  `exp` int(11) NOT NULL DEFAULT '0',
  `medal` mediumint(9) NOT NULL DEFAULT '0',
  `battleTotalCount` int(11) NOT NULL DEFAULT '0',
  `winBattleCount` int(11) NOT NULL DEFAULT '0',
  `dayBattleCount` int(11) NOT NULL DEFAULT '0',
  `userRename` tinyint(4) NOT NULL DEFAULT '0',
  UNIQUE KEY `PidUid` (`pid`,`uid`) USING BTREE,
  KEY `uname` (`uname`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of login
-- ----------------------------
INSERT INTO login VALUES ('0', '11', '大师眼睛的爱', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '111', '丢了蚂蚁灰灰', '1', '2000', '0', '0', '0', '1', '0');
INSERT INTO login VALUES ('0', '1112', '颓丧的伯尔', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '1116', '惊讶的戴利', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '115', '性感小白的梦', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '12', '都是等待的花', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '123', '读书情人毛毛', '5', '5100', '0', '0', '0', '8', '0');
INSERT INTO login VALUES ('0', '13', '搁浅美丽之殇', '2', '600', '0', '0', '0', '3', '0');
INSERT INTO login VALUES ('0', '132', '小老鼠的马', '3', '5860', '0', '0', '0', '8', '0');
INSERT INTO login VALUES ('0', '133', '宇智波之剑', '3', '840', '0', '0', '0', '6', '0');
INSERT INTO login VALUES ('0', '134', '狂喜的艾丽斯', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '135', '无奈的华莱士', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '143', '惊恐的特雷弗', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '145', '焦虑的乔纳', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '146', '开心的威廉', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '167', '奋斗的奥帕尔', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '178', '激怒的特罗伊', '1', '2000', '0', '0', '0', '1', '0');
INSERT INTO login VALUES ('0', '189', '哀伤的亨特利', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '2', '还有太阳丫的', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '213', '天子童话蝴蝶', '1', '4000', '0', '0', '0', '2', '0');
INSERT INTO login VALUES ('0', '21312', '后悔的菲奥娜', '1', '4000', '0', '0', '0', '2', '0');
INSERT INTO login VALUES ('0', '22', '蜗牛的龙', '1', '2000', '0', '0', '0', '1', '0');
INSERT INTO login VALUES ('0', '226', '约定月亮兔子', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '228', '旋律风筝薇薇', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '23', '寂静情人翅膀', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '231', '美人鱼玫瑰', '1', '2000', '0', '0', '0', '1', '0');
INSERT INTO login VALUES ('0', '234', '小丫头之爱', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '25', '书生翅膀的脚', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '32', '小飞侠童年', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '321', '小蚊子的爱', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '348', '一起烦恼的海', '1', '1800', '0', '0', '0', '1', '0');
INSERT INTO login VALUES ('0', '356', '死亡童话的花', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '384', '下辈子怪咖', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '393', '天下小白薇薇', '1', '2000', '0', '0', '0', '1', '0');
INSERT INTO login VALUES ('0', '414', '——情人的伤', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '509', '快乐时光的鸟', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '525', '没名字疯子', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '577', '不过朋友思念', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '615', '年轻翅膀的我', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '707', '快乐小白的我', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '734', '地球记忆菜菜', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '777', '人是人生的鸟', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '783', '悲伤男孩温暖', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '800', '欧阳快乐季节', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '846', '小可爱的云', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '942', '随心小马的家', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO login VALUES ('0', '姘寸數璐规按鐢佃垂鐨?3', '焦急的帕姆', '1', '0', '0', '0', '0', '0', '0');

-- ----------------------------
-- Table structure for `mail`
-- ----------------------------
DROP TABLE IF EXISTS `mail`;
CREATE TABLE `mail` (
  `mailid` int(11) NOT NULL AUTO_INCREMENT,
  `pid` varchar(255) NOT NULL,
  `uid` varchar(255) NOT NULL,
  `title` varchar(255) NOT NULL,
  `sender` varchar(255) NOT NULL DEFAULT '系统邮件',
  `sendtime` int(11) NOT NULL,
  `state` tinyint(4) NOT NULL,
  `content` varchar(4096) NOT NULL DEFAULT '',
  PRIMARY KEY (`mailid`),
  KEY `PidUid` (`pid`,`uid`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of mail
-- ----------------------------

-- ----------------------------
-- Table structure for `record`
-- ----------------------------
DROP TABLE IF EXISTS `record`;
CREATE TABLE `record` (
  `uid` varchar(255) NOT NULL,
  `total` int(11) DEFAULT '0',
  `mvp` int(11) DEFAULT '0',
  `threeKill` int(11) DEFAULT '0',
  `fourKill` int(11) DEFAULT '0',
  `fiveKill` int(11) DEFAULT '0',
  `totalKill` int(11) DEFAULT '0',
  `dieNum` int(11) DEFAULT '0',
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of record
-- ----------------------------
INSERT INTO record VALUES ('1112', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO record VALUES ('1116', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO record VALUES ('115', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO record VALUES ('123', '28', '8', '0', '0', '0', '0', '2');
INSERT INTO record VALUES ('13', '10', '2', '1', '1', '1', '6', '8');
INSERT INTO record VALUES ('132', '19', '5', '3', '1', '0', '17', '28');
INSERT INTO record VALUES ('133', '7', '1', '0', '0', '0', '1', '18');
INSERT INTO record VALUES ('134', '2', '0', '0', '0', '0', '0', '0');
INSERT INTO record VALUES ('135', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO record VALUES ('143', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO record VALUES ('145', '1', '0', '0', '0', '0', '0', '1');
INSERT INTO record VALUES ('146', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO record VALUES ('167', '1', '0', '0', '0', '0', '0', '0');
INSERT INTO record VALUES ('178', '2', '1', '0', '0', '0', '0', '0');
INSERT INTO record VALUES ('189', '3', '0', '0', '0', '0', '1', '0');
INSERT INTO record VALUES ('213', '2', '1', '0', '0', '0', '0', '3');
INSERT INTO record VALUES ('21312', '5', '2', '0', '0', '0', '0', '0');

-- ----------------------------
-- Procedure structure for `ClearDayBattle`
-- ----------------------------
DROP PROCEDURE IF EXISTS `ClearDayBattle`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `ClearDayBattle`()
BEGIN
	#Routine body goes here...
	update login set dayBattleCount = 0;
END
;;
DELIMITER ;

-- ----------------------------
-- Event structure for `ClearDayBattleCount`
-- ----------------------------
DROP EVENT IF EXISTS `ClearDayBattleCount`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` EVENT `ClearDayBattleCount` ON SCHEDULE EVERY 1 DAY STARTS '2016-06-01 00:00:00' ON COMPLETION NOT PRESERVE ENABLE DO call ClearDayBattle()
;;
DELIMITER ;
