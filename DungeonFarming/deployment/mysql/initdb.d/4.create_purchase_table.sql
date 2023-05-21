CREATE DATABASE df_purchase_history_db;
USE df_purchase_history_db;

CREATE TABLE IF NOT EXISTS purchase_histories (
  `purchase_id` BIGINT PRIMARY KEY NOT NULL AUTO_INCREMENT COMMENT '구매 내역 id',
  `user_id` BIGINT NOT NULL COMMENT '구매한 유저의 id',
  `package_code` SMALLINT NOT NULL COMMENT '패키지 id' ,
  `purchase_token` TEXT NOT NULL COMMENT 'purchase token',
  `purchase_date` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);