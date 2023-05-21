CREATE USER 'gyeon'@'%' IDENTIFIED BY '1q2w3e4r';
GRANT ALL PRIVILEGES ON df_account_db.* TO 'gyeon'@'%';
GRANT ALL PRIVILEGES ON df_game_db.* TO 'gyeon'@'%';
GRANT ALL PRIVILEGES ON df_purchase_history_db.* TO 'gyeon'@'%';
FLUSH PRIVILEGES;