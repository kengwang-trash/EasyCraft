/*
 Navicat Premium Data Transfer

 Source Server         : db
 Source Server Type    : SQLite
 Source Server Version : 3021000
 Source Schema         : main

 Target Server Type    : SQLite
 Target Server Version : 3021000
 File Encoding         : 65001

 Date: 30/07/2020 09:34:45
*/

PRAGMA foreign_keys = false;

-- ----------------------------
-- Table structure for schedule
-- ----------------------------
DROP TABLE IF EXISTS "schedule";
CREATE TABLE "schedule" (
  "sid" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "server" INTEGER NOT NULL,
  "type" INTEGER NOT NULL,
  "param" TEXT,
  "time" TEXT,
  CONSTRAINT "fk_schedule_server_1" FOREIGN KEY ("server") REFERENCES "server" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION
);

-- ----------------------------
-- Table structure for server
-- ----------------------------
DROP TABLE IF EXISTS "server";
CREATE TABLE "server" (
  "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "name" TEXT,
  "owner" INTEGER,
  "port" INTEGER,
  "core" TEXT,
  "maxplayer" INTEGER,
  "ram" INTEGER,
  "world" TEXT,
  "expiretime" TIMESTAMP,
  CONSTRAINT "fk_server_user_1" FOREIGN KEY ("owner") REFERENCES "user" ("uid") ON DELETE NO ACTION ON UPDATE NO ACTION
);

-- ----------------------------
-- Table structure for sqlite_sequence
-- ----------------------------
DROP TABLE IF EXISTS "sqlite_sequence";
CREATE TABLE "sqlite_sequence" (
  "name",
  "seq"
);

-- ----------------------------
-- Table structure for user
-- ----------------------------
DROP TABLE IF EXISTS "user";
CREATE TABLE "user" (
  "uid" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "username" TEXT NOT NULL,
  "password" TEXT NOT NULL,
  "email" TEXT,
  "auth" TEXT
);

-- ----------------------------
-- Table structure for version
-- ----------------------------
DROP TABLE IF EXISTS "version";
CREATE TABLE "version" (
  "version" integer
);

-- ----------------------------
-- Records of version
-- ----------------------------
INSERT INTO "version" VALUES (1);

PRAGMA foreign_keys = true;
