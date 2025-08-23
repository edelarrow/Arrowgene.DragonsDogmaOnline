CREATE TABLE "ddon_dispel_seals"
(
    "character_id"              INTEGER NOT NULL,
    "seal_index"                INTEGER NOT NULL,
    CONSTRAINT "pk_ddon_dispel_seals" PRIMARY KEY ("character_id", "seal_index"),
    CONSTRAINT "fk_ddon_dispel_seals_character_id" FOREIGN KEY ("character_id") references "ddon_character" ("character_id") ON DELETE CASCADE
)
