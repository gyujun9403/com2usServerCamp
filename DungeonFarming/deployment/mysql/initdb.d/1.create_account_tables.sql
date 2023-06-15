CREATE DATABASE IF NOT EXISTS df_account_db DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT ENCRYPTION='N';
USE df_account_db;

CREATE TABLE IF NOT EXISTS user_accounts
(
	user_id BIGINT AUTO_INCREMENT COMMENT 'db상 유저 아이디',
	user_assigned_id VARCHAR(10) NOT NULL COMMENT '유저가 사용할 아이디',
	salt BINARY(16) NOT NULL COMMENT '유저에게 발급된 salt값',
	hashed_password BINARY(32) NOT NULL COMMENT '해싱된 비밀번호',
	PRIMARY KEY (user_id),
	UNIQUE (user_assigned_id)
);
