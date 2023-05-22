CREATE DATABASE IF NOT EXISTS df_game_db DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT ENCRYPTION='N';
USE df_game_db;

INSERT into mt_item_defines (item_code , item_name , attribute, sell, buy, use_lv, attack, defence, magic, enhance_max_count , max_stack)
VALUES 
(0, '돈', 5, 0, 0, 0, 0, 0, 0, 0, 1000000000),
(1, '작은 칼', 1, 10, 20, 1, 10, 5, 1, 10, 1),
(2, '도금 칼', 1, 100, 200, 5, 29, 12, 10, 10, 1),
(3, '나무 방패', 2, 7, 15, 1, 3, 10, 1, 10, 1),
(4, '보통 모자', 3, 5, 8, 1, 1, 1, 1, 10, 1),
(5, '포션', 4, 3, 6, 1, 0, 0, 0, 0, 100);

INSERT INTO mt_default_items_list VALUE (0, 0, 100000, 1, 1, 3, 1, 5, 10);

INSERT INTO mt_item_attributes
VALUES (1, '무기'), (2, '방어구'), (3, '복장'), (4, '마법도구'), (5, '돈');

INSERT INTO mt_daily_login_rewards
VALUES 
(1, 1, 100), (2, 1, 100), (3, 1, 100),
(4, 1, 200), (5, 1, 200), (6, 1, 200),
(7, 2, 1), (8, 1, 100), (9, 1, 100),
(10, 1, 100), (11, 6, 5), (12, 1, 150),
(13, 1, 150), (14, 1, 150), (15, 1, 150),
(16, 1, 150), (17, 1, 150), (18, 4, 1),
(19, 1, 200), (20, 1, 200), (21, 1, 200),
(22, 1, 200), (23, 1, 200), (24, 5, 1),
(25, 1, 250), (26, 1, 250), (27, 1, 250),
(28, 1, 250), (29, 1, 250), (30, 3, 1);

INSERT INTO mt_package
VALUES
(1, 1, 1000), (1, 2, 1), (1, 3, 1),
(2, 4, 1), (2, 5, 1), (2, 6, 10),
(3, 1, 2000), (3, 2, 1), (3, 3, 1),
(3, 5, 1);

INSERT mt_user_level_exps 
VALUES (1, 200), (2, 500), (3, 1000),(4, 2000), (5, 3800);

INSERT mt_stage_infos
VALUES (1, "첫번째 던전", 1), (2, "두번째 던전", 3);

INSERT mt_stage_items (stage_code, item_code)
VALUES (1, 1), (1, 2), (2, 3), (2, 3);

INSERT mt_stage_npcs (stage_code, npc_code, npc_count, exp_per_npc)
VALUES 
(1, 101, 10, 10), (1, 110, 12, 15), (2, 201, 40, 20),
(2, 211, 20, 35), (2, 221, 1, 50);