/*
Navicat MySQL Data Transfer

Source Server         : local_copy_copy
Source Server Version : 50712
Source Host           : localhost:3306
Source Database       : zczb_log_android_cn_s999

Target Server Type    : MYSQL
Target Server Version : 50712
File Encoding         : 65001

Date: 2016-06-01 16:44:40
*/

SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for `dict_value_type`
-- ----------------------------
DROP TABLE IF EXISTS `dict_value_type`;
CREATE TABLE `dict_value_type` (
  `value_type_id` tinyint(4) DEFAULT NULL,
  `value_type_name` varchar(255) DEFAULT NULL,
  `oss_show` tinyint(4) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of dict_value_type
-- ----------------------------
