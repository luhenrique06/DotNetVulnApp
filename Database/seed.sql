-- VaultBank seed — laboratório OWASP Top 10 2025
-- Dados fictícios. Senhas em MD5(base64) SEM sal (A04 - Cryptographic Failures).
PRAGMA foreign_keys = OFF;

DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS accounts;
DROP TABLE IF EXISTS infobanking;
DROP TABLE IF EXISTS transfers;
DROP TABLE IF EXISTS cards;
DROP TABLE IF EXISTS webhooks;
DROP TABLE IF EXISTS coupons;
DROP TABLE IF EXISTS todoitems;

-- ------------------------------------------------------------------
-- CLIENTES (tabela `users` mantida para realismo de login/SQLi)
-- ------------------------------------------------------------------
CREATE TABLE users (
    id                 TEXT NOT NULL PRIMARY KEY,
    name               TEXT NOT NULL,
    cpf                TEXT,
    login              TEXT NOT NULL,
    password           TEXT NOT NULL,           -- MD5 base64 sem sal (A04)
    role               TEXT NOT NULL DEFAULT 'customer',  -- customer | support | admin
    mfaSecret          TEXT,
    dailyLimit         NUMERIC(14,2) DEFAULT 1000,        -- A06: existe mas nunca é checado
    resetToken         TEXT,                    -- A04: token de reset previsível
    dateInsert         TEXT,
    dateUpdate         TEXT,
    isAdmin            INTEGER DEFAULT 0,        -- legado usado no JWT
    inativo            INTEGER NOT NULL DEFAULT 0,
    dateChangePassword TEXT
);

-- admin (credencial fraca - A02) | password: admin123
INSERT INTO users VALUES ('4c02fd79-eab4-49d9-b9d8-a30fbb3f0407','Luiz Henrique','111.444.777-35','admin','AZICOnu9cyUFFvBp3xi1AA==','admin','JBSWY3DPEHPK3PXP',1000000,NULL,'2024-01-10 09:00:00',NULL,1,0,NULL);
-- suporte (role privilegiada - alvo de escalonamento) | password: support123
INSERT INTO users VALUES ('7b19b0e2-1c44-4a90-8f0b-2a2f0c9d1e10','Central de Suporte','222.333.444-55','suporte','WILETadPkjuqvoR2xuivNw==','support','KRSXG5BAMFXWG',5000,NULL,'2024-01-10 09:00:00',NULL,0,0,NULL);
-- clientes comuns
INSERT INTO users VALUES ('af27290e-3e91-4f6f-945c-6af304315306','Marcelo Souza','256.987.741-09','marcelo','mVvwU8RpTh41PP1CuU5ERw==','customer',NULL,1000,NULL,'2024-02-01 10:20:00',NULL,0,0,NULL);
-- password: Cliente@123
INSERT INTO users VALUES ('aae06968-2782-49d3-8f55-e67e4606698a','Ana Silva','325.548.456-70','ana.silva','xi5IVfcbP47Cm+TSziFXJw==','customer',NULL,2000,NULL,'2024-02-05 14:00:00',NULL,0,0,NULL);
-- password: senha123
INSERT INTO users VALUES ('89a951d7-56a7-4e06-86ae-5993a6ad5beb','Joao Pereira','659.898.585-88','joao','59gP/u+iErfFxVcA5PcZPg==','customer',NULL,1000,NULL,'2024-03-01 11:00:00',NULL,0,0,NULL);
-- password: teste
INSERT INTO users VALUES ('1f3d2c0a-9e88-4b21-a7d6-0c5b3e4f6a72','Cliente Teste','207.311.983-28','teste','aY3BnUicTk23PiinE+qwew==','customer',NULL,500,NULL,'2024-03-10 08:30:00',NULL,0,0,NULL);

-- ------------------------------------------------------------------
-- CONTAS (evolui de `infobanking`; accountId sequencial = alvo BOLA A01)
-- ------------------------------------------------------------------
CREATE TABLE accounts (
    accountId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    agencia   TEXT,
    conta     TEXT,
    cpf       TEXT,
    ownerId   TEXT,                    -- FK users.id
    saldo     NUMERIC(14,2) DEFAULT 0,
    status    TEXT DEFAULT 'active'
);
INSERT INTO accounts (agencia,conta,cpf,ownerId,saldo,status) VALUES
 ('0001','25245-6','111.444.777-35','4c02fd79-eab4-49d9-b9d8-a30fbb3f0407',10000.00,'active'),
 ('0001','32554-8','256.987.741-09','af27290e-3e91-4f6f-945c-6af304315306',2541.21,'active'),
 ('0001','65989-8','325.548.456-70','aae06968-2782-49d3-8f55-e67e4606698a',0.01,'active'),
 ('0001','65985-4','659.898.585-88','89a951d7-56a7-4e06-86ae-5993a6ad5beb',10.21,'active'),
 ('0001','65898-5','207.311.983-28','1f3d2c0a-9e88-4b21-a7d6-0c5b3e4f6a72',15421451.11,'active');

-- ------------------------------------------------------------------
-- TRANSFERÊNCIAS (A06 race / A10 estado inconsistente)
-- ------------------------------------------------------------------
CREATE TABLE transfers (
    id             INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    fromAccount    INTEGER,
    toAccount      INTEGER,
    amount         NUMERIC(14,2),
    status         TEXT,
    createdAt      TEXT,
    idempotencyKey TEXT
);
INSERT INTO transfers (fromAccount,toAccount,amount,status,createdAt,idempotencyKey) VALUES
 (2,4,50.00,'completed','2024-04-01 12:00:00','a1b2c3'),
 (5,2,1000.00,'completed','2024-04-02 09:30:00','d4e5f6');

-- ------------------------------------------------------------------
-- CARTÕES (A04: PAN/CVV em texto claro)
-- ------------------------------------------------------------------
CREATE TABLE cards (
    id        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    ownerId   TEXT,
    pan       TEXT,     -- em claro (A04)
    cvv       TEXT,     -- em claro (A04)
    expiry    TEXT,
    cardLimit NUMERIC(14,2)
);
INSERT INTO cards (ownerId,pan,cvv,expiry,cardLimit) VALUES
 ('af27290e-3e91-4f6f-945c-6af304315306','4111111111111111','123','12/27',3000.00),
 ('aae06968-2782-49d3-8f55-e67e4606698a','5500005555555559','456','08/26',8000.00),
 ('1f3d2c0a-9e88-4b21-a7d6-0c5b3e4f6a72','4000001234567899','789','01/28',20000.00);

-- ------------------------------------------------------------------
-- WEBHOOKS (A08: callback sem verificação de assinatura / A-SSRF)
-- ------------------------------------------------------------------
CREATE TABLE webhooks (
    id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    ownerId     TEXT,
    callbackUrl TEXT,
    secret      TEXT
);
INSERT INTO webhooks (ownerId,callbackUrl,secret) VALUES
 ('af27290e-3e91-4f6f-945c-6af304315306','https://webhook.site/marcelo','shared-secret-123');

-- ------------------------------------------------------------------
-- CUPONS (A06: reuso por falta de atomicidade)
-- ------------------------------------------------------------------
CREATE TABLE coupons (
    code     TEXT NOT NULL PRIMARY KEY,
    valuePct INTEGER,
    usesLeft INTEGER
);
INSERT INTO coupons VALUES ('CASHBACK50',50,1),('BEMVINDO10',10,100);

PRAGMA foreign_keys = ON;
