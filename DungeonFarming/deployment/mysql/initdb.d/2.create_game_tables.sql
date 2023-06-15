CREATE DATABASE IF NOT EXISTS df_game_db DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT ENCRYPTION='N';
USE df_game_db;

CREATE TABLE IF NOT EXISTS login_log
(
	user_id BIGINT NOT NULL COMMENT '유저의 고유 넘버, Account db의 값과 일치',
	consecutive_login_count SMALLINT NOT NULL DEFAULT 0 COMMENT '연속접속 횟수',
	last_login_date DATETIME COMMENT '마지막 접속',
	PRIMARY KEY (user_id)
);

CREATE TABLE IF NOT EXISTS user_items
(
	item_id BIGINT NOT NULL AUTO_INCREMENT COMMENT '아이템 코드',
	user_id BIGINT NOT NULL COMMENT '소유자',
	item_code INT NOT NULL COMMENT '아이템 코드',
	item_count BIGINT NOT NULL COMMENT '아이템 개수',
	attack INT NOT NULL COMMENT '데미지',
	defence INT NOT NULL COMMENT '방어력',
	magic INT NOT NULL COMMENT '주문력',
	enhance_count SMALLINT NOT NULL COMMENT '강화 횟수',
	PRIMARY KEY (item_id)
);

CREATE TABLE IF NOT EXISTS mailbox
(
	mail_id BIGINT NOT NULL AUTO_INCREMENT COMMENT '우편함 아이디',
	user_id BIGINT NOT NULL COMMENT '유저 아이디',
	item0_code INT DEFAULT -1 COMMENT '0번 아이템 종류',
	item0_count INT DEFAULT -1 COMMENT '0번 아이템 개수',
	item1_code INT DEFAULT -1 COMMENT '1번 아이템 코드',
	item1_count INT DEFAULT -1 COMMENT '1번 아이템 개수',
	item2_code INT DEFAULT -1 COMMENT '2번 아이템 코드',
	item2_count INT DEFAULT -1 COMMENT '2번 아이템 개수',
	item3_code INT DEFAULT -1 COMMENT '3번 아이템 코드',
	item3_count INT DEFAULT -1 COMMENT '3번 아이템 개수',
	mail_title TEXT NOT NULL COMMENT '우편 타이틀',
	mail_text TEXT COMMENT '메세지',
	read_date DATETIME NOT NULL DEFAULT '9999-12-31 23:59:59' COMMENT '읽은 날짜',
	recieve_date DATETIME NOT NULL COMMENT '수령일',
	expiration_date DATETIME NOT NULL DEFAULT '9999-12-31 23:59:59' COMMENT '만료일',
	is_deleted TINYINT NOT NULL DEFAULT 0 COMMENT '삭제여부',
	PRIMARY KEY (mail_id)
);

CREATE TABLE IF NOT EXISTS user_achievement
(
	user_id BIGINT NOT NULL COMMENT '유저 아이디',
	user_level INT NOT NULL DEFAULT 1 COMMENT '유저 레벨',
	user_exp BIGINT NOT NULL DEFAULT 0 COMMENT '유저가 가진 경험치',
	highest_cleared_stage_id INT NOT NULL DEFAULT 0 COMMENT '유저가 최대로 클리어한 던전',
	PRIMARY KEY (user_id)
);