SET FOREIGN_KEY_CHECKS=0;
SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

CREATE DATABASE IF NOT EXISTS `parkplaces` DEFAULT CHARACTER SET latin1 COLLATE latin1_swedish_ci;
USE `parkplaces`;

CREATE TABLE IF NOT EXISTS `cities` (
`id` int(11) NOT NULL,
  `city` varchar(255) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;

INSERT INTO `cities` (`id`, `city`) VALUES
(1, 'Szeged');

CREATE TABLE IF NOT EXISTS `geometry` (
  `zoneid` int(11) NOT NULL,
`id` int(11) NOT NULL,
  `lat` double NOT NULL,
  `lng` double NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=latin1;

INSERT INTO `geometry` (`zoneid`, `id`, `lat`, `lng`) VALUES
(1, 1, 46.255846818480315, 20.14090061187744),
(1, 2, 46.25658865484284, 20.1414155960083),
(1, 3, 46.25390317914044, 20.150363445281982),
(1, 4, 46.25290907743219, 20.149741172790527),
(2, 5, 46.25115822762373, 20.15214443206787),
(2, 6, 46.25136595848944, 20.153732299804688),
(2, 7, 46.24961505941882, 20.15690803527832),
(3, 8, 46.25718210329677, 20.141758918762207),
(3, 9, 46.257508501306454, 20.142316818237305),
(3, 10, 46.25557975761527, 20.146865844726562),
(3, 11, 46.25522367443872, 20.146565437316895);

CREATE TABLE IF NOT EXISTS `users` (
`id` int(11) NOT NULL,
  `username` varchar(20) NOT NULL,
  `password` varchar(255) NOT NULL,
  `groupid` int(11) NOT NULL,
  `creatorid` int(11) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;

INSERT INTO `users` (`id`, `username`, `password`, `groupid`, `creatorid`) VALUES
(1, 'admin', '$2y$10$naypQWIa7gb7H8QLIUWa9.I8K3J3fh0SIp3AdOmnYpApBpnrC/KjG', 4, 0),
(3, 'asd', '$2a$06$m62IRO/Y7ka/XReIxprXNuvHqziBzDarlRKXbncJNx8J3MP2H8LFe', 4, 1),
(4, 'felhasznalo', '$2a$06$AGHGkstJ/.qyMFlNX1G9ZeGqQQwPulqFCnzOzrOnS3iQ17EX16dVS', 3, 0);

CREATE TABLE IF NOT EXISTS `zones` (
`id` int(11) NOT NULL,
  `cityid` int(11) NOT NULL,
  `color` varchar(255) NOT NULL,
  `fee` int(11) NOT NULL,
  `service_na` varchar(255) NOT NULL,
  `description` varchar(255) NOT NULL,
  `timetable` varchar(255) DEFAULT NULL,
  `common_name` varchar(255) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=latin1;

INSERT INTO `zones` (`id`, `cityid`, `color`, `fee`, `service_na`, `description`, `timetable`, `common_name`) VALUES
(1, 1, 'Red', 700, 'Szeged megyei jogú város önkormányzata', 'New zone', '06:00-18:00', 'SZEGED-1'),
(2, 1, 'Yellow', 220, 'Szeged megyei jogú város önkormányzata', 'New zone', '06:00-18:00', 'SZEGED-2'),
(3, 1, 'Olive', 500, 'Szeged megyei jogú város önkormányzata', 'New zone', '06:00-18:00', 'SZEGED-3');


ALTER TABLE `cities`
 ADD PRIMARY KEY (`id`), ADD KEY `id` (`id`);

ALTER TABLE `geometry`
 ADD PRIMARY KEY (`id`);

ALTER TABLE `users`
 ADD PRIMARY KEY (`id`);

ALTER TABLE `zones`
 ADD PRIMARY KEY (`id`), ADD KEY `cityid` (`cityid`);


ALTER TABLE `cities`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=2;
ALTER TABLE `geometry`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=12;
ALTER TABLE `users`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=5;
ALTER TABLE `zones`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=4;SET FOREIGN_KEY_CHECKS=1;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
