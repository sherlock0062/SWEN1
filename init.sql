DROP TABLE IF EXISTS battles CASCADE;
DROP TABLE IF EXISTS deck_cards CASCADE;
DROP TABLE IF EXISTS package_cards CASCADE;
DROP TABLE IF EXISTS packages CASCADE;
DROP TABLE IF EXISTS cards CASCADE;
DROP TABLE IF EXISTS users CASCADE;


CREATE TABLE users (
                       username      VARCHAR(50) PRIMARY KEY,
                       password      VARCHAR(255) NOT NULL,
                       name          VARCHAR(100),
                       bio           TEXT,
                       image         TEXT,
                       coins         INTEGER DEFAULT 20,
                       elo           INTEGER DEFAULT 100,
                       wins          INTEGER DEFAULT 0,
                       losses        INTEGER DEFAULT 0,
                       token         VARCHAR(255)
);

CREATE TABLE cards (
                       cardid       UUID PRIMARY KEY,
                       name         VARCHAR(50) NOT NULL,
                       damage       DECIMAL(10,1) NOT NULL,
                       elementtype  VARCHAR(10) NOT NULL,
                       cardtype     VARCHAR(10) NOT NULL,
                       ownerid      VARCHAR(50) REFERENCES users(username) ON DELETE SET NULL
);

CREATE TABLE packages (
                          id          SERIAL PRIMARY KEY,
                          isacquired  BOOLEAN DEFAULT FALSE
);

CREATE TABLE package_cards (
                               package_id  INTEGER REFERENCES packages(id) ON DELETE CASCADE,
                               cardid      UUID    REFERENCES cards(cardid) ON DELETE CASCADE,
                               PRIMARY KEY (package_id, cardid)
);

CREATE TABLE deck_cards (
                            username  VARCHAR(50) REFERENCES users(username) ON DELETE CASCADE,
                            cardid    UUID        REFERENCES cards(cardid) ON DELETE CASCADE,
                            PRIMARY KEY (username, cardid)
);

CREATE TABLE battles (
                         battleid    SERIAL       PRIMARY KEY,
                         user1id     VARCHAR(50)  REFERENCES users(username) ON DELETE SET NULL,
                         user2id     VARCHAR(50)  REFERENCES users(username) ON DELETE SET NULL,
                         winnerid    VARCHAR(50)  REFERENCES users(username) ON DELETE SET NULL,
                         battlelog   TEXT,
                         timestamp   TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO users (username, password, name, token, coins)
VALUES ('admin', 'istrator', 'Administrator', 'admin-mtcgToken', 100);
