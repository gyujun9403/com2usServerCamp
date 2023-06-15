CREATE DATABASE IF NOT EXISTS df_game_db DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT ENCRYPTION='N';
USE df_game_db;

CREATE TABLE IF NOT EXISTS mt_item_defines
(
    item_code INT NOT NULL PRIMARY KEY COMMENT '해당 아이템의 코드',
    item_name VARCHAR(50) NOT NULL UNIQUE COMMENT '해당 아이템 명칭',
    attribute SMALLINT NOT NULL COMMENT '해당 아이템의 속성',
    sell BIGINT NOT NULL COMMENT '판매 가격',
    buy BIGINT NOT NULL COMMENT '구매 가격',
    use_lv SMALLINT NOT NULL COMMENT '사용 가능 레벨',
    attack INT NOT NULL COMMENT '데미지',
    defence INT NOT NULL COMMENT '방어력',
    magic INT NOT NULL COMMENT '주문력',
    enhance_max_count SMALLINT NOT NULL COMMENT '최대 강화 횟수',
    max_stack INT NOT NULL DEFAULT 1 COMMENT '한 공간당 최대 소유 개수'
);

CREATE TABLE IF NOT EXISTS mt_default_items_list
(
	list_id SMALLINT NOT NULL PRIMARY KEY COMMENT '리스트의 코드',
	item0_code INT NOT NULL DEFAULT -1 COMMENT '기본 지급 아이템1 종류',
	item0_count INT NOT NULL DEFAULT 0 COMMENT '기본 지급 아이템1 개수',
	item1_code INT NOT NULL DEFAULT -1 COMMENT '기본 지급 아이템2 개수',
	item1_count INT NOT NULL DEFAULT 0 COMMENT '기본 지급 아이템2 종류',
	item2_code INT NOT NULL DEFAULT -1 COMMENT '기본 지급 아이템3 개수',
	item2_count INT NOT NULL DEFAULT 0 COMMENT '기본 지급 아이템3 종류',
	item3_code INT NOT NULL DEFAULT -1 COMMENT '기본 지급 아이템4 개수',
	item3_count INT NOT NULL DEFAULT 0 COMMENT '기본 지급 아이템4 종류'
);

CREATE TABLE IF NOT EXISTS mt_item_attributes
(
	attribute SMALLINT AUTO_INCREMENT PRIMARY KEY COMMENT '종류 코드',
	item_name VARCHAR(50) NOT NULL UNIQUE COMMENT '종류 명'
);

CREATE TABLE IF NOT EXISTS mt_daily_login_rewards
(
	day_count INT PRIMARY KEY COMMENT '연속 접속 일자',
	item_code INT NOT NULL COMMENT '지급 아이템 코드',
	item_count INT NOT NULL COMMENT '지급 아이템 개수'
);

CREATE TABLE IF NOT EXISTS mt_package
(
	package_code INT COMMENT '상품 코드, 중복 가능한 값',
	item_code INT NOT NULL COMMENT '지급 아이템 코드',
	item_count INT NOT NULL COMMENT '지급 아이템 개수'
);

CREATE TABLE IF NOT EXISTS mt_user_level_exps
(
	user_level INT PRIMARY KEY NOT NULL COMMENT '유저의 레밸',
	max_exp BIGINT NOT NULL COMMENT '해당 레벨의 최대 경험치량'
);

CREATE TABLE IF NOT EXISTS mt_stage_infos
(
	stage_code INT PRIMARY KEY NOT NULL COMMENT '던전 아이디',
	stage_name VARCHAR(20) NOT NULL COMMENT '유저 표시 던전명',
	required_user_level INT NOT NULL COMMENT '던전 입장 가능 유저 레벨'
);

CREATE TABLE IF NOT EXISTS mt_stage_npcs
(
	pk_id INT PRIMARY KEY NOT NULL AUTO_INCREMENT COMMENT 'pkid',
	stage_code INT NOT NULL COMMENT '던전 코드',
	npc_code INT NOT NULL COMMENT 'npc 코드',
	npc_count INT NOT NULL COMMENT 'npc 숫자',
	exp_per_npc INT NOT NULL COMMENT 'npc 하나가 주는 경험치'
);

CREATE TABLE IF NOT EXISTS mt_stage_items
(
	pk_id INT PRIMARY KEY NOT NULL AUTO_INCREMENT COMMENT 'pkid',  
	stage_code INT NOT NULL COMMENT '던전 아이디',
	item_code INT NOT NULL COMMENT '던전 제공 아이템'  
);