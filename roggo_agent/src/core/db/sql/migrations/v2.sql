-- 2.1: add playlist attribute to matches table
alter table matches
add column playlist integer not null default 0;

