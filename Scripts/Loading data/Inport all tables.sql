INSERT INTO main_block."Area" (
id_area, name_area) VALUES (
'0'::integer, 'Root'::character varying(100))
 returning id_area;
 
 INSERT INTO main_block."Areas_tree" (
id_area, id_subarea, name_subarea,id_parent_area) VALUES (
'0'::integer, '0'::integer, 'Root'::character varying(100),'0'::integer)
 returning id_subarea;
 
copy main_block."Physical_process" (name) FROM 'C:\Scripts\Loading data\Physical_process.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Type_of_power_equipment" (name) FROM 'C:\Scripts\Loading data\Type_of_power_equipment.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Ph.p._T.p.e." ("id_Ph.p.", "id_T.p.e.") FROM 'C:\Scripts\Loading data\Ph.p._T.p.e.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Area" ("name_area") FROM 'C:\Scripts\Loading data\Area.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Areas_tree" (id_area, name_subarea, id_parent_area) FROM 'C:\Scripts\Loading data\Areas_tree.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Experiment_class" ("id_Ph.p._T.p.e.", id_subarea) FROM 'C:\Scripts\Loading data\Experiment_class.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Data_type" (name_data) FROM 'C:\Scripts\Loading data\data_type.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Type_of_parametrs" (name) FROM 'C:\Scripts\Loading data\type_of_parametrs.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."Parametrs" (id_type, name_param, short_name_param, unit_param) FROM 'C:\Scripts\Loading data\parametrs.txt' DELIMITER E'\t' ENCODING 'UTF8';
copy main_block."String_values" (id_param, value) FROM 'C:\Scripts\Loading data\string_values.txt' DELIMITER E'\t' ENCODING 'UTF8';
--copy main_block."Experiment" ("ID*",id_exp,id_param,id_data) FROM 'C:\5 semestr\DB project\Scripts\Loading data\experiments.txt' DELIMITER E'\t' ENCODING 'UTF8';