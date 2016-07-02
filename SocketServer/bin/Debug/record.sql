/*
Navicat MySQL Data Transfer

Source Server         : local_copy_copy
Source Server Version : 50712
Source Host           : localhost:3306
Source Database       : tank

Target Server Type    : MYSQL
Target Server Version : 50712
File Encoding         : 65001

Date: 2016-06-23 11:08:42
*/

SET FOREIGN_KEY_CHECKS=0;
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
INSERT INTO record VALUES ('123', '10', '8', '0', '0', '0', '0', '2');
INSERT INTO record VALUES ('13', '4', '2', '1', '1', '1', '6', '8');
INSERT INTO record VALUES ('132', '11', '5', '3', '1', '0', '16', '28');
INSERT INTO record VALUES ('133', '7', '1', '0', '0', '0', '1', '18');
INSERT INTO record VALUES ('213', '2', '1', '0', '0', '0', '0', '3');
