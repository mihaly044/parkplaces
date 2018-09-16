SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for cities
-- ----------------------------
DROP TABLE IF EXISTS `cities`;
CREATE TABLE `cities` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `city` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of cities
-- ----------------------------
INSERT INTO `cities` VALUES ('1', 'Szeged');
INSERT INTO `cities` VALUES ('2', 'Budapest');

-- ----------------------------
-- Table structure for geometry
-- ----------------------------
DROP TABLE IF EXISTS `geometry`;
CREATE TABLE `geometry` (
  `zoneid` int(11) NOT NULL,
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `lat` double NOT NULL,
  `lng` double NOT NULL,
  `polyindex` int(255) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of geometry
-- ----------------------------
INSERT INTO `geometry` VALUES ('1', '1', '47.49540043878657', '19.037632942199707', '0');
INSERT INTO `geometry` VALUES ('1', '2', '47.50201104821246', '19.041152000427246', '1');
INSERT INTO `geometry` VALUES ('1', '3', '47.49783692937882', '19.041036665439606', '4');
INSERT INTO `geometry` VALUES ('1', '4', '47.49653125993388', '19.042611122131348', '2');
INSERT INTO `geometry` VALUES ('1', '5', '47.497995492159504', '19.04108762741089', '3');

-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `username` varchar(20) NOT NULL,
  `password` varchar(255) NOT NULL,
  `groupid` int(11) NOT NULL,
  `creatorid` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of users
-- ----------------------------
INSERT INTO `users` VALUES ('1', 'admin', '$2a$06$4K7V4/gHrrpl0oL/E.xCnOepsfTle9VdxBmUEZbijFZOpehMr9WAq', '4', '0');

-- ----------------------------
-- Table structure for zones
-- ----------------------------
DROP TABLE IF EXISTS `zones`;
CREATE TABLE `zones` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `cityid` int(11) NOT NULL,
  `color` varchar(255) NOT NULL,
  `fee` int(11) NOT NULL,
  `service_na` varchar(255) NOT NULL,
  `description` varchar(255) NOT NULL,
  `timetable` varchar(255) DEFAULT NULL,
  `common_name` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of zones
-- ----------------------------
INSERT INTO `zones` VALUES ('1', '2', '#FF8000', '0', 'Test', 'New zone', '1', '1');
