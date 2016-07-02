/*
Navicat MySQL Data Transfer

Source Server         : local_copy_copy
Source Server Version : 50712
Source Host           : localhost:3306
Source Database       : tank

Target Server Type    : MYSQL
Target Server Version : 50712
File Encoding         : 65001

Date: 2016-06-03 16:59:47
*/

SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for `login`
-- ----------------------------
DROP TABLE IF EXISTS `login`;
CREATE TABLE `login` (
  `pid` varchar(255) NOT NULL,
  `uid` varchar(255) NOT NULL,
  `uname` char(30) DEFAULT '',
  UNIQUE KEY `PidUid` (`pid`,`uid`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of login
-- ----------------------------
INSERT INTO login VALUES ('0', 'liyonghelpme', '唐朝妖精之');
