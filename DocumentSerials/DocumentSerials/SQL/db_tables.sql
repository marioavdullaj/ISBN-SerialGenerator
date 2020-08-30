DROP TABLE `duration`;
DROP TABLE `country`;
DROP TABLE `book`;

CREATE TABLE `duration` (
  `id` int(11) NOT NULL,
  `description` varchar(250) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;



CREATE TABLE `country` (
  `iso` int(11) NOT NULL,
  `code` varchar(250) NOT NULL,
  `name` varchar(250) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE `book` (
  `id` int(11) NOT NULL,
  `title` varchar(250) NOT NULL,
  `description` text
) ENGINE=InnoDB DEFAULT CHARSET=latin1;