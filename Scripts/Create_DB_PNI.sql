
--создание таблиц-----
--Таблица хранения физических процессов--
	CREATE TABLE main_block."Physical_process"
(
    "id_Ph.p." serial NOT NULL,
    name character varying(100) NOT NULL,
    PRIMARY KEY ("id_Ph.p.")
);

ALTER TABLE main_block."Physical_process"
    OWNER to postgres;
	
--Таблица хранения типов энергетического оборудования--	
	CREATE TABLE main_block."Type_of_power_equipment"
(
    "id_T.p.e." serial NOT NULL,
    name character varying(100) NOT NULL,
    PRIMARY KEY ("id_T.p.e.")
);

ALTER TABLE main_block."Type_of_power_equipment"
    OWNER to postgres;
	
--Связующая таблица--	
	CREATE TABLE main_block."Ph.p._T.p.e."
(
    "id_Ph.p." integer NOT NULL,
    "id_T.p.e." integer NOT NULL,
    "id_Ph.p._T.p.e." serial NOT NULL,
    PRIMARY KEY ("id_Ph.p._T.p.e."),
    FOREIGN KEY ("id_Ph.p.")
        REFERENCES main_block."Physical_process" ("id_Ph.p.") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    FOREIGN KEY ("id_T.p.e.")
        REFERENCES main_block."Type_of_power_equipment" ("id_T.p.e.") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

ALTER TABLE main_block."Ph.p._T.p.e."
    OWNER to postgres;

--Таблица хранения названий рассматриваемых областей--
	CREATE TABLE main_block."Area"
(
    id_area serial NOT NULL,
    name_area character varying(100) NOT NULL,
    PRIMARY KEY (id_area)
);

ALTER TABLE main_block."Area"
    OWNER to postgres;


 
--Таблица хранения названий экспериментальных объектов и их элементов--	
	CREATE TABLE main_block."Areas_tree"
(
    id_area integer NOT NULL,
    "id_subarea" serial NOT NULL,
    name_subarea character varying(100) NOT NULL,
    id_parent_area integer,
    PRIMARY KEY ("id_subarea"),
    FOREIGN KEY (id_area)
        REFERENCES main_block."Area" (id_area) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    FOREIGN KEY (id_parent_area)
		REFERENCES main_block."Areas_tree" (id_subarea) MATCH SIMPLE
		ON UPDATE CASCADE
		ON DELETE CASCADE
);

ALTER TABLE main_block."Areas_tree"
    OWNER to postgres;
	


--Таблица хранения номера класса задачи--
CREATE TABLE main_block."Experiment_class"
(
    "id_Ph.p._T.p.e." integer NOT NULL,
    id_subarea integer NOT NULL,
    "ID*" serial NOT NULL,
	"Main_pict" character varying(100),
	"Geom_pict" character varying(100),
	"Reg_pict" character varying(100),
	"Tepl_pict" character varying(100),
    PRIMARY KEY ("ID*"),
    FOREIGN KEY ("id_Ph.p._T.p.e.")
        REFERENCES main_block."Ph.p._T.p.e." ("id_Ph.p._T.p.e.") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    FOREIGN KEY (id_subarea)
        REFERENCES main_block."Areas_tree" (id_subarea) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

ALTER TABLE main_block."Experiment_class"
    OWNER to postgres;
---
--Таблица хранения типов параметров--
	CREATE TABLE main_block."Type_of_parametrs"
(
    id_type serial NOT NULL,
    name character varying(100) NOT NULL,
    PRIMARY KEY (id_type)
);

ALTER TABLE main_block."Type_of_parametrs"
    OWNER to postgres;

--Таблица хранения типов данных параметров--
	CREATE TABLE main_block."Data_type"
(
    id_data serial NOT NULL,
    name_data character varying(50) NOT NULL,
    PRIMARY KEY (id_data)
);

ALTER TABLE main_block."Data_type"
    OWNER to postgres;

--Таблица хранения информации о параметрах--
	CREATE TABLE main_block."Parametrs"
(
    id_type integer NOT NULL,
    id_param serial NOT NULL,
    name_param character varying(100) NOT NULL,
    short_name_param character varying(100),
    unit_param character varying(100),
    PRIMARY KEY (id_param),
    FOREIGN KEY (id_type)
        REFERENCES main_block."Type_of_parametrs" (id_type) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

ALTER TABLE main_block."Parametrs"
    OWNER to postgres;

--Таблица хранения возможных значений строковых параметров--
	CREATE TABLE main_block."String_values"
(
    id_param integer NOT NULL,
    value character varying(100) NOT NULL,
	PRIMARY KEY (id_param,value),
    FOREIGN KEY (id_param)
        REFERENCES main_block."Parametrs" (id_param) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

ALTER TABLE main_block."String_values"
    OWNER to postgres;
----	

--Таблица хранения действий пользователей--
CREATE TABLE main_block."Log_table"
(
    "user" character varying(100) COLLATE pg_catalog."default" NOT NULL,
    action character varying(100) COLLATE pg_catalog."default" NOT NULL,
    date timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "Log_table_pkey" PRIMARY KEY (action)
);

--Таблица с информацией о стендах--
CREATE TABLE main_block."Stands"
(
    "Stand_id" serial NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    scheme character varying(100),
    "3d_model" character varying(100),
    "P&M" character varying(100),
	PRIMARY KEY ("Stand_id")
);

ALTER TABLE main_block."Stands"
    OWNER to postgres;
------------------------------------------------------------------
--Таблица формирующая связь между классом задачи и стендом--
CREATE TABLE main_block."Stand_ID*"
(
    "Id$" serial NOT NULL,
    "ID*" integer NOT NULL,
    "Stand_id" integer NOT NULL,
    PRIMARY KEY ("Id$"),
    FOREIGN KEY ("ID*")
        REFERENCES main_block."Experiment_class" ("ID*") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID,
	 FOREIGN KEY ("Stand_id")
        REFERENCES  main_block."Stands" ("Stand_id") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE main_block."Stand_ID*"
    OWNER to postgres;
------------------------------------------------------------------
--Таблица с названиями файлов дополнительной документации стендов--
CREATE TABLE main_block."Stand_documentation"
(
    "Stand_id" integer,
    "Documentation_name" character varying(100),
    PRIMARY KEY ("Stand_id", "Documentation_name"),
    FOREIGN KEY ("Stand_id")
        REFERENCES main_block."Stands" ("Stand_id") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE main_block."Stand_documentation"
    OWNER to postgres;
------------------------------------------------------------------	
--Таблица номерами исполнений и каналов-- 
CREATE TABLE main_block."Realization_channel"
(
    "Id$" integer NOT NULL,
    "Realization" integer NOT NULL,
    "Channel" integer NOT NULL,
    "Id_R_C" serial NOT NULL,
    PRIMARY KEY ("Id_R_C"),
    FOREIGN KEY ("Id$")
        REFERENCES main_block."Stand_ID*" ("Id$") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE main_block."Realization_channel"
    OWNER to postgres;
------------------------------------------------------------------
--Таблица со значениями геометрических параметров--
CREATE TABLE main_block."Geometric_parametrs"
(
    "Id_R_C" integer NOT NULL,
    id_param integer NOT NULL,
    value_number numeric(14, 7) NOT NULL,
    PRIMARY KEY ("Id_R_C", id_param),
    FOREIGN KEY (id_param)
        REFERENCES main_block."Parametrs" (id_param) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID,
	FOREIGN KEY ("Id_R_C")
        REFERENCES main_block."Realization_channel" ("Id_R_C") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

ALTER TABLE main_block."Geometric_parametrs"
    OWNER to postgres;
------------------------------------------------------------------
--таблица графического матерала для иллюстрации геометрических параметров--
CREATE TABLE main_block."Graphic_material"
(
    "Id_R_C" integer NOT NULL,
    "Graphic_name" character varying(100) NOT NULL,
    PRIMARY KEY ("Id_R_C", "Graphic_name"),
    FOREIGN KEY ("Id_R_C")
        REFERENCES main_block."Realization_channel" ("Id_R_C") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE main_block."Graphic_material"
    OWNER to postgres;

------------------------------------------------------------------
CREATE TABLE main_block."Mode"
(
    "Id_R_C" integer NOT NULL,
    "Id_mode" integer NOT NULL,
    "Id_rcm" serial NOT NULL,
    PRIMARY KEY ("Id_rcm"),
    FOREIGN KEY ("Id_R_C")
        REFERENCES main_block."Realization_channel" ("Id_R_C") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE main_block."Mode"
    OWNER to postgres;
	
---------------------------------------------------------------------
CREATE TABLE main_block."Cros_section"
(
    "Id_rcm" integer NOT NULL,
    "id_cros_section" integer NOT NULL,
    "Id_m_c" serial NOT NULL,
    PRIMARY KEY ("Id_m_c"),
    FOREIGN KEY ("Id_rcm")
        REFERENCES main_block."Mode" ("Id_rcm") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE main_block."Cros_section"
    OWNER to postgres;
---------------------------------------------------------------------
CREATE OR REPLACE VIEW main_block."Mode_cros_section"
 AS
select "Id_R_C", "Id_mode", id_cros_section, "Id_m_c" from main_block."Mode" m join main_block."Cros_section" c on m."Id_rcm" = c."Id_rcm";

ALTER TABLE main_block."Mode_cros_section"
    OWNER TO postgres;
---------------------------------------------------------------------

CREATE TABLE main_block."Settings_number"
(
    "Id_rcm" integer,
    "Id_setting" integer,
    "Id_ms" serial,
	PRIMARY KEY ("Id_ms"),
    FOREIGN KEY ("Id_rcm")
        REFERENCES main_block."Mode" ("Id_rcm") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

ALTER TABLE main_block."Settings_number"
    OWNER to postgres;
-------------------------------------------------------------------------

CREATE TABLE main_block."Reg_pars"
(
    "Id_rcm" integer NOT NULL,
    "Id_param" integer NOT NULL,
    value_string character varying(100),
    value_number numeric(14,7),
    PRIMARY KEY ("Id_rcm", "Id_param"),
    FOREIGN KEY ("Id_param")
        REFERENCES main_block."Parametrs" (id_param) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
   FOREIGN KEY ("Id_rcm")
        REFERENCES main_block."Mode" ("Id_rcm") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

ALTER TABLE main_block."Reg_pars"
    OWNER to postgres;
-------------------------------------------------------------------
CREATE TABLE main_block."Setting_cros_section"
(
    "Id_ms" integer NOT NULL,
    "id_cros_section" integer NOT NULL,
    "Id_sec" serial,
    PRIMARY KEY ("Id_sec"),
	FOREIGN KEY ("Id_ms")
        REFERENCES main_block."Settings_number" ("Id_ms") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

ALTER TABLE main_block."Setting_cros_section"
    OWNER to postgres;	

---------------------------------------------------------------------

CREATE OR REPLACE VIEW main_block."Mode_setting_cros_section"
 AS
 SELECT m."Id_R_C",
    m."Id_mode",
    n."Id_setting",
    c.id_cros_section,
    c."Id_sec"
   FROM main_block."Mode" m
     JOIN main_block."Settings_number" n ON m."Id_rcm" = n."Id_rcm"
     JOIN main_block."Setting_cros_section" c ON c."Id_ms" = n."Id_ms";

ALTER TABLE main_block."Mode_setting_cros_section"
    OWNER TO postgres;

---------------------------------------------------------------------
CREATE TABLE main_block."Settings_values"
(
    "Id_ms" integer NOT NULL,
    "Id_param" integer NOT NULL,
    value_string character varying(100),
    value_number numeric(14,7),
    PRIMARY KEY ("Id_ms", "Id_param"),
    FOREIGN KEY ("Id_param")
        REFERENCES main_block."Parametrs" (id_param) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
   FOREIGN KEY ("Id_ms")
        REFERENCES main_block."Settings_number" ("Id_ms") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

ALTER TABLE main_block."Settings_values"
    OWNER to postgres;
----------------------------------------------------------------------
--таблица с параметрами моделирования--
CREATE TABLE main_block."Parametrs_modelling"
(
    "Id#" serial NOT NULL,
    "Id_R_C" integer NOT NULL,
    "Id_param" integer NOT NULL,
    id_data integer NOT NULL,
    PRIMARY KEY ("Id#"),
    FOREIGN KEY ("Id_R_C")
        REFERENCES main_block."Realization_channel" ("Id_R_C") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID,
    FOREIGN KEY ("Id_param")
        REFERENCES main_block."Parametrs" (id_param) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID,
    FOREIGN KEY (id_data)
        REFERENCES main_block."Data_type" (id_data) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE main_block."Parametrs_modelling"
    OWNER to postgres;
------------------------------------------------------------------
--Таблица с результатами моделирования и их обработкой--
CREATE TABLE main_block."Values_modelling"
(
	id serial not null,
    "id_obr0/rez1" integer NOT NULL,
    "Id_sec" integer NOT NULL,
    "Id#" integer NOT NULL,
    id_traversing integer NOT NULL,
    value_string character varying(100),
    value_number numeric(14, 7),
    date timestamp with time zone NOT NULL  DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY ("id"),
    FOREIGN KEY ("Id#")
        REFERENCES main_block."Parametrs_modelling" ("Id#") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID,
	FOREIGN KEY ("Id_sec")
        REFERENCES main_block."Setting_cros_section" ("Id_sec") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE main_block."Values_modelling"
    OWNER to postgres;

------------------------------------------------------------------
--таблица с параметрами эксперимента--
CREATE TABLE main_block."Parametrs_experiment"
(
    "Id#" serial NOT NULL,
    "Id_R_C" integer NOT NULL,
    "Id_param" integer NOT NULL,
    id_data integer NOT NULL,
    PRIMARY KEY ("Id#"),
    FOREIGN KEY ("Id_R_C")
        REFERENCES main_block."Realization_channel" ("Id_R_C") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID,
    FOREIGN KEY ("Id_param")
        REFERENCES main_block."Parametrs" (id_param) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID,
    FOREIGN KEY (id_data)
        REFERENCES main_block."Data_type" (id_data) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE main_block."Parametrs_experiment"
    OWNER to postgres;
------------------------------------------------------------------
--Таблица с результатами эксперимента и их обработкой--
CREATE TABLE main_block."Values_experiment"
(
	id serial not null,
    "id_obr0/rez1" integer NOT NULL,
    "Id_m_c" integer NOT NULL,
    "Id#" integer NOT NULL,
    id_traversing integer NOT NULL,
    value_string character varying(100),
    value_number numeric(14, 7),
    date timestamp with time zone NOT NULL  DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY ("id"),
    FOREIGN KEY ("Id#")
        REFERENCES main_block."Parametrs_experiment" ("Id#") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID,
	FOREIGN KEY ("Id_m_c")
        REFERENCES main_block."Cros_section" ("Id_m_c") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE main_block."Values_experiment"
    OWNER to postgres;

------------------------------------------------------------------
--- окончание создания таблиц ----

---функции и процедуры-----
----
--функция возврата уникального номера задачи по 3ем параметрам
CREATE OR REPLACE function main_block.select_ID(php text,tpe text,ar1 text)
returns integer
AS $BODY$
select "ID*" from main_block."Experiment_class" 
				where 
				id_subarea in (select id_subarea from main_block."Areas_tree" where name_subarea=ar1)
				and 
				"id_Ph.p._T.p.e." in (select "id_Ph.p._T.p.e." from main_block."Ph.p._T.p.e."
					where
					"id_Ph.p."=(select "id_Ph.p." from main_block."Physical_process" where name=php)
					and 
					"id_T.p.e."=(select "id_T.p.e." from main_block."Type_of_power_equipment" where name=tpe)
				) 
$BODY$
LANGUAGE sql;

--Функция поиска названия класса задачи и соответствующих графических материалов по ID*--
CREATE OR REPLACE function main_block.select_names_ID(id_ integer)

returns table 
(Physical_process character varying(100),
 Type_of_power_equipment character varying(100),
 name_areas character varying(100),
 Main_pict character varying(100),
 Geom_pict character varying(100),
 Reg_pict character varying(100),
 Tepl_pict character varying(100))
AS $BODY$
select  p.name,t.name,a.name_subarea,e."Main_pict",e."Geom_pict",e."Reg_pict",e."Tepl_pict" from 
main_block."Physical_process" p join main_block."Ph.p._T.p.e." pt on pt."id_Ph.p."=p."id_Ph.p."
join main_block."Type_of_power_equipment" t on t."id_T.p.e."=pt."id_T.p.e."
join main_block."Experiment_class" e on e."id_Ph.p._T.p.e."=pt."id_Ph.p._T.p.e."
join main_block."Areas_tree" a on a.id_subarea=e.id_subarea where "ID*"=id_;
$BODY$
LANGUAGE sql;
--select* from main_block.select_names_ID(1);

--Функция вывода всех физических процессов--
CREATE OR REPLACE function main_block.select_name_php()

returns table (name character varying(100))
AS $BODY$
SELECT  name FROM main_block."Physical_process"
$BODY$
LANGUAGE sql;
--select * from main_block.select_name_php();
/**/

--Функция вывода типов энергетического оборудования по названию физического процсса--
CREATE OR REPLACE function main_block.select_name_tpe(php text)

returns table (name character varying(100))
AS $BODY$
SELECT  name FROM main_block."Type_of_power_equipment"
where "id_T.p.e." in (select "id_T.p.e." from main_block."Ph.p._T.p.e."
where "id_Ph.p."=(select "id_Ph.p." from main_block."Physical_process" where name=php))
$BODY$
LANGUAGE sql;
--select * from main_block.select_name_tpe('Гидрогазодинамика');
/**/

--Функция вывода названия экспериментального объекта по физическому процессу и типу энергетического оборудования--
CREATE OR REPLACE function main_block.select_name_subarea(php text,tpe text,areaname text)

returns table (name character varying(100))
AS $BODY$
select name_subarea from main_block."Areas_tree" 
where id_area=(select id_area from main_block."Area"
			   where name_area=areaname) 
	  and id_subarea in (select id_subarea from main_block."Experiment_class" 
						 where "id_Ph.p._T.p.e."= (select "id_Ph.p._T.p.e." from main_block."Ph.p._T.p.e." 
												   where 
												   "id_Ph.p."=(select "id_Ph.p." from main_block."Physical_process" where name=php)
												   and 
												   "id_T.p.e."=(select "id_T.p.e." from main_block."Type_of_power_equipment" where name=tpe)
												  ) ) ;
$BODY$
LANGUAGE sql;
--select * from main_block.select_name_subarea('Гидрогазодинамика','Теплообменники','Область расчетного случая');
/**/

--Функция вывода названий параметров по их типу--
CREATE OR REPLACE function main_block.select_parametrs(type_par text)

returns table ( name_param text, short_name_param text, unit_param text)
AS $BODY$
SELECT name_param, short_name_param, unit_param from
main_block."Parametrs" where 
id_type = (select id_type from main_block."Type_of_parametrs" where name=type_par);
$BODY$
LANGUAGE sql;

--select * from main_block.select_parametrs('Геометрические параметры');
--
--Процедуры добавления и обновления данных--
---
CREATE OR REPLACE PROCEDURE main_block.add_pict_geom(id_ integer,fname text)
AS $BODY$
UPDATE main_block."Experiment_class" SET "Geom_pict"=fname WHERE "ID*"=id_;
$BODY$
LANGUAGE sql;
---
CREATE OR REPLACE PROCEDURE main_block.add_pict_reg(id_ integer,fname text)
AS $BODY$
UPDATE main_block."Experiment_class" SET "Reg_pict"=fname WHERE "ID*"=id_;
$BODY$
LANGUAGE sql;
---
CREATE OR REPLACE PROCEDURE main_block.add_pict_tepl(id_ integer,fname text)
AS $BODY$
UPDATE main_block."Experiment_class" SET "Tepl_pict"=fname WHERE "ID*"=id_;
$BODY$
LANGUAGE sql;
---
CREATE OR REPLACE PROCEDURE main_block.add_pict_main(id_ integer,fname text)
AS $BODY$
UPDATE main_block."Experiment_class" SET "Main_pict"=fname WHERE "ID*"=id_;
$BODY$
LANGUAGE sql;
---
CREATE OR REPLACE PROCEDURE main_block.insert_name_php(name_ text)
AS $BODY$
INSERT INTO main_block."Physical_process"(name)
	VALUES (name_);
$BODY$
LANGUAGE 'sql';
---
CREATE OR REPLACE procedure main_block.insert_name_tpe(name_ text)
AS $BODY$
INSERT INTO main_block."Type_of_power_equipment"(name)
	VALUES (name_);
$BODY$
LANGUAGE sql;
---
CREATE OR REPLACE procedure main_block.insert_name_areastree(name_ text)
AS $BODY$
INSERT INTO main_block."Areas_tree"(id_area,name_subarea,id_parent_area)
	VALUES (1,name_,0);
$BODY$
LANGUAGE sql;
---
CREATE OR REPLACE PROCEDURE main_block.insert_PhpTpe(php text,tpe text)
AS $BODY$
INSERT INTO main_block."Ph.p._T.p.e."(
	"id_Ph.p.", "id_T.p.e.")
	VALUES (
		(select "id_Ph.p." from main_block."Physical_process" where name=php)
		,
		(select "id_T.p.e." from main_block."Type_of_power_equipment" where name=tpe)
	);
$BODY$
LANGUAGE 'sql';

--call main_block.insert_PhpTpe('Гидрогазодинамика','Теплообменники');
--
CREATE OR REPLACE PROCEDURE main_block.insert_Experiment(php text,tpe text,ar text)
AS $BODY$
INSERT INTO main_block."Experiment_class"(
	"id_Ph.p._T.p.e.", id_subarea)
	VALUES (
		(select "id_Ph.p._T.p.e." from main_block."Ph.p._T.p.e." p 
		 join main_block."Physical_process" ph on  p."id_Ph.p."=ph."id_Ph.p."
		 join main_block."Type_of_power_equipment" t on p."id_T.p.e."=t."id_T.p.e."
		where ph.name= php and t.name= tpe)
		,
		(select "id_subarea" from main_block."Areas_tree" where name_subarea=ar and id_area=1)
	);
	
$BODY$
LANGUAGE 'sql';
--call main_block.insert_Experiment('Прочность','Теплообменники','Сопловые каналы');
--

--Функция вывода количества комбинаций физического процесса и типов энергетического оборудования--
CREATE OR REPLACE function main_block.exist_php_tpe(php text,tpe text)
returns bigint
AS $BODY$
select count(*) from main_block."Ph.p._T.p.e." p 
		 join main_block."Physical_process" ph on  p."id_Ph.p."=ph."id_Ph.p."
		 join main_block."Type_of_power_equipment" t on p."id_T.p.e."=t."id_T.p.e."
		where ph.name= php and t.name= tpe;
$BODY$
LANGUAGE sql;
--select main_block.exist_php_tpe('Гидрогазодинамика','Котельные установки')
--

--Функция проверки наличия класса задачи--
CREATE OR REPLACE function main_block.exist_experiment_class(php text,tpe text,ar1 text)
returns bigint
AS $BODY$
select count(*) from main_block."Ph.p._T.p.e." p 
		 join main_block."Physical_process" ph on  p."id_Ph.p."=ph."id_Ph.p."
		 join main_block."Type_of_power_equipment" t on p."id_T.p.e."=t."id_T.p.e."
		 join main_block."Experiment_class" e on e."id_Ph.p._T.p.e."=p."id_Ph.p._T.p.e."
		 join main_block."Areas_tree" a on a.id_subarea=e.id_subarea
		where ph.name= php and t.name= tpe and a.name_subarea=ar1;
$BODY$
LANGUAGE sql;

--select main_block.exist_experiment_class('Гидрогазодинамика','Паровые и газовые турбины','Сопловые каналы');
--

--Функция вывода возможных значений строкового параметра по его названию--
CREATE OR REPLACE function main_block.select_string_values(nam text)
returns table 
(name character varying(100))
AS $BODY$
SELECT value
	FROM main_block."String_values" s join main_block."Parametrs" p on p.id_param=s.id_param
	where name_param=nam;
$BODY$	
LANGUAGE sql;
--select * from main_block.select_string_values('Среда');
--

--Процедура добавления нового параметра--
CREATE OR REPLACE PROCEDURE main_block.insert_parametrs(nam_t text,nam_p text,short_nam text,unit_par text)
AS $BODY$
INSERT INTO main_block."Parametrs"(
	id_type, name_param, short_name_param, unit_param)
	VALUES ((select id_type FROM main_block."Type_of_parametrs" where name=nam_t),
			nam_p,
			short_nam,
			unit_par
		   );
	
$BODY$
LANGUAGE 'sql';

--call main_block.insert_parametrs('Геометрические параметры','Ширина','d','мм');
--

--процедура добавления вариантов строковы параметров--
CREATE OR REPLACE PROCEDURE main_block.insert_string_values(namepar text,value_ text)
as $BODY$
insert into main_block."String_values"(id_param,value)
values (
(select id_param from main_block."Parametrs" where name_param=namepar),
	value_
);
$BODY$
Language 'sql';
---


-- выборка геометрических параметров для поиска по номеру канала и номеру классса задачи
CREATE OR REPLACE function main_block.exp_search_geom(id_ integer,id_chan integer)
returns table 
(name_param character varying(100),
 short_name_param character varying(100),
 unit_param character varying(100))
AS $BODY$
SELECT distinct "name_param","short_name_param", "unit_param"  from
main_block."Realization_channel" r join main_block."Geometric_parametrs" g on  r."Id_R_C"=g."Id_R_C"
join main_block."Parametrs" p on p."id_param" = g."id_param" where 
r."Channel" = id_chan and
"Id$" = (select "Id$" from main_block."Stand_ID*" where "ID*"=id_) ;
		
$BODY$	
LANGUAGE sql;

--select * from main_block."exp_search_geom"(1,1);

--вывод данных результатов эксперимента проведенных в определенном канале
CREATE OR REPLACE function main_block.select_chan_results(Id_obj integer,realiz integer, chan integer, obr_res integer)
returns table 
("Id_mode" integer,
 id_cros_section integer,
 id_traversing integer,
 id_type integer,
 "name_param" character varying(100),
 short_name_param character varying(100),
 unit_param character varying(100),
 value_string character varying(100),
 value_namber numeric,
 id_data integer)
AS $BODY$
SELECT mcs."Id_mode", id_cros_section, id_traversing,id_type, "name_param" ,"short_name_param", "unit_param", value_string, value_number , id_data
	FROM main_block."Values_experiment" v  join main_block."Parametrs_experiment" pe on pe."Id#"=v."Id#"
	join main_block."Mode_cros_section" mcs on mcs."Id_m_c"=v."Id_m_c"
	join main_block."Parametrs" p on p."id_param" = pe."Id_param" 
	where "id_obr0/rez1"=obr_res and
	v."Id_m_c" in (select "Id_m_c" from main_block."Mode_cros_section" where "Id_R_C" = 
			   (select "Id_R_C" from main_block."Realization_channel" where "Id$"=Id_obj and "Realization" = realiz and "Channel" = chan))
	order by mcs."Id_mode", id_cros_section, id_traversing, "name_param";
		
$BODY$	
LANGUAGE sql;

--select * from main_block."select_chan_results"(6,1,1,1);


-- выборка геометрических параметров для поиска по номеру канала и номеру классса задачи
CREATE OR REPLACE function main_block.exp_search_geom(id_ integer,id_chan integer)
returns table 
(name_param character varying(100),
 short_name_param character varying(100),
 unit_param character varying(100))
AS $BODY$
SELECT distinct "name_param","short_name_param", "unit_param"  from
main_block."Realization_channel" r join main_block."Geometric_parametrs" g on  r."Id_R_C"=g."Id_R_C"
join main_block."Parametrs" p on p."id_param" = g."id_param" where 
r."Channel" = id_chan and
"Id$" = (select "Id$" from main_block."Stand_ID*" where "ID*"=id_) ;
		
$BODY$	
LANGUAGE sql;

--select * from main_block."exp_search_geom"(1,1);

--вывод данных результатов эксперимента проведенных в определенном канале
CREATE OR REPLACE function main_block.select_chan_results(Id_obj integer,realiz integer, chan integer, obr_res integer)
returns table 
("Id_mode" integer,
 id_cros_section integer,
 id_traversing integer,
 id_type integer,
 "name_param" character varying(100),
 short_name_param character varying(100),
 unit_param character varying(100),
 value_string character varying(100),
 value_namber numeric(14,7),
 id_data integer)
AS $BODY$
SELECT mcs."Id_mode", id_cros_section, id_traversing,id_type, "name_param" ,"short_name_param", "unit_param", value_string, value_number , id_data
	FROM main_block."Values_experiment" v  join main_block."Parametrs_experiment" pe on pe."Id#"=v."Id#"
	join main_block."Mode_cros_section" mcs on mcs."Id_m_c"=v."Id_m_c"
	join main_block."Parametrs" p on p."id_param" = pe."Id_param" 
	where "id_obr0/rez1"=obr_res and
	v."Id_m_c" in (select "Id_m_c" from main_block."Mode_cros_section" where "Id_R_C" = 
			   (select "Id_R_C" from main_block."Realization_channel" where "Id$"=Id_obj and "Realization" = realiz and "Channel" = chan))
	order by mcs."Id_mode", id_cros_section, id_traversing, "name_param";
		
$BODY$	
LANGUAGE sql;

--select * from main_block."select_chan_results"(6,1,1,1);

--выборка настроек определенного режима
CREATE OR REPLACE function main_block.select_settings_values(id_rcm_ integer)

returns table 
(
 setting_numbers character varying(1000),
 id_type integer,
 name_param character varying(100),
 array_par_number character varying(1000),
 array_par_string character varying(1000),
 drop_list character varying(1000)
)
AS $BODY$
with s1 as
(select 
 array_to_string(array_agg("Id_setting" ORDER BY "Id_setting"),',') as setting_numbers,
 name_param,
 array_to_string(ARRAY_AGG(value_string ORDER BY s."Id_ms"),',') as value_str,
 array_to_string (ARRAY_AGG(value_number ORDER BY s."Id_ms"),',') as value_num,id_type 
	FROM main_block."Settings_values" s join main_block."Parametrs" p  on p.id_param = s."Id_param" 
 	join main_block."Settings_number" n on n."Id_ms"=s."Id_ms"
	where s."Id_ms" in (select v."Id_ms" from main_block."Settings_number" n join main_block."Settings_values" v on v."Id_ms" = n."Id_ms" where "Id_rcm" = id_rcm_ )
	group by name_param, id_type),
s2 as
(SELECT name_param, array_to_string(Array_agg(distinct value),',') as drop_list, id_type 
	FROM main_block."Settings_values" s join main_block."Parametrs" p  on p.id_param = s."Id_param" 
	left join main_block."String_values" v on v.id_param = p.id_param
    where "Id_ms" in (select v."Id_ms" from main_block."Settings_number" n join main_block."Settings_values" v on v."Id_ms" = n."Id_ms" where "Id_rcm" = id_rcm_ )
	group by name_param, id_type)
	
select s1.setting_numbers, s1.id_type,s1.name_param, value_num, value_str, drop_list from s1 join s2
on s1.name_param = s2.name_param

$BODY$
LANGUAGE sql;

--выборка результатов моделирования
CREATE OR REPLACE FUNCTION main_block.select_chan_results_modelling(
	id_obj_ integer,
	id_mode_ integer,
	realiz_ integer,
	chan_ integer,
	obr_res_ integer)
    RETURNS TABLE("Id_setting" integer, id_cros_section integer, id_traversing integer, id_type integer, name_param character varying, short_name_param character varying, unit_param character varying, value_string character varying, value_namber numeric, id_data integer) 
    LANGUAGE 'sql'

AS $BODY$
SELECT mscs."Id_setting", "id_cros_section", id_traversing,id_type, "name_param" ,"short_name_param", "unit_param", value_string, value_number , id_data
	FROM main_block."Values_modelling" v  join main_block."Parametrs_modelling" pe on pe."Id#"=v."Id#"
	join main_block."Mode_setting_cros_section" mscs on mscs."Id_sec"=v."Id_sec"
	join main_block."Parametrs" p on p."id_param" = pe."Id_param" 
	where "id_obr0/rez1"=obr_res_ and
	v."Id_sec" in (select "Id_sec" from main_block."Mode_setting_cros_section" where 
				   "Id_R_C" = (select "Id_R_C" from main_block."Realization_channel" where "Id$"=id_obj_ and "Realization" = realiz_ and "Channel" =chan_)
				   and "Id_mode" = id_mode_)
	order by mscs."Id_setting", "id_cros_section", id_traversing, "name_param";

$BODY$;

ALTER FUNCTION main_block.select_chan_results_modelling(integer, integer, integer, integer, integer)
    OWNER TO postgres;

--выборка результатов эксперимента(для заполнения моделирования)
CREATE OR REPLACE FUNCTION main_block.select_chan_mode_exp_results(
	id_obj_ integer,
	realiz_ integer,
	chan_ integer,
	id_mode_ integer,
	obr_res_ integer)
    RETURNS TABLE("Id_mode" integer, id_cros_section integer, id_traversing integer, id_type integer, name_param character varying, short_name_param character varying, unit_param character varying, value_string character varying, value_namber numeric, id_data integer) 
    LANGUAGE 'sql'
    COST 100
    VOLATILE PARALLEL UNSAFE
    ROWS 1000

AS $BODY$
SELECT mcs."Id_mode", id_cros_section, id_traversing,id_type, "name_param" ,"short_name_param", "unit_param", value_string, value_number , id_data
	FROM main_block."Values_experiment" v  join main_block."Parametrs_experiment" pe on pe."Id#"=v."Id#"
	join main_block."Mode_cros_section" mcs on mcs."Id_m_c"=v."Id_m_c"
	join main_block."Parametrs" p on p."id_param" = pe."Id_param" 
	where "id_obr0/rez1"=obr_res_ and
	v."Id_m_c" in (select "Id_m_c" from main_block."Mode_cros_section" where "Id_mode"=id_mode_ and "Id_R_C" = 
			   (select "Id_R_C" from main_block."Realization_channel" where "Id$"=id_obj_ and "Realization" = realiz_ and "Channel" = chan_))
	order by mcs."Id_mode", id_cros_section, id_traversing, "name_param";
$BODY$;

ALTER FUNCTION main_block.select_chan_mode_exp_results(integer, integer,integer, integer, integer)
    OWNER TO postgres;

--copy main_block."Parametrs_modelling" ("Id_R_C","Id_param", "id_data") FROM 'C:\Scripts\Loading data\parametrs_experiment.txt' DELIMITER E'\t' ENCODING 'UTF8';
--copy main_block."Values_modelling" ("id_obr0/rez1","Id_m_c", "Id#", "id_traversing", "value_string", "value_number") FROM 'C:\Scripts\Loading data\values_modelling.txt' DELIMITER E'\t' ENCODING 'UTF8';

--выборка режимных параметров моделирования
CREATE OR REPLACE FUNCTION main_block.select_Reg_pars(
	id_r_c_ integer)
    RETURNS TABLE(id_mode integer, name_param character varying, value_number numeric, value_string character varying) 
    LANGUAGE 'sql'
    COST 100
    VOLATILE PARALLEL UNSAFE
    ROWS 1000

AS $BODY$
select m."Id_mode", p.name_param, r.value_number, r.value_string from 
main_block."Mode" m
join main_block."Reg_pars" r on r."Id_rcm" = m."Id_rcm"
join main_block."Parametrs" p on p.id_param = r."Id_param"
where m."Id_R_C" = id_r_c_
order by m."Id_mode"
$BODY$;

ALTER FUNCTION main_block.select_Reg_pars(integer)
    OWNER TO postgres;

--вставка данных в результаты моделирования
CREATE OR REPLACE PROCEDURE main_block.insert_values_mod(
	obr0_rez1_ integer,
	rezh_ integer,
	setting_ integer,
	sec_ integer,
	id_r_c_ integer,
	id_data_ integer,
	par_name_ text,
	traver_ integer,
	value_n numeric,
	value_s text)
LANGUAGE 'plpgsql'
AS $BODY$
begin
	if id_data_=1 then
		INSERT INTO main_block."Values_modelling"("id_obr0/rez1", "Id_sec", "Id#", id_traversing, value_number)
		VALUES (
		obr0_rez1_,
		(select "Id_sec" from main_block."Mode_setting_cros_section" where "Id_mode" = rezh_ and id_cros_section = sec_ and "Id_R_C" = id_r_c_ and "Id_setting" = setting_),
		(select "Id#" from main_block."Parametrs_modelling" where Id_data = id_data_ and "Id_param" = (select id_param from main_block."Parametrs" where "name_param" = par_name_) and "Id_R_C" = id_r_c_),
		traver_,
		value_n); 
	else
		if id_data_=4 then
			INSERT INTO main_block."Values_modelling"("id_obr0/rez1", "Id_sec", "Id#", id_traversing, value_number, value_string)
			VALUES (
			obr0_rez1_,
			(select "Id_sec" from main_block."Mode_setting_cros_section" where "Id_mode" = rezh_ and id_cros_section = sec_ and "Id_R_C" = id_r_c_ and "Id_setting" = setting_),
			(select "Id#" from main_block."Parametrs_modelling" where Id_data = id_data_ and "Id_param" = (select id_param from main_block."Parametrs" where "name_param" = par_name_) and "Id_R_C" = id_r_c_),
			traver_,
			value_n,
			value_s); 
		else
			INSERT INTO main_block."Values_modelling"("id_obr0/rez1", "Id_sec", "Id#", id_traversing, value_string)
			VALUES (
			obr0_rez1_,
			(select "Id_sec" from main_block."Mode_setting_cros_section" where "Id_mode" = rezh_ and id_cros_section = sec_ and "Id_R_C" = id_r_c_ and "Id_setting" = setting_),
			(select "Id#" from main_block."Parametrs_modelling" where Id_data = id_data_ and "Id_param" = (select id_param from main_block."Parametrs" where "name_param" = par_name_) and "Id_R_C" = id_r_c_),
			traver_,
			value_s); 
		end if;
	end if;
end;
$BODY$;
-----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION main_block.select_exp_rezh_params(
	id_r_c_ integer)
    RETURNS TABLE(id_mode integer, name_param character varying, value_number numeric, value_string character varying) 
    LANGUAGE 'sql'
    COST 100
    VOLATILE PARALLEL UNSAFE
    ROWS 1000

AS $BODY$
select m."Id_mode", p.name_param, v.value_number, v.value_string from 
main_block."Mode_cros_section" m 
join main_block."Values_experiment" v on v."Id_m_c" = m."Id_m_c"
join main_block."Parametrs_experiment" e on v."Id#"=e."Id#"
join main_block."Parametrs" p on p.id_param = e."Id_param"
where p.id_type = 2 and m."Id_R_C" = id_r_c_ and v."id_obr0/rez1" = 1
order by m."Id_mode"
$BODY$;

ALTER FUNCTION main_block.select_exp_rezh_params(integer)
    OWNER TO postgres;

-----------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE main_block.update_values_mod(
	obr0_rez1_ integer,
	rezh_ integer,
	sett_ integer,
	sec_ integer,
	id_r_c_ integer,
	id_data_ integer,
	par_name_ text,
	traver_ integer,
	value_n_ numeric,
	value_s_ text)
LANGUAGE 'plpgsql'
AS $BODY$
begin
	
	if id_data_=1 then
		UPDATE main_block."Values_modelling" SET
		value_number = value_n_,
		date = CURRENT_TIMESTAMP
		WHERE "id_obr0/rez1" = obr0_rez1_ and
		"Id_sec" = (select "Id_sec" from main_block."Mode_setting_cros_section" where "Id_mode" = rezh_ and id_cros_section = sec_ and "Id_R_C" = id_r_c_ and "Id_setting" = sett_) and
		"Id#" = (select "Id#" from main_block."Parametrs_modelling" where Id_data = id_data_ and "Id_param" = (select id_param from main_block."Parametrs" where "name_param" = par_name_) and "Id_R_C" = id_r_c_) and 
		id_traversing = traver_; 
	else
		if id_data_=4 then
			UPDATE main_block."Values_modelling" SET 
			value_string = value_s_,
			value_number = value_n_,
			date = CURRENT_TIMESTAMP
			WHERE "id_obr0/rez1" = obr0_rez1_ and
			"Id_sec" = (select "Id_sec" from main_block."Mode_setting_cros_section" where "Id_mode" = rezh_ and id_cros_section = sec_ and "Id_R_C" = id_r_c_ and "Id_setting" = sett_) and
			"Id#" = (select "Id#" from main_block."Parametrs_modelling" where Id_data = id_data_ and "Id_param" = (select id_param from main_block."Parametrs" where "name_param" = par_name_) and "Id_R_C" = id_r_c_) and 
			id_traversing = traver_; 
		else
			UPDATE main_block."Values_modelling" SET 
			value_string = value_s_,
			date = CURRENT_TIMESTAMP
			WHERE "id_obr0/rez1" = obr0_rez1_ and
			"Id_sec" = (select "Id_sec" from main_block."Mode_setting_cros_section" where "Id_mode" = rezh_ and id_cros_section = sec_ and "Id_R_C" = id_r_c_ and "Id_setting" = sett_) and
			"Id#" = (select "Id#" from main_block."Parametrs_modelling" where Id_data = id_data_ and "Id_param" = (select id_param from main_block."Parametrs" where "name_param" = par_name_) and "Id_R_C" = id_r_c_) and 
			id_traversing = traver_; 
		end if;
	end if;
end;
$BODY$;
-----------------------------------------------------------------------------

-----------------------------------------------------------------------------



-- фильтрация номеров исполнений по введенному предыдущему параметру

CREATE OR REPLACE FUNCTION main_block.experiment_filter(
id_ integer,	-- ID*
chan1 integer,	-- номер канала введенного (предыдущего) параметра
chan2 integer,	-- номер канала для искомого (текущего) параметра
rang integer[],	-- массив номеров исполнений предыдущего параметра
par1 text,		-- название предыдущего параметра
par1_val numeric,	-- значение предыдущего параметра
par2 text)		-- название текущего параметра
RETURNS TABLE(realiz integer, par2_val numeric)
LANGUAGE 'sql'

COST 100
VOLATILE
ROWS 1000

AS $BODY$

with P1 as (select rc."Realization" from main_block."Realization_channel" rc join main_block."Geometric_parametrs" gp
on rc."Id_R_C"=gp."Id_R_C" where rc."Channel"=chan1 and rc."Id$"=(select "Id$" from main_block."Stand_ID*" where "ID*"=id_) and
gp."id_param" = (select id_param from main_block."Parametrs" where name_param = par1) and gp."value_number" = par1_val and array[rc."Realization"] <@ rang),

P2 as (select rc."Realization", gp."value_number" from main_block."Realization_channel" rc join main_block."Geometric_parametrs" gp
on rc."Id_R_C"=gp."Id_R_C" where rc."Channel"=chan2 and rc."Id$"=(select "Id$" from main_block."Stand_ID*" where "ID*"=id_) and
gp."id_param" = (select id_param from main_block."Parametrs" where name_param = par2))

select P1."Realization", P2."value_number" from P1 join P2 on P1."Realization" = P2."Realization";

$BODY$;

--процедура вставки в Values_experiment
--(обработка/результат) (режим) (сечение) (id_r_c_) (тип данных) (название параметра) (траверсирование) (значение) (строка)

CREATE OR REPLACE PROCEDURE main_block.insert_values_exp(obr0_rez1_ int,rezh_ int, sec_ int, id_r_c_ int, id_data_ int, par_name_ text,traver_ int, value_n_ numeric(14,7), value_s_ text)
as $BODY$
begin
	if id_data_=1 then
		INSERT INTO main_block."Values_experiment"("id_obr0/rez1", "Id_m_c", "Id#", id_traversing, value_number)
		VALUES (
		obr0_rez1_,
		(select "Id_m_c" from main_block."Mode_cros_section" where "Id_mode" = rezh_ and id_cros_section = sec_ and "Id_R_C" = id_r_c_),
		(select "Id#" from main_block."Parametrs_experiment" where Id_data = id_data_ and "Id_param" = (select id_param from main_block."Parametrs" where "name_param" = par_name_) and "Id_R_C" = id_r_c_),
		traver_,
		value_n_); 
	else
		if id_data_=4 THEN
			INSERT INTO main_block."Values_experiment"("id_obr0/rez1", "Id_m_c", "Id#", id_traversing, value_number, value_string)
			VALUES (
			obr0_rez1_,
			(select "Id_m_c" from main_block."Mode_cros_section" where "Id_mode" = rezh_ and id_cros_section = sec_ and "Id_R_C" = id_r_c_),
			(select "Id#" from main_block."Parametrs_experiment" where Id_data = id_data_ and "Id_param" = (select id_param from main_block."Parametrs" where "name_param" = par_name_) and "Id_R_C" = id_r_c_),
			traver_,
			value_n_,
			value_s_);
		else
			INSERT INTO main_block."Values_experiment"("id_obr0/rez1", "Id_m_c", "Id#", id_traversing, value_string)
			VALUES (
			obr0_rez1_,
			(select "Id_m_c" from main_block."Mode_cros_section" where "Id_mode" = rezh_ and id_cros_section = sec_ and "Id_R_C" = id_r_c_),
			(select "Id#" from main_block."Parametrs_experiment" where Id_data = id_data_ and "Id_param" = (select id_param from main_block."Parametrs" where "name_param" = par_name_) and "Id_R_C" = id_r_c_),
			traver_,
			value_s_); 
		end if;
	end if;
end;

$BODY$
Language plpgsql;

--call main_block.insert_values_exp(1,1, 0, 1, 1, 'Число Рейнольдса',0, '60606',Null);

--процедура обновления записи в таблице Values_experiment
--(обработка/результат) (режим) (сечение) (id_r_c_) (тип данных) (название параметра) (траверсирование) (значение) (строка)

CREATE OR REPLACE PROCEDURE main_block.update_values_exp(obr0_rez1_ int,rezh_ int, sec_ int, id_r_c_ int, id_data_ int, par_name_ text,traver_ int, value_n_ numeric(14,7), value_s_ text)
as $BODY$
begin
	
	if id_data_=1 then
		UPDATE main_block."Values_experiment" SET
		value_number = value_n_,
		date = CURRENT_TIMESTAMP
		WHERE "id_obr0/rez1" = obr0_rez1_ and
		"Id_m_c" = (select "Id_m_c" from main_block."Mode_cros_section" where "Id_mode" = rezh_ and id_cros_section = sec_ and "Id_R_C" = id_r_c_) and 
		"Id#" = (select "Id#" from main_block."Parametrs_experiment" where Id_data = id_data_ and "Id_param" = (select id_param from main_block."Parametrs" where "name_param" = par_name_) and "Id_R_C" = id_r_c_) and 
		id_traversing = traver_; 
	else
		if id_data_=4 THEN
			UPDATE main_block."Values_experiment" SET
			value_number = value_n_,
			value_string = value_s_,
			date = CURRENT_TIMESTAMP
			WHERE "id_obr0/rez1" = obr0_rez1_ and
			"Id_m_c" = (select "Id_m_c" from main_block."Mode_cros_section" where "Id_mode" = rezh_ and id_cros_section = sec_ and "Id_R_C" = id_r_c_) and 
			"Id#" = (select "Id#" from main_block."Parametrs_experiment" where Id_data = id_data_ and "Id_param" = (select id_param from main_block."Parametrs" where "name_param" = par_name_) and "Id_R_C" = id_r_c_) and 
			id_traversing = traver_; 
		else
			UPDATE main_block."Values_experiment" SET 
			value_string = value_s_,
			date = CURRENT_TIMESTAMP
			WHERE "id_obr0/rez1" = obr0_rez1_ and
			"Id_m_c" = (select "Id_m_c" from main_block."Mode_cros_section" where "Id_mode" = rezh_ and id_cros_section = sec_ and "Id_R_C" = id_r_c_) and 
			"Id#" = (select "Id#" from main_block."Parametrs_experiment" where Id_data = id_data_ and "Id_param" = (select id_param from main_block."Parametrs" where "name_param" = par_name_) and "Id_R_C" = id_r_c_) and 
			id_traversing = traver_; 
		end if;
	end if;
end;

$BODY$
Language plpgsql;

--call main_block.update_values_exp(1,1, 0, 1, 1, 'Число Рейнольдса',0, 666.666,Null);

-- фильтрация номеров исполнений по введенному предыдущему параметру

CREATE OR REPLACE FUNCTION main_block.experiment_filter(
id_ integer,	-- ID*
chan1 integer,	-- номер канала введенного (предыдущего) параметра
chan2 integer,	-- номер канала для искомого (текущего) параметра
rang integer[],	-- массив номеров исполнений предыдущего параметра
par1 text,		-- название предыдущего параметра
par1_val numeric,	-- значение предыдущего параметра
par2 text)		-- название текущего параметра
RETURNS TABLE(realiz integer, par2_val numeric)
LANGUAGE 'sql'

COST 100
VOLATILE
ROWS 1000

AS $BODY$

with P1 as (select rc."Realization" from main_block."Realization_channel" rc join main_block."Geometric_parametrs" gp
on rc."Id_R_C"=gp."Id_R_C" where rc."Channel"=chan1 and rc."Id$"=(select "Id$" from main_block."Stand_ID*" where "ID*"=id_) and
gp."id_param" = (select id_param from main_block."Parametrs" where name_param = par1) and gp."value_number" = par1_val and array[rc."Realization"] <@ rang),

P2 as (select rc."Realization", gp."value_number" from main_block."Realization_channel" rc join main_block."Geometric_parametrs" gp
on rc."Id_R_C"=gp."Id_R_C" where rc."Channel"=chan2 and rc."Id$"=(select "Id$" from main_block."Stand_ID*" where "ID*"=id_) and
gp."id_param" = (select id_param from main_block."Parametrs" where name_param = par2))

select P1."Realization", P2."value_number" from P1 join P2 on P1."Realization" = P2."Realization";

$BODY$;

----------------------------------------------------------------------------------------------
CREATE OR REPLACE function main_block.test(mess text)

returns table 
("action_" character varying(100))
AS $BODY$
DECLARE
    _arr text[];
BEGIN
    SELECT regexp_split_to_array(mess, '_')
    INTO _arr;
return QUERY select action from main_block."Log_table" as l where 
	case  
	when mess like 'Update_exp_%' then 
		"action" = mess or
		"action" = 'Search_exp_'||_arr[3] or
		"action" like 'Search_FSA_'||_arr[3]||'_%' or
		"action" like 'Update_FSA_'||_arr[3]||'_%' or
		"action" = 'Update_soft'
		
	when mess like 'Update_FSA_%' then
		"action" = mess or
		"action" = 'Search_FSA_'||_arr[3]||'_'||_arr[4] or
		"action" = 'Update_exp_'||_arr[3] or
		"action" = 'Update_PC'
		
	when mess = 'Update_PC' then
		"action" = mess or
		"action" like 'Update_FSA_%'
		
	when mess = 'Update_soft' then
		"action" = mess or
		"action" like 'Update_exp_%'
		
	when mess = 'Update_users' or(mess like '%_online')  then
		"action" = mess
		
	when mess like 'Search_exp_%' then
		"action" = 'Update_exp_'||_arr[3]
		
	when mess like 'Search_FSA_%' then
		"action" = 'Update_exp_'||_arr[3] or
		"action" = 'Update_FSA_'||_arr[3]||'_'||_arr[4]
	end;
end
$BODY$
LANGUAGE plpgsql;
----------------------------------------------------------------------------------------------------
CREATE OR REPLACE procedure main_block.create_user(login_ text,psw_ text, lvl_ text)
AS $BODY$
begin
if lvl_ = '1' then execute 
	'CREATE ROLE '|| quote_ident(login_) ||' WITH
	LOGIN
	NOSUPERUSER
	NOCREATEDB
	CREATEROLE
	INHERIT
	NOREPLICATION
	CONNECTION LIMIT -1
	PASSWORD '|| quote_literal(psw_) ||';';execute format('GRANT "Users_LVL_1" TO %I',login_);
	else 
		if lvl_ = '2' then execute 
	'CREATE ROLE '|| quote_ident(login_) ||' WITH
	LOGIN
	NOSUPERUSER
	NOCREATEDB
	NOCREATEROLE
	INHERIT
	NOREPLICATION
	CONNECTION LIMIT -1
	PASSWORD '|| quote_literal(psw_) ||';';execute format('GRANT "Users_LVL_2" TO %I',login_);
		else
			if lvl_ = '3' then execute 
	'CREATE ROLE '|| quote_ident(login_) ||' WITH
	LOGIN
	NOSUPERUSER
	NOCREATEDB
	NOCREATEROLE
	INHERIT
	NOREPLICATION
	CONNECTION LIMIT -1
	PASSWORD '|| quote_literal(psw_) ||';';execute format('GRANT "Users_LVL_3" TO %I',login_);
		end if;
	end if;
end if;	 
end;
$BODY$
LANGUAGE plpgsql;

-------------------------------------------------------------------------------------------------------
Create or replace function main_block.user("log" text)
returns text
AS $BODY$
select substr(groname,11,1) as level_ from pg_group  join pg_user  on usesysid = ANY (grolist )
where 
usename = "log";
$BODY$
LANGUAGE sql;
-------------------------------------------------------------------------------------------------------



--- заполнение базы данных--
INSERT INTO main_block."Area" (
id_area, name_area) VALUES (
'0'::integer, 'Root'::character varying(100))
 returning id_area;
 
 INSERT INTO main_block."Areas_tree" (
id_area, id_subarea, name_subarea,id_parent_area) VALUES (
'0'::integer, '0'::integer, 'Root'::character varying(100),'0'::integer)
 returning id_subarea;
 
 INSERT INTO main_block."Stands" (
"Stand_id", name) VALUES (
'0'::integer, 'null'::character varying);
 
copy main_block."Physical_process" (name) FROM 'C:\Scripts\Loading data\Physical_process.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Type_of_power_equipment" (name) FROM 'C:\Scripts\Loading data\Type_of_power_equipment.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Ph.p._T.p.e." ("id_Ph.p.", "id_T.p.e.") FROM 'C:\Scripts\Loading data\Ph.p._T.p.e.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Area" ("name_area") FROM 'C:\Scripts\Loading data\Area.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Areas_tree" (id_area, name_subarea, id_parent_area) FROM 'C:\Scripts\Loading data\Areas_tree.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Experiment_class" ("id_Ph.p._T.p.e.", id_subarea,"Main_pict","Geom_pict","Reg_pict","Tepl_pict") FROM 'C:\Scripts\Loading data\Experiment_class.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Data_type" (name_data) FROM 'C:\Scripts\Loading data\data_type.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Type_of_parametrs" (name) FROM 'C:\Scripts\Loading data\type_of_parametrs.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Parametrs" (id_type, name_param, short_name_param, unit_param) FROM 'C:\Scripts\Loading data\parametrs.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."String_values" (id_param, value) FROM 'C:\Scripts\Loading data\string_values.txt' DELIMITER E'\t' ENCODING 'UTF8';
--copy main_block."Experiment" ("ID*",id_exp,id_param,id_data) FROM 'C:\Scripts\Loading data\experiments.txt' DELIMITER E'\t' ENCODING 'UTF8';
--copy main_block."Parametrs_values" (id_value,value_number,value_range,value_string) FROM 'C:\Scripts\Loading data\parametrs_values.txt' DELIMITER E'\t' ENCODING 'UTF8';
--copy main_block."Software configuration" (name,grid_gen,calc_module,type_license,license_term,workplaces,price) FROM 'C:\Scripts\Loading data\Software configuration.txt' DELIMITER E'\t' ENCODING 'UTF8';
--copy main_block."PC Configuration" (proc_name,proc_freq,proc_arch,num_cores,"amount_RAM","type_RAM",sys_cap,"GPU",price) FROM 'C:\Scripts\Loading data\PC Configuration.txt' DELIMITER E'\t' ENCODING 'UTF8';
--insert into main_block."Unic_EXP" ("ID*",id_exp) select  distinct "ID*",id_exp FROM main_block."Experiment"  order by "ID*",id_exp;
/*
copy main_block."Stands" ("name", "description", "scheme", "3d_model", "P&M") FROM 'C:\Scripts\Loading data\Stands.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Stand_ID*" ("ID*","Stand_id") FROM 'C:\Scripts\Loading data\Stand_ID.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Realization_channel" ("Id$","Realization", "Channel", "Id_R_C") FROM 'C:\Scripts\Loading data\Realization_channel.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Geometric_parametrs" ("Id_R_C","id_param", "value_number") FROM 'C:\Scripts\Loading data\Geometric_parametrs.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Mode_cros_section" ("Id_R_C","Id_mode", "id_cros_section") FROM 'C:\Scripts\Loading data\mode_cros_section.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Parametrs_experiment" ("Id_R_C","Id_param", "id_data") FROM 'C:\Scripts\Loading data\parametrs_experiment.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Values_experiment" ("id_obr0/rez1","Id_m_c", "Id#", "id_traversing", "value_string", "value_number") FROM 'C:\Scripts\Loading data\values_experiment.txt' DELIMITER E'\t' ENCODING 'UTF8';
*/






	