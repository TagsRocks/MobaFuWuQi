/*
Navicat MySQL Data Transfer

Source Server         : local_copy_copy
Source Server Version : 50712
Source Host           : localhost:3306
Source Database       : tank

Target Server Type    : MYSQL
Target Server Version : 50712
File Encoding         : 65001

Date: 2016-06-23 14:44:07
*/

SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for `mail`
-- ----------------------------
DROP TABLE IF EXISTS `mail`;
CREATE TABLE `mail` (
  `mailid` int(11) NOT NULL AUTO_INCREMENT,
  `pid` varchar(255) NOT NULL,
  `uid` varchar(255) NOT NULL,
  `title` varchar(255) NOT NULL,
  `sender` varchar(255) NOT NULL,
  `sendtime` int(11) NOT NULL,
  `state` tinyint(4) NOT NULL,
  `content` varchar(4096) NOT NULL DEFAULT '',
  PRIMARY KEY (`mailid`),
  KEY `PidUid` (`pid`,`uid`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of mail
-- ----------------------------
