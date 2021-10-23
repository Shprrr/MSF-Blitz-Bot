DROP TABLE fight_heroes;

CREATE TABLE [fight_heroes] (
  [fight_id] INTEGER NOT NULL
, [side] INTEGER NOT NULL
, [index] INTEGER NOT NULL
, [character_id] TEXT NOT NULL
, [power] INTEGER NOT NULL
, [accuracy] NUMERIC NOT NULL
, CONSTRAINT [PK_fight_heroes] PRIMARY KEY ([fight_id],[side],[index])
, FOREIGN KEY(fight_id) REFERENCES fights(fight_id)
);
