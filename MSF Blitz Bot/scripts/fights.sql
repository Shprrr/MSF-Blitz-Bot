DROP TABLE fights;

CREATE TABLE [fights] (
  [fight_id] INTEGER NOT NULL
, [result] INTEGER NOT NULL
, [date_time] TEXT NOT NULL
, CONSTRAINT [PK_fights] PRIMARY KEY ([fight_id])
);
