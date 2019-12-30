CREATE DATABASE Evator;
USE Evator;

create table accounts
(
    at_id int IDENTITY(1,1) not null,
    at_name varchar(50),
    employee_id int not null,
    department varchar(50),
    email varchar(50),
    at_password varchar(50),
    at_profile varchar(50),
    role_type tinyint not null,
    primary key (at_id)
);

create table at_token
(
    token char(64),
    at_id int not null,
    tn_status tinyint not null,
    primary key (token)
);

alter table at_token add constraint fk_tn_at foreign key (at_id) references accounts (at_id);

create table events
(
    et_id int IDENTITY(1,1) not null,
    owner_id int not null,
    et_name varchar(50),
    speaker varchar(50),
    et_location varchar(50),
    date_start date not null,
    date_end date not null,
    time_start time not null,
    time_end time not null,
    et_description varchar(250),
    qr_invite varchar(100),
    qr_attend varchar(100),
    banner varchar(100),
    et_status tinyint not null,
    primary key (et_id)
);

alter table events add constraint fk_et_at foreign key (owner_id) references accounts (at_id);

create table attend
(
    at_id int IDENTITY(1,1) not null,
    et_id int not null,
    at_id int not null,
    ad_record datetime not null
);

alter table attend add constraint fk_ad_at foreign key (at_id) references accounts(at_id);
alter table attend add constraint fk_ad_et foreign key (et_id) references events(et_id);

create table  invite
(
    ie_id int IDENTITY(1,1) not null,
    et_id int not null,
    at_id int not null
);

alter table invite add constraint fk_ie_at foreign key (at_id) references accounts(at_id);
alter table invite add constraint fk_ie_et foreign key (et_id) references events(et_id);