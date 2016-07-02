/*
Navicat MySQL Data Transfer

Source Server         : local_copy_copy
Source Server Version : 50712
Source Host           : localhost:3306
Source Database       : tank

Target Server Type    : MYSQL
Target Server Version : 50712
File Encoding         : 65001

Date: 2016-06-03 17:14:59
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
  `uname` char(30) DEFAULT '',
  UNIQUE KEY `PidUid` (`pid`,`uid`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of login
-- ----------------------------
INSERT INTO login VALUES ('0', 'liyonghelpme', '唐朝妖精之');
