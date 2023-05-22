CREATE DATABASE IF NOT EXISTS df_account_db DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT ENCRYPTION='N';
USE df_account_db;

CREATE TABLE IF NOT EXISTS user_accounts
(
	pk_id BIGINT AUTO_INCREMENT PRIMARY KEY,
	user_id VARCHAR(10) UNIQUE NOT NULL,
	salt BINARY(16) NOT NULL,
	hashed_password BINARY(32) NOT NULL
);
